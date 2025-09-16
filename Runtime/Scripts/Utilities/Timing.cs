//Copyright (c) 2025 Bruska Technologies LLC (James Bruska)

//This file is part of PsyForge.
//PsyForge is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//PsyForge is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with PsyForge. If not, see <https://www.gnu.org/licenses/>. 

using System;
using System.Threading;
using System.Threading.Tasks;

using PsyForge.Threading;
using UnityEngine;

namespace PsyForge.Utilities {
    public class Timing : SingletonEventMonoBehaviour<Timing> {
        protected override void AwakeOverride() { }

        
        public static async Task Delay(int millisecondsDelay, bool pauseAware = true, CancellationToken ct = default) {
            if (millisecondsDelay != 0) {
                await Instance.DoWaitFor(Instance.DelayHelper, millisecondsDelay, (Bool)pauseAware, ct);
            }
        }
        public static async Task DelayTS(int millisecondsDelay, bool pauseAware = true, CancellationToken ct = default) {
            if (millisecondsDelay != 0) {
                // This is hack to get arround the Blittability check for CancellationToken, because I know it is thread safe
                Func<int, Bool, Task> func = async (int _millisecondsDelay, Bool _pauseAware) => {
                    await Instance.DelayHelper(millisecondsDelay, pauseAware, ct);
                };
                await Instance.DoWaitForTS(func, millisecondsDelay, pauseAware);
            }
        }

        internal async Task DelayHelper(int millisecondsDelay, Bool pauseAware, CancellationToken ct) {
            if (millisecondsDelay < 0) {
                throw new ArgumentOutOfRangeException($"millisecondsDelay <= 0 ({millisecondsDelay})");
            } else if (millisecondsDelay == 0) {
                return;
            }

            Timer timer = new(millisecondsDelay, pauseAware);
            while (!timer.IsFinished()) {
                await Awaitable.NextFrameAsync(ct);
            }
        }
    }
}