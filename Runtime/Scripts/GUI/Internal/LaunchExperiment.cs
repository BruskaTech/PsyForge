//Copyright (c) 2024 Jefferson University (James Bruska)
//Copyright (c) 2024 Bruska Technologies LLC (James Bruska)
//Copyright (c) 2023 University of Pennsylvania (James Bruska)

//This file is part of PsyForge.
//PsyForge is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//PsyForge is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with PsyForge. If not, see <https://www.gnu.org/licenses/>. 

using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using PsyForge.Localization;

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

        protected virtual void Update() {
            string experimentName = experimentSelection.GetExperiment();
            bool participantValid = FileManager.isValidParticipant(participantNameInput.text);
            bool syncboxTestRunning = manager.syncBoxes?.IsContinuousPulsing() ?? false;

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
            if (!manager.syncBoxes?.IsContinuousPulsing() ?? false) {
                syncButton.GetComponent<Button>().interactable = false;

                // TODO: JPB: (need) Fix Syncbox test
                manager.syncBoxes.StartContinuousPulsing();
                await manager.Delay(Config.syncBoxTestDurationMs);
                manager.syncBoxes.StopContinuousPulsing();

                syncButton.GetComponent<Button>().interactable = true;
            }
        }

        // activated by UI launch button
        public void LaunchExp() {
            DoTS(LaunchExpHelper);
        }
        protected async void LaunchExpHelper() {
            if (launchButton != null) launchButton.SetActive(false);
            if (loadingButton != null) loadingButton.SetActive(true);

            string subject = participantNameInput.text;
            int sessionNumber = ParticipantSelection.nextSessionNumber;

            await Startup.LaunchExperiment(subject, sessionNumber);
        }
    }
}