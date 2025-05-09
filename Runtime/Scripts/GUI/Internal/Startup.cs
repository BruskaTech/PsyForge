//Copyright (c) 2025 University of Bonn (James Bruska)

//This file is part of OpenField.
//PsyForge is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//PsyForge is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with PsyForge. If not, see <https://www.gnu.org/licenses/>. 

using PsyForge.DataManagement;
using PsyForge.ExternalDevices;
using PsyForge.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PsyForge.GUI {
    internal static class Startup {
        private static readonly List<KeyCode> ynKeyCodes = new List<KeyCode> {KeyCode.Y, KeyCode.N};

        public static async Awaitable LaunchExperiment(string subject, int sessionNumber) {
            MainManager manager = MainManager.Instance;
            EventReporter eventReporter = EventReporter.Instance;

            if (!Config.IsExperimentConfigSetup()) {
                throw new Exception("No experiment configuration loaded");
            }

            // Create path for current participant/session and set the subject and sessionNum globally
            // CreateSession MUST be called before the Config.sessionNum is set because if there is an error in the session creation, 
            //    there will be a recursive error as it tries to write the the session.json file in the path that doesn't exist yet.
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

        private static async Awaitable SetFrameRate() {
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
    }
}