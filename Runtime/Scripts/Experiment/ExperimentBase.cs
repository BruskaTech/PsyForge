//Copyright (c) 2024 Columbia University (James Bruska)
//Copyright (c) 2024 Jefferson University (James Bruska)
//Copyright (c) 2024 Bruska Technologies LLC (James Bruska)
//Copyright (c) 2023 University of Pennsylvania (James Bruska)

//This file is part of PsyForge.
//PsyForge is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//PsyForge is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with PsyForge. If not, see <https://www.gnu.org/licenses/>. 

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

using PsyForge.Utilities;
using PsyForge.ExternalDevices;
using PsyForge.GUI;
using PsyForge.Extensions;
using PsyForge.DataManagement;

namespace PsyForge.Experiment {

    static class ExperimentActive {
        private static bool active = false;
        public static bool isActive() { return active; }
        public static void SetActive(bool val) {
            if (val && active) {
                throw new InvalidOperationException("Trying to make an experiment active when there is already an active experiment."
                    + "If you have more than one experiment, make sure to make them all inactive in the editor.");
            }
            active = val;
        }
    }

    public abstract class ExperimentBase<Self, SessionType, TrialType, Constants> : SingletonEventMonoBehaviour<Self>
        where Self : ExperimentBase<Self, SessionType, TrialType, Constants>
        where SessionType : ExperimentSession<TrialType>
        where Constants: ExperimentConstants, new()
    {
        protected readonly Constants CONSTANTS = new();

        protected InputManager inputManager;
        protected TextDisplayer textDisplayer;

        protected SessionType session;
        protected SessionType practiceSession;
        protected SessionType normalSession;
        protected bool noPracticeSession = false;

        protected new void Awake() {
            base.Awake();
            inputManager = InputManager.Instance;
            textDisplayer = TextDisplayer.Instance;
            LogExperimentInfo();
            LogConstants();
            LogConstantsAndConfigs();
            ReportSessionNum();

            DoTS(ExperimentQuit);
            DoTS(ExperimentPause);
            manager.syncBox?.StartContinuousPulsing();
        }

        protected void OnEnable() {
            ExperimentActive.SetActive(true);
        }

        protected void OnDisable() {
            ExperimentActive.SetActive(false);
        }

        /// <summary>
        /// Things run at the very beginning of the experiment.
        /// This is useful for setting up resources and doing initial things like microphone tests.
        /// </summary>
        /// <returns></returns>
        protected abstract Task InitialStates();
        /// <summary>
        /// Things to set up and run before the practice trials.
        /// Technically, anything done here can be done in InitialStates, but this is useful for organization.
        /// </summary>
        /// <returns></returns>
        protected virtual Task SetupPracticeTrials() { return Task.CompletedTask; }
        /// <summary>
        /// These are the practice trials.
        /// </summary>
        /// <returns></returns>
        protected abstract Task PracticeTrialStates();
        /// <summary>
        /// Things to set up and run before the experiment trials.
        /// </summary>
        /// <returns></returns>
        protected virtual Task SetupTrials() { return Task.CompletedTask; }
        /// <summary>
        /// These are the experiment trials.
        /// </summary>
        /// <returns></returns>
        protected abstract Task TrialStates();
        /// <summary>
        /// Things run at the very end of the experiment.
        /// This is useful for things like post-experiment questionnaires.
        /// </summary>
        /// <returns></returns>
        protected abstract Task FinalStates();


        protected void EndCurrentSession() {
            throw new EndSessionException();
        }

        protected void Run() {
            DoTS(RunHelper().ToEnumerator);
        }
        protected async Task RunHelper() {
            // Initilize experiment
            await InitialStates();

            // Run practice session
            session = practiceSession;
            try {
                await SetupPracticeTrials();
                session = practiceSession; // Done again in case the session was created in SetupPracticeTrials
                if (noPracticeSession) {
                    throw new EndSessionException();
                } else if (session == null) {
                    throw new Exception($"{GetType().Name} did not set a practice session."
                        + "\n\nEither assign a value to 'practiceSession' in your experiment or set 'noPracticeSession' to true in the experiment.");
                }
                while (true) {
                    await PracticeTrialStates();
                    session.TrialNum++;
                }
            } catch (EndSessionException) {} // do nothing

            // Run normal session
            session = normalSession;
            try {
                await SetupTrials();
                session = normalSession; // Done again in case the session was created in SetupTrials
                if (session == null) {
                    throw new Exception($"{GetType().Name} did not set a normal session."
                        + "\n\nAssign a value to 'normalSession' in your experiment.");
                }
                while (true) {
                    await TrialStates();
                    session.TrialNum++;
                }
            } catch (EndSessionException) {} // do nothing

            // Final Ssates and quit
            await FinalStates();
            await manager.QuitTS();
        }

        // Logging Functions
        protected virtual void LogExperimentInfo() {
            // Log versions and experiment info
            eventReporter.LogTS("session start", new() {
                { "application version", BuildInfo.ApplicationVersion() },
                { "experiment name", Config.experimentName },
                { "participant", Config.subject },
                { "session", Config.sessionNum },
                { "psyForge version", BuildInfo.PackageVersion() },
                { "unity version", BuildInfo.UnityVersion() },
                { "logfile version", "1.0.0" },
                { "build date", BuildInfo.BuildDateTime() },
                { "psyForge commit hash", BuildInfo.PackageCommitHash() },
                { "application commit hash", BuildInfo.ApplicationCommitHash() },
                { "rndSeed", Utilities.Random.RndSeed },
                { "stableRndSeed", Utilities.Random.StableRndSeed },
            });
        }
        protected virtual void LogConstants() {
            eventReporter.LogTS("experiment constants", CONSTANTS.ToDict());
        }
        protected virtual void LogConstantsAndConfigs() {
            var dict = CONSTANTS.ToDict();
            foreach (var kvp in Config.ToDict()) {
                if (dict.ContainsKey(kvp.Key)) {
                    throw new Exception("Experiment constants and one of the Configs have the same key: " + kvp.Key);
                }
                dict[kvp.Key] = kvp.Value;
            }
            eventReporter.LogTS("constants and configs", Config.ToDict());
        }

        // Pause and Quit Functions
        protected virtual async void ExperimentQuit() {
            if (Config.quitAnytime) {
                bool firstLoop = true;
                await ExpHelpers.RepeatUntilYes(async (CancellationToken ct) => {
                    // Resume since they don't want to quit (or haven't tried yet)
                    if (!firstLoop) {
                        ExpHelpers.SetExperimentStatus(HostPcStatusMsg.PAUSE(false));
                        firstLoop = false;
                    }
                    manager.Pause(false);

                    // Wait for the quit key
                    await inputManager.WaitForKey(new List<KeyCode>() { KeyCode.Q }, unpausable: true, ct: ct);

                    // Pause everything and ask if they want to quit
                    ExpHelpers.SetExperimentStatus(HostPcStatusMsg.PAUSE(true));
                    manager.Pause(true);
                }, "experiment quit", LangStrings.ExperimentQuit(), new(), unpausable: true);
                
                manager.Pause(false);
                await manager.QuitTS();
            }
        }
        protected virtual async void ExperimentPause() {
            if (Config.pauseAnytime) {
                var pauseKeyCodes = new List<KeyCode>() { KeyCode.P };
                bool firstLoop = true;
                await ExpHelpers.RepeatForever(async (CancellationToken ct) => {
                    // Resume since they don't want to quit (or haven't tried yet)
                    manager.Pause(false);
                    if (!firstLoop) {
                        ExpHelpers.SetExperimentStatus(HostPcStatusMsg.PAUSE(false));
                        firstLoop = false;
                    }

                    // Wait for the pause key
                    await inputManager.WaitForKey(pauseKeyCodes, ct: ct);

                    // Pause everything and ask if they want to quit
                    manager.Pause(true);
                    ExpHelpers.SetExperimentStatus(HostPcStatusMsg.PAUSE(true));
                }, "experiment pause", LangStrings.ExperimentPaused(), pauseKeyCodes, new(), unpausable: true);
            }
        }

        // Pre-Trial States
        protected virtual async Task Introduction() {
            await ExpHelpers.RepeatUntilYes(async (CancellationToken ct) => {
                await ExpHelpers.PressAnyKey("show instruction video", LangStrings.ShowInstructionVideo());

                manager.videoControl.SetVideo(Config.introductionVideo, true);
                await manager.videoControl.PlayVideo();
            }, "repeat introduction video", LangStrings.RepeatIntroductionVideo(), new());
        }
        protected virtual async Task MicrophoneTest() {
            await ExpHelpers.RepeatUntilYes(async (CancellationToken ct) => {
                await ExpHelpers.PressAnyKey("microphone test prompt", LangStrings.MicrophoneTestTitle(), LangStrings.MicrophoneTest());

                string wavPath = System.IO.Path.Combine(FileManager.SessionPath(), "microphone_test_"
                        + Clock.UtcNow.ToString("yyyy-MM-dd_HH_mm_ss") + ".wav");

                manager.lowBeep.Play();
                await DoWaitWhile(() => manager.lowBeep.isPlaying);
                await manager.Delay(100); // This is needed so you don't hear the end of the beep in the recording

                manager.recorder.StartRecording(wavPath);
                var coloredTestRec = LangStrings.MicrophoneTestRecording().Color("red");
                textDisplayer.Display("microphone test recording", text: coloredTestRec);
                await manager.Delay(Config.micTestDurationMs);
                var clip = manager.recorder.StopRecording();

                var coloredTestPlay = LangStrings.MicrophoneTestPlaying().Color("green");
                textDisplayer.Display("microphone test playing", text: coloredTestPlay);
                manager.playback.PlayOneShot(clip);
                await manager.Delay(Config.micTestDurationMs);
            }, "repeat mic test", LangStrings.RepeatMicTest(), new());
        }
        protected virtual async Task QuitPrompt() {
            ExpHelpers.SetExperimentStatus(HostPcStatusMsg.WAITING());

            textDisplayer.Display("subject/session confirmation",
                text: LangStrings.SubjectSessionConfirmation(Config.subject, Config.sessionNum.Value, Config.experimentName));
            var keyCode = await inputManager.WaitForKey(new List<KeyCode>() { KeyCode.Y, KeyCode.N });

            if (keyCode == KeyCode.N) {
                await manager.QuitTS();
            }
        }
        protected virtual async Task ConfirmStart() {
            await ExpHelpers.PressAnyKey("confirm start", LangStrings.ConfirmStart());
        }
        protected void ReportSessionNum(Dictionary<string, object> extraData = null) {
            var exp = HostPcExpMsg.SESSION(Config.sessionNum.Value);
            var dict = (extraData ?? new()).Concat(exp.dict).ToDictionary(x=>x.Key,x=>x.Value);
            eventReporter.LogTS(exp.name, dict);
            // manager.hostPC.SendExpMsgTS(exp, extraData);
        }
        protected void ReportTrialNum(bool stim, Dictionary<string, object> extraData = null) {
            var exp = HostPcExpMsg.TRIAL((int)session.TrialNum, stim, session.isPractice);
            var dict = (extraData ?? new()).Concat(exp.dict).ToDictionary(x=>x.Key,x=>x.Value);
            eventReporter.LogTS(exp.name, dict);
            // manager.hostPC.SendExpMsgTS(exp, extraData);
        }
    }
}