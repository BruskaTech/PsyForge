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

        protected new void Awake() {
            base.Awake();
            inputManager = InputManager.Instance;
            textDisplayer = TextDisplayer.Instance;
            LogExperimentInfo();
            LogConstants();
            LogConstantsAndConfigs();
            ReportSessionNum();
        }

        protected void OnEnable() {
            ExperimentActive.SetActive(true);
        }

        protected void OnDisable() {
            ExperimentActive.SetActive(false);
        }

        protected abstract Task PreTrialStates();
        protected abstract Task PracticeTrialStates();
        protected abstract Task TrialStates();
        protected abstract Task PostTrialStates();

        protected void EndCurrentSession() {
            throw new EndSessionException();
        }

        protected void Run() {
            DoTS(RunHelper().ToEnumerator);
        }
        protected async Task RunHelper() {
            ExperimentSetup();
            
            await PreTrialStates();

            if (practiceSession == null) {
                throw new Exception("The Experiment did not set any practice sessions.");
            } else if (normalSession == null) {
                throw new Exception("The Experiment did not set any normal session.");
            }

            session = practiceSession;
            try {
                while (true) {
                    await PracticeTrialStates();
                    session.TrialNum++;
                }
            } catch (EndSessionException) {} // do nothing

            session = normalSession;
            try {
                while (true) {
                    await TrialStates();
                    session.TrialNum++;
                }
            } catch (EndSessionException) {} // do nothing

            await PostTrialStates();
            await manager.QuitTS();
        }

        protected void ExperimentSetup() {
            DoTS(ExperimentQuit);
            DoTS(ExperimentPause);
            manager.syncBox?.StartContinuousPulsing();
        }

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

        protected virtual async void ExperimentQuit() {
            if (Config.quitAnytime) {
                bool firstLoop = true;
                await RepeatUntilYes(async (CancellationToken ct) => {
                    // Resume since they don't want to quit (or haven't tried yet)
                    if (!firstLoop) {
                        SetExperimentStatus(HostPcStatusMsg.PAUSE(false));
                        firstLoop = false;
                    }
                    manager.Pause(false);

                    // Wait for the quit key
                    await inputManager.WaitForKey(new List<KeyCode>() { KeyCode.Q }, unpausable: true, ct: ct);

                    // Pause everything and ask if they want to quit
                    SetExperimentStatus(HostPcStatusMsg.PAUSE(true));
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
                await RepeatForever(async (CancellationToken ct) => {
                    // Resume since they don't want to quit (or haven't tried yet)
                    manager.Pause(false);
                    if (!firstLoop) {
                        SetExperimentStatus(HostPcStatusMsg.PAUSE(false));
                        firstLoop = false;
                    }

                    // Wait for the pause key
                    await inputManager.WaitForKey(pauseKeyCodes, ct: ct);

                    // Pause everything and ask if they want to quit
                    manager.Pause(true);
                    SetExperimentStatus(HostPcStatusMsg.PAUSE(true));
                }, "experiment pause", LangStrings.ExperimentPaused(), pauseKeyCodes, new(), unpausable: true);
            }
        }

        // Wrapper/Replacement Functions
        protected bool IsNumericKeyCode(KeyCode keyCode) {
            bool isAlphaNum = keyCode >= KeyCode.Alpha0 && keyCode <= KeyCode.Alpha9;
            bool isKeypadNum = keyCode >= KeyCode.Keypad0 && keyCode <= KeyCode.Keypad9;
            return isAlphaNum || isKeypadNum;
        }
        protected virtual void SendRamulatorStateMsg(HostPcStatusMsg state, bool stateToggle, Dictionary<string, object> extraData = null) {
            // Do nothing by default
        }
        protected async Task RepeatUntilYes(Func<CancellationToken, Task> preFunc, string description, LangString displayText, CancellationToken ct, Func<bool, CancellationToken, Task> postFunc = null, bool unpausable = false) {
            var repeat = true;
            while (repeat && !ct.IsCancellationRequested) {
                await preFunc(ct);
                ct.ThrowIfCancellationRequested();

                SendRamulatorStateMsg(HostPcStatusMsg.WAITING(), true);
                await textDisplayer.DisplayForTask(description, LangStrings.Blank(), displayText, null, ct, async (CancellationToken ct) => {
                    var keyCode = await inputManager.WaitForKey(new List<KeyCode>() { KeyCode.Y, KeyCode.N }, unpausable: unpausable, ct: ct);
                    repeat = keyCode != KeyCode.Y;
                });
                SendRamulatorStateMsg(HostPcStatusMsg.WAITING(), false);
                ct.ThrowIfCancellationRequested();

                if (postFunc != null) { await postFunc(repeat, ct); }
            }
        }
        protected async Task RepeatUntilNo(Func<CancellationToken, Task> preFunc, string description, LangString displayText, CancellationToken ct, Func<bool, CancellationToken, Task> postFunc = null, bool unpausable = false) {
            var repeat = true;
            while (repeat && !ct.IsCancellationRequested) {
                await preFunc(ct);
                ct.ThrowIfCancellationRequested();

                SendRamulatorStateMsg(HostPcStatusMsg.WAITING(), true);
                await textDisplayer.DisplayForTask(description, LangStrings.Blank(), displayText, null, ct, async (CancellationToken ct) => {
                    var keyCode = await inputManager.WaitForKey(new List<KeyCode>() { KeyCode.Y, KeyCode.N }, unpausable: unpausable, ct: ct);
                    repeat = keyCode != KeyCode.N;
                });
                SendRamulatorStateMsg(HostPcStatusMsg.WAITING(), false);
                ct.ThrowIfCancellationRequested();

                if (postFunc != null) { await postFunc(repeat, ct); }
            }
        }
        protected async Task RepeatForever(Func<CancellationToken, Task> preFunc, string description, LangString displayText, List<KeyCode> keyCodes, CancellationToken ct, Func<CancellationToken, Task> postFunc = null, bool unpausable = false) {
            while (!ct.IsCancellationRequested) {
                await preFunc(ct);
                ct.ThrowIfCancellationRequested();

                SendRamulatorStateMsg(HostPcStatusMsg.WAITING(), true);
                await textDisplayer.DisplayForTask(description, LangStrings.Blank(), displayText, null, ct, async (CancellationToken ct) => {
                    var keyCode = await inputManager.WaitForKey(keyCodes, unpausable: unpausable, ct: ct);
                });
                SendRamulatorStateMsg(HostPcStatusMsg.WAITING(), false);
                ct.ThrowIfCancellationRequested();

                if (postFunc != null) { await postFunc(ct); }
            }
        }

        // Pre-Trial States
        protected virtual async Task Introduction() {
            await RepeatUntilYes(async (CancellationToken ct) => {
                await PressAnyKey("show instruction video", LangStrings.ShowInstructionVideo());

                manager.videoControl.SetVideo(Config.introductionVideo, true);
                await manager.videoControl.PlayVideo();
            }, "repeat introduction video", LangStrings.RepeatIntroductionVideo(), new());
        }
        protected virtual async Task MicrophoneTest() {
            await RepeatUntilYes(async (CancellationToken ct) => {
                await PressAnyKey("microphone test prompt", LangStrings.MicrophoneTestTitle(), LangStrings.MicrophoneTest());

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
                manager.playback.Play(clip);
                await manager.Delay(Config.micTestDurationMs);
            }, "repeat mic test", LangStrings.RepeatMicTest(), new());
        }
        protected virtual async Task QuitPrompt() {
            SendRamulatorStateMsg(HostPcStatusMsg.WAITING(), true);
            SetExperimentStatus(HostPcStatusMsg.WAITING());

            textDisplayer.Display("subject/session confirmation",
                text: LangStrings.SubjectSessionConfirmation(Config.subject, Config.sessionNum.Value, Config.experimentName));
            var keyCode = await inputManager.WaitForKey(new List<KeyCode>() { KeyCode.Y, KeyCode.N });

            SendRamulatorStateMsg(HostPcStatusMsg.WAITING(), false);

            if (keyCode == KeyCode.N) {
                await manager.QuitTS();
            }
        }
        protected virtual async Task ConfirmStart() {
            await PressAnyKey("confirm start", LangStrings.ConfirmStart());
        }
        protected void SetExperimentStatus(HostPcStatusMsg state, Dictionary<string, object> extraData = null) {
            var dict = (extraData ?? new()).Concat(state.dict).ToDictionary(x=>x.Key,x=>x.Value);
            eventReporter.LogTS(state.name, dict);
            // manager.hostPC?.SendStatusMsgTS(state, extraData);
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
    
        /// <summary>
        /// Display a message and wait for keypress
        /// </summary>
        /// <param name="description"></param>
        /// <param name="displayText"></param>
        /// <param name="displayText"></param>
        /// <returns></returns>
        protected async Task<KeyCode> PressAnyKey(string description, LangString displayText) {
            return await PressAnyKey(description, null, displayText);
        }
        protected async Task<KeyCode> PressAnyKey(string description, LangString displayTitle, LangString displayText) {
            SetExperimentStatus(HostPcStatusMsg.WAITING());
            // TODO: JPB: (needed) Add Ramulator to match this
            textDisplayer.Display($"{description} (press any key prompt)", displayTitle, displayText, LangStrings.AnyKeyToContinue());
            var keyCode = await InputManager.Instance.WaitForKey();
            textDisplayer.Clear();
            return keyCode;
        }
    }
}