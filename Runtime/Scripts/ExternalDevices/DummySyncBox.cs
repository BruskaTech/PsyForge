//Copyright (c) 2024 Columbia University (James Bruska)

//This file is part of CityBlock.
//CityBlock is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//CityBlock is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with CityBlock. If not, see <https://www.gnu.org/licenses/>.

using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace PsyForge.ExternalDevices {

    /// <summary>
    /// Dummy SyncBox that only logs its timing.
    /// It also prints a Debug Log in the Unity Editor.
    /// </summary>
    public class DummySyncBox : SyncBox {
        internal override Task Init() { return Task.CompletedTask; }
        internal override Task TearDown() { return Task.CompletedTask; }
        protected override async Task PulseInternals(CancellationToken ct = default) {
            Debug.Log("DummySyncBox Pulse On");
            await manager.Delay(500);
            Debug.Log("DummySyncBox Pulse Off");
            await manager.Delay(500, ct: ct);
        }
        public override int MaxPulseDuration() {
            return 1000;
        }
    }

}

