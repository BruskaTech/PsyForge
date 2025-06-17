//Copyright (c) 2024 Jefferson University (James Bruska)
//Copyright (c) 2024 Bruska Technologies LLC (James Bruska)
//Copyright (c) 2023 University of Pennsylvania (James Bruska)

//This file is part of PsyForge.
//PsyForge is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//PsyForge is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with PsyForge. If not, see <https://www.gnu.org/licenses/>. 

using System.IO;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using UnityEngine.UI;
using PsyForge.Localization;
using System;
using UnityEngine.EventSystems;

namespace PsyForge.GUI {

    /// <summary>
    /// This is attached to the participant selection dropdown.  It is responsible for loading the information about the selected participant.
    /// 
    /// It also allows users to edit the loaded information with Increase/Decrease Session/List number buttons.
    /// </summary>
    [RequireComponent(typeof(TMP_Dropdown))]
    public class ParticipantSelection : EventMonoBehaviour {
        protected override void AwakeOverride() { }

        public TMP_InputField participantNameInput;
        public TextMeshProUGUI sessionNumberText;


        public static int nextSessionNumber = 0;
        public static int nextListNumber = 0;

        public void ExperimentUpdated() {
            Do(ExperimentUpdatedHelper);
        }
        protected virtual void ExperimentUpdatedHelper() {
            FindParticipants();
        }


        public void ParticipantSelected() {
            Do(ParticipantSelectedHelper);
        }
        protected virtual void ParticipantSelectedHelper() {
            LoadParticipant();
        }


        public void DecreaseSessionNumber() {
            Do(DecreaseSessionNumberHelper);
        }
        protected virtual void DecreaseSessionNumberHelper() {
            if (nextSessionNumber > 0)
                nextSessionNumber--;
            UpdateSessionNumTexts();
        }

        public void IncreaseSessionNumber() {
            Do(IncreaseSessionNumberHelper);
        }
        protected virtual void IncreaseSessionNumberHelper() {
            nextSessionNumber++;
            UpdateSessionNumTexts();
        }

        protected virtual void FindParticipants() {
            var dropdown = GetComponent<TMP_Dropdown>();

            dropdown.ClearOptions();
            dropdown.AddOptions(new List<string>() { LangStrings.StartupNewParticipant() });

            string participantDirectory = FileManager.ExperimentPath();
            if (Directory.Exists(participantDirectory)) {
                string[] filepaths = Directory.GetDirectories(participantDirectory);
                List<string> filenames = new List<string>();

                for (int i = 0; i < filepaths.Length; i++) {
                    var filename = Path.GetFileName(filepaths[i]);
                    var participantInput = participantNameInput.text ?? "";
                    if (FileManager.isValidParticipant(filename) && filename.StartsWith(participantInput)) {
                        filenames.Add(filename);
                    }
                }

                dropdown.AddOptions(filenames);
            }
            dropdown.RefreshShownValue();

            nextSessionNumber = 0;
            nextListNumber = 0;
            UpdateSessionNumTexts();
        }
        protected virtual void LoadParticipant() {
            var dropdown = GetComponent<TMP_Dropdown>();
            if (dropdown.value < 0) return;

            string selectedParticipant = dropdown.captionText.text;

            if (selectedParticipant == LangStrings.StartupNewParticipant()) {
                participantNameInput.text = "";
                nextSessionNumber = 0;
                UpdateSessionNumTexts();
                return;
            }

            if (!Directory.Exists(FileManager.ParticipantPath(selectedParticipant)))
                throw new UnityException("You tried to load a participant that doesn't exist.");

            participantNameInput.text = selectedParticipant;
            nextSessionNumber = FileManager.CurrentSession(selectedParticipant);

            UpdateSessionNumTexts();
        }
        protected virtual void UpdateSessionNumTexts() {
            sessionNumberText.text = nextSessionNumber.ToString();
        }

        public virtual void UpdateParticipantDropDown() {
            FindParticipants();
        }
    }

}