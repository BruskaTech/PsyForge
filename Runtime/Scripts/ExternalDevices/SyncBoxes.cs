//Copyright (c) 2024 Jefferson University (James Bruska)
//Copyright (c) 2024 Bruska Technologies LLC (James Bruska)
//Copyright (c) 2023 University of Pennsylvania (James Bruska)

//This file is part of PsyForge.
//PsyForge is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//PsyForge is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with PsyForge. If not, see <https://www.gnu.org/licenses/>. 

using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;

namespace PsyForge.ExternalDevices {
    public class SyncBoxes {
        private List<SyncBox> syncBoxes = new List<SyncBox>();

        public async Task AddSyncBox(SyncBox syncBox) {
            if (syncBoxes.Any(x => x.GetType() == syncBox.GetType())) {
                throw new Exception($"SyncBox of type {syncBox.GetType()} already exists."
                    + "\n\nMake sure you do not have the same SyncBox multiple times in the config.");
            }
            syncBoxes.Add(syncBox);
            await syncBox.Init();
        }
        public T GetSyncBox<T>() where T : SyncBox {
            return syncBoxes.OfType<T>().FirstOrDefault();
        }

        private async Task Init() {
            await Task.WhenAll(syncBoxes.Select(x => x.Init()));
        }

        internal async Task TearDown() {
            await Task.WhenAll(syncBoxes.Select(x => x.TearDown()));
        }

        public async Task Pulse(Dictionary<string, object> logOnValues = null, Dictionary<string, object> logOffValues = null) {
            await Task.WhenAll(syncBoxes.Select(x => x.Pulse(logOnValues, logOffValues)));
        }

        public void StartContinuousPulsing() {
            syncBoxes.ForEach(syncBox => syncBox.StartContinuousPulsing());
        }
        public void StopContinuousPulsing() {
            syncBoxes.ForEach(syncBox => syncBox.StopContinuousPulsing());
        }
        public bool IsContinuousPulsing() {
            return syncBoxes.Any(x => x.IsContinuousPulsing());
        }
    }
}