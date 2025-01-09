//Copyright (c) 2024 Jefferson University (James Bruska)
//Copyright (c) 2024 Bruska Technologies LLC (James Bruska)
//Copyright (c) 2023 University of Pennsylvania (James Bruska)

//This file is part of PsyForge.
//PsyForge is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//PsyForge is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with PsyForge. If not, see <https://www.gnu.org/licenses/>. 

using System;
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
    
        /// <summary>
        /// Load a DXT-compressed DDS image file.
        /// Base on: https://discussions.unity.com/t/can-you-load-dds-textures-during-runtime/84192/2
        /// </summary>
        /// <param name="ddsBytes"></param>
        /// <param name="textureFormat"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static Texture2D LoadTextureDXT(byte[] ddsBytes, TextureFormat textureFormat) {
            if (textureFormat != TextureFormat.DXT1 && textureFormat != TextureFormat.DXT5) {
                throw new Exception("Invalid TextureFormat. Only DXT1 and DXT5 formats are supported by this method.");
            }

            byte ddsSizeCheck = ddsBytes[4];
            if (ddsSizeCheck != 124) {
                throw new Exception("Invalid DDS DXTn texture. Unable to read");  // this header byte should be 124 for DDS image files
            }

            int height = ddsBytes[13] * 256 + ddsBytes[12];
            int width = ddsBytes[17] * 256 + ddsBytes[16];

            int DDS_HEADER_SIZE = 128;
            byte[] dxtBytes = new byte[ddsBytes.Length - DDS_HEADER_SIZE];
            Buffer.BlockCopy(ddsBytes, DDS_HEADER_SIZE, dxtBytes, 0, ddsBytes.Length - DDS_HEADER_SIZE);

            Texture2D texture = new Texture2D(width, height, textureFormat, false);
            texture.LoadRawTextureData(dxtBytes);
            texture.Apply();

            return texture;
        }
    }
}
