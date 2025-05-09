//Copyright (c) 2024 Jefferson University (James Bruska)
//Copyright (c) 2024 Bruska Technologies LLC (James Bruska)
//Copyright (c) 2023 University of Pennsylvania (James Bruska)

//This file is part of PsyForge.
//PsyForge is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//PsyForge is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with PsyForge. If not, see <https://www.gnu.org/licenses/>. 

using System.Threading.Tasks;
using UnityEngine;
using PsyForge.DataManagement;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace PsyForge.ExternalDevices {
    public abstract class SyncBox : EventMonoBehaviour {
        private bool continuousPulsing = false;
        private int lastFrameCount = -1;
        protected int pulseNum { get; private set; } = 0;

        internal abstract Task Init();
        protected abstract Task PulseInternals(CancellationToken ct = default);
        internal abstract Task TearDown();

        protected override void AwakeOverride() { }
        protected void OnDestroy() {
            StopContinuousPulsing();
            TearDown();
        }

        /// <summary>
        /// This method is called every time the SyncBox pulses on.
        //  It should return a dictionary of values that will be logged to the event log.
        /// Do not use "pulseNum" since it is already used.
        /// </summary>
        /// <returns></returns>
        protected virtual Dictionary<string, object> PulseOnLogValues() {
            return new();
        }

        /// <summary>
        /// This method is called every time the SyncBox pulses off.
        //  It should return a dictionary of values that will be logged to the event log.
        /// Do not use "pulseNum" since it is already used.
        /// </summary>
        /// <returns></returns>
        protected virtual Dictionary<string, object> PulseOffLogValues() {
            return new();
        }

        public async Task Pulse(Dictionary<string, object> logOnValues = null, Dictionary<string, object> logOffValues = null, CancellationToken ct = default) {
            Dictionary<string, object> logOnDict =
                new Dictionary<string, object>() {
                    { "syncBox", GetType().Name },
                    { "pulseNum", pulseNum }, }
                .Concat(PulseOnLogValues() ?? new())
                .Concat(logOnValues ?? new())
                .ToDictionary(x=>x.Key,x=>x.Value);
            EventReporter.Instance.LogTS("syncbox pulse on", logOnDict);

            await PulseInternals(ct);

            Dictionary<string, object> logOffDict =
                new Dictionary<string, object>() {
                    { "syncBox", GetType().Name },
                    { "pulseNum", pulseNum }, }
                .Concat(PulseOffLogValues() ?? new())
                .Concat(logOffValues ?? new())
                .ToDictionary(x=>x.Key,x=>x.Value);
            EventReporter.Instance.LogTS("syncbox pulse off", logOffDict);

            pulseNum++;
        }

        public void StartContinuousPulsing(CancellationToken ct = default) {
            DoTS(StartContinuousPulsingHelper, ct);
        }
        private async void StartContinuousPulsingHelper(CancellationToken ct) {
            continuousPulsing = true;
            while (continuousPulsing && !ct.IsCancellationRequested) {
                if (lastFrameCount == Time.frameCount) {
                    throw new System.Exception($"SyncBox ({this.GetType().Name}) is pulsing too fast (or has no delays in it). You can only pulse once per frame.");
                }
                lastFrameCount = Time.frameCount;
                await Pulse(ct: ct);
            }
        }
        public void StopContinuousPulsing() {
            continuousPulsing = false;
        }
        public bool IsContinuousPulsing() {
            return continuousPulsing;
        }
    }
}