//Copyright (c) 2025 University of Bonn (James Bruska)
//Copyright (c) 2024 Jefferson University (James Bruska)
//Copyright (c) 2024 Bruska Technologies LLC (James Bruska)
//Copyright (c) 2023 University of Pennsylvania (James Bruska)

//This file is part of PsyForge.
//PsyForge is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//PsyForge is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with PsyForge. If not, see <https://www.gnu.org/licenses/>. 

using PsyForge.Localization;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PsyForge.GUI {

    /// <summary>
    /// This is attached to the dropdown menu which selects experiments.
    /// 
    /// It only needs to call PsyForge.SetExperimentName().
    /// </summary>
    [RequireComponent(typeof(TMP_Dropdown))]
    public class ExperimentSelection : EventMonoBehaviour {
        protected TMP_Dropdown dropdown;

        [SerializeField]
        protected ParticipantSelection participantSelection;

        protected override void AwakeOverride() {
            dropdown = GetComponent<TMP_Dropdown>();

            List<string> experiments = new(Config.availableExperiments.Val);

            dropdown.AddOptions(new List<string>(new string[] { LangStrings.StartupExperimentPlaceholder() }));
            dropdown.AddOptions(experiments);
            SetExperiment();
        }

        public void SetExperiment() {
            DoTS(SetExperimentHelper);
        }
        protected virtual async void SetExperimentHelper() {
            if (dropdown.captionText.text != LangStrings.StartupExperimentPlaceholder()) {
                await Config.SetupExperimentConfig(dropdown.captionText.text);
                participantSelection.ExperimentUpdated();
            }
        }

        public string GetExperiment() {
            return DoGet(GetExperimentHelper);
        }
        protected virtual string GetExperimentHelper() {
            if (dropdown.captionText.text != LangStrings.StartupExperimentPlaceholder()) {
                return dropdown.captionText.text;
            } else {
                return null;
            }
        }
    }

}