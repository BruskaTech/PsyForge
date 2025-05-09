//Copyright (c) 2024 Jefferson University (James Bruska)
//Copyright (c) 2024 Bruska Technologies LLC (James Bruska)
//Copyright (c) 2023 University of Pennsylvania (James Bruska)

//This file is part of PsyForge.
//PsyForge is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//PsyForge is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with PsyForge. If not, see <https://www.gnu.org/licenses/>. 

using System;
using UnityEditor;

namespace PsyForge.Utilities {
    public class Timer {
        readonly DateTime startTime;
        readonly DateTime stopTime;
        TimeSpan pausedDuration;
        DateTime pauseStart;

        /// <summary>
        /// Creates a timer that will stop after the specified duration.
        /// If pauseAware is true, the timer will ignore any time the application is paused.
        /// By default, pauseAware will be on for things on unity main thread, and off for things on other threads.
        /// </summary>
        /// <param name="duration"></param>
        /// <param name="pauseAware"></param>
        public Timer(DateTime stopTime, bool? pauseAware = null) {
            this.startTime = Clock.UtcNow;
            this.stopTime = stopTime;
            this.pausedDuration = TimeSpan.Zero;
            this.pauseStart = default;

            var manager = MainManager.Instance;
            if (pauseAware ?? manager.OnUnityThread()) {
                manager.AddTimerTS(this);
            }
        }
        public Timer(TimeSpan duration, bool? pauseAware = null) : this(Clock.UtcNow + duration, pauseAware) {}
        public Timer(int durationMs, bool? pauseAware = null) : this(TimeSpan.FromMilliseconds(durationMs), pauseAware) {}

        public void Pause() {
            if (pauseStart == default) { // Don't reset if already paused
                pauseStart = Clock.UtcNow;
            }
        }

        public void UnPause() {
            if (pauseStart != default) {
                pausedDuration += Clock.UtcNow - pauseStart;
                pauseStart = default; // Reset pause start
            }
        }

        public bool IsFinished() {
            var currentPauseDuration = pauseStart != default ? Clock.UtcNow - pauseStart : TimeSpan.Zero;
            var ret = Clock.UtcNow >= stopTime + pausedDuration + currentPauseDuration;
            if (ret) { MainManager.Instance.TryRemoveTimerTS(this); }
            return Clock.UtcNow >= stopTime + pausedDuration + currentPauseDuration;
        }
    }
}