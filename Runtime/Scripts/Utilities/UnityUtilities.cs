//Copyright (c) 2024 Jefferson University (James Bruska)
//Copyright (c) 2024 Bruska Technologies LLC (James Bruska)
//Copyright (c) 2023 University of Pennsylvania (James Bruska)

//This file is part of PsyForge.
//PsyForge is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//PsyForge is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with PsyForge. If not, see <https://www.gnu.org/licenses/>. 

using UnityEngine;

namespace PsyForge.Utilities {
    public static class UnityUtilities {
        public static bool IsMacOS() {
            return Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer;
        }

        /// <summary>
        /// Returns the attempted frame rate of the current Unity application.
        /// If the game is set to run as fast as possible, then this will return -1.
        /// </summary>
        /// <returns></returns>
        public static int FrameRate() {
            if (QualitySettings.vSyncCount > 0) {
                var screenFpsRatio = Screen.currentResolution.refreshRateRatio;
                var screenFps = screenFpsRatio.numerator / screenFpsRatio.denominator;
                return (int)screenFps / QualitySettings.vSyncCount;
            } else {
                return Application.targetFrameRate;
            }
        }
    }
}
