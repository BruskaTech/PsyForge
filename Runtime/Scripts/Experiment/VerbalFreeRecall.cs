//Copyright (c) 2024 Columbia University (James Bruska)
//Copyright (c) 2024 Bruska Technologies LLC (James Bruska)

//This file is part of CityBlock.
//CityBlock is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//CityBlock is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with CityBlock. If not, see <https://www.gnu.org/licenses/>.

using PsyForge;
using PsyForge.DataManagement;
using PsyForge.Experiment;
using PsyForge.ExternalDevices;
using PsyForge.GUI;
using PsyForge.Localization;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace PsyForge.Experiment {
    public class VerbalFreeRecall {
        protected readonly TextDisplayer textDisplayer = TextDisplayer.Instance;
        protected readonly EventReporter eventReporter = EventReporter.Instance;
        protected readonly MainManager manager = MainManager.Instance;

        protected readonly int practiceVerbalFreeRecallDurationMs;
        protected readonly int verbalFreeRecallDurationMs;

        public VerbalFreeRecall(int practiceVerbalFreeRecallDurationMs, int verbalFreeRecallDurationMs) {
            this.practiceVerbalFreeRecallDurationMs = practiceVerbalFreeRecallDurationMs;
            this.verbalFreeRecallDurationMs = verbalFreeRecallDurationMs;
        }

        public async Task Run(bool isPractice, uint trialNum, LangString verbalRecallDisplay = null) {
            verbalRecallDisplay ??= LangStrings.VerbalRecallDisplay();

            // Setup
            var realVerbalFreeRecallDurationMs = isPractice ?
                practiceVerbalFreeRecallDurationMs :
                verbalFreeRecallDurationMs;
            ExpHelpers.SetExperimentStatus(HostPcStatusMsg.RECALL(realVerbalFreeRecallDurationMs, trialNum));
            var practiceStr = isPractice ? "practice_" : "";
            string wavPath = Path.Combine(FileManager.SessionPath(), practiceStr + trialNum + ".wav");

            // Play start beep
            manager.lowBeep.Play();
            textDisplayer.Display("verbal recall display", text: verbalRecallDisplay);

            while (manager.lowBeep.isPlaying) { await Awaitable.NextFrameAsync(); }
            await manager.Delay(100); // This is needed so you don't hear the end of the beep in the recording

            // Start recording
            manager.recorder.StartRecording(wavPath);
            eventReporter.LogTS("start verbal recall period");

            // Wait for recall duration
            await manager.Delay(realVerbalFreeRecallDurationMs);

            // Stop recording and beep for indication
            textDisplayer.Clear();
            manager.recorder.StopRecording();
            eventReporter.LogTS("end verbal recall period");
            manager.lowBeep.Play();
        }
    }
}