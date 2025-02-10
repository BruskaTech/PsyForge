﻿//Copyright (c) 2024 Jefferson University (James Bruska)
//Copyright (c) 2024 Bruska Technologies LLC (James Bruska)
//Copyright (c) 2023 University of Pennsylvania (James Bruska)

//This file is part of PsyForge.
//PsyForge is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//PsyForge is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with PsyForge. If not, see <https://www.gnu.org/licenses/>. 

using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using PsyForge.Utilities;
using PsyForge.ExternalDevices;
using PsyForge.Extensions;
using System.Collections.Generic;
using TMPro;

namespace PsyForge.GUI {

    /// <summary>
    /// This handles the button which launches the experiment.
    /// 
    /// DoLaunchExperiment is responsible for calling EditableExperiment.ConfigureExperiment with the proper parameters.
    /// </summary>
    public class LaunchExperiment : EventMonoBehaviour {
        [SerializeField] protected ExperimentSelection experimentSelection;
        [SerializeField] protected TMP_InputField participantNameInput;
        [SerializeField] protected GameObject launchButton;

        [SerializeField] protected GameObject syncButton;
        [SerializeField] protected GameObject greyedLaunchButton;
        [SerializeField] protected GameObject loadingButton;

        [SerializeField] protected TextMeshProUGUI experimentLauncherTitleText;
        [SerializeField] protected TextMeshProUGUI experimentTitleText;
        [SerializeField] protected TextMeshProUGUI subjectTitleText;
        [SerializeField] protected TextMeshProUGUI sessionTitleText;
        [SerializeField] protected TextMeshProUGUI greyedLaunchButtonText;

        protected readonly List<KeyCode> ynKeyCodes = new List<KeyCode> {KeyCode.Y, KeyCode.N};

        protected override void AwakeOverride() {
            experimentLauncherTitleText.text = LangStrings.StartupExperimentLauncher();
            experimentTitleText.text = LangStrings.StartupExperiment();
            subjectTitleText.text = LangStrings.StartupSubject();
            sessionTitleText.text = LangStrings.StartupSession();
            participantNameInput.placeholder.GetComponent<TextMeshProUGUI>().text = LangStrings.StartupParticipantCodePlaceholder();
            syncButton.GetComponentInChildren<TextMeshProUGUI>().text = LangStrings.StartupTestSyncboxButton();
            loadingButton.GetComponentInChildren<TextMeshProUGUI>().text = LangStrings.StartupLoadingButton();
        }

        void Update() {
            string experimentName = experimentSelection.GetExperiment();
            bool participantValid = isValidParticipant(participantNameInput.text);
            bool syncboxTestRunning = manager.syncBox?.IsContinuousPulsing() ?? false;

            if (experimentName == null) {
                greyedLaunchButtonText.text = LangStrings.StartupGreyedLaunchButtonSelectExp();
                launchButton.SetActive(false);
            } else if (participantNameInput.text.Equals("")) {
                greyedLaunchButtonText.text = LangStrings.StartupGreyedLaunchButtonEnterParticipant();
                launchButton.SetActive(false);
            } else if (!participantValid) {
                greyedLaunchButtonText.text = LangStrings.StartupGreyedLaunchButtonEnterValidParticipant();
                launchButton.SetActive(false);
            } else if (syncboxTestRunning) {
                greyedLaunchButtonText.text = LangStrings.StartupGreyedLaunchButtonSyncboxTest();
                launchButton.SetActive(false);
            } else {
                launchButton.SetActive(true);
            }
            greyedLaunchButton.SetActive(!launchButton.activeSelf);

            if (participantValid) {
                int sessionNumber = ParticipantSelection.nextSessionNumber;
                launchButton.GetComponentInChildren<TextMeshProUGUI>().text = LangStrings.StartupLaunchButton(sessionNumber);
            }
        }

        public async void DoSyncBoxTest() {
            await DoWaitFor(DoSyncBoxTestHelper);
        }
        protected async Task DoSyncBoxTestHelper() {
            if (!manager.syncBox?.IsContinuousPulsing() ?? false) {
                syncButton.GetComponent<Button>().interactable = false;

                // TODO: JPB: (need) Fix Syncbox test
                manager.syncBox.StartContinuousPulsing();
                await manager.Delay(Config.syncBoxTestDurationMs);
                manager.syncBox.StopContinuousPulsing();

                syncButton.GetComponent<Button>().interactable = true;
            }
        }

        // activated by UI launch button
        public void LaunchExp() {
            DoTS(LaunchExpHelper);
        }
        protected async void LaunchExpHelper() {
            if (!Config.IsExperimentConfigSetup()) {
                throw new Exception("No experiment configuration loaded");
            }

            launchButton.SetActive(false);
            loadingButton.SetActive(true);

            // Create path for current participant/session and set the subject and sessionNum globally
            // CreateSession MUST be called before the Config.sessionNum is set because if there is an error in the session creation, 
            //    there will be a recursive error as it tries to write the the session.json file in the path that doesn't exist yet.
            string subject = participantNameInput.text;
            int sessionNumber = ParticipantSelection.nextSessionNumber;
            FileManager.CreateSession(subject, sessionNumber);
            Config.subject.Val = subject;
            Config.sessionNum.Val = sessionNumber;

            // Setup the stable random seed with the participant name
            Utilities.Random.StableRndSeed = Config.subject.GetHashCode();

            // Setup basic Unity stuff
            Cursor.visible = false;
            Application.runInBackground = true;

            // Connect to HostPC
            if (Config.elememOn) {
                TextDisplayer.Instance.Display("Elemem connection display", text: LangStrings.ElememConnection());
                manager.hostPC = new ElememInterface(sessionNumber);
            }
            if (manager.hostPC != null) {
                await manager.hostPC.ConnectTS();
                await manager.hostPC.ConfigureTS();
            }

            // Set the game frame rate
            await SetFrameRate();

            // Save Configs
            Config.SaveConfigs(FileManager.SessionPath());
            eventReporter.experimentConfigured = true;

            SceneManager.sceneLoaded += onExperimentSceneLoaded;
            SceneManager.LoadScene(Config.experimentScene);
        }

        private async Task SetFrameRate() {
            // Make the game run at screen refresh rate if targetFrameRate is not set
            if (!Config.targetFrameRate.HasValue) {
                QualitySettings.vSyncCount = 1;
                Application.targetFrameRate = -1;
                return;
            }

            var targetFps = Config.targetFrameRate.Val ?? -1;

            // Make the game run as fast as possible
            if (targetFps < 0) {
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = -1;
                return;
            }

            if (targetFps == 0) {
                throw new Exception("Config variable targetFrameRate must not be 0.");
            }

            // Get the screen refresh rate
            var screenFpsRatio = Screen.currentResolution.refreshRateRatio;
            var screenFps = screenFpsRatio.numerator / screenFpsRatio.denominator;

            // Make the game run at the target frame rate
            if (screenFps % targetFps == 0) {
                QualitySettings.vSyncCount = (int)(screenFps / targetFps);
                Application.targetFrameRate = targetFps;
            } else {
                TextDisplayer.Instance.Display("incompatible frame rate",
                    text: LangStrings.IncompatibleTargetFrameRate(targetFps, screenFps));
                var keyCode = await InputManager.Instance.WaitForKey(ynKeyCodes);
                if (keyCode == KeyCode.Y) {
                    QualitySettings.vSyncCount = 0;
                    Application.targetFrameRate = targetFps;
                } else {
                    throw new Exception($"Config variable targetFrameRate ({Config.targetFrameRate.Val}) must be a factor of the screen refresh rate {screenFps}.");
                }
            }

            // Check for inaccurate frame display times
            if (Config.logFrameDisplayTimes && QualitySettings.vSyncCount == 0) {
                throw new Exception("The config variable 'logFrameDisplayTimes' is enabled but VSync is not enabled. This will cause inaccurate frame display times."
                    + "\n\nPlease set the config variable 'targetFrameRate' to a multiple of the screen refresh rate or turn off 'logFrameDisplayTimes' in the config."
                    + "\nYou can also remove 'targetFrameRate' from the config to have the game run at the screen refresh rate.");
            }
        }

        private static void onExperimentSceneLoaded(Scene scene, LoadSceneMode mode) {
            // Experiment Manager
            // TODO: JPB: (bug) Fix issue where unity crashes if I check for multiple experiments
            try {
                // Use gameObject.scene to get values in DontDestroyOnLoad
                var activeExperiments = MainManager.Instance.gameObject.scene.GetRootGameObjects()
                    .Where(go => go.name == Config.experimentClass && go.activeSelf);

                if (activeExperiments.Count() == 0) {
                    var expManager = scene.GetRootGameObjects().Where(go => go.name == Config.experimentClass).First();
                    expManager.SetActive(true);
                }
            } catch (InvalidOperationException exception) {
                throw new Exception(
                    $"Missing experiment GameObject that is the same name as the experiment class ({Config.experimentClass})",
                    exception);
            }

            SceneManager.sceneLoaded -= onExperimentSceneLoaded;
        }

        private bool isValidParticipant(string name) {
            return FileManager.isValidParticipant(name);
        }
    }
}