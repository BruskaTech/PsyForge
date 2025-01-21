//Copyright (c) 2024 Jefferson University
//Copyright (c) 2023 University of Pennsylvania

//This file is part of UnityExperiments.
//UnityExperiments is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//UnityExperiments is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with UnityExperiments. If not, see <https://www.gnu.org/licenses/>. 

using System;
using UnityEngine;

namespace PsyForge.GUI {

    public class TopTextOverlay : TextOverlay<TopTextOverlay> {
        /// <summary>
        /// Sets the size of the text box.
        /// </summary>
        /// <param name="size"></param>
        /// <exception cref="Exception"></exception>
        protected override void ResizeBox(TextOverlayBoxSize size) {
            gameObject.GetComponent<RectTransform>().anchorMin =
                size == TextOverlayBoxSize.Small ? new Vector2(0f, 0.94f)
                : size == TextOverlayBoxSize.Normal ? new Vector2(0f, 0.89f)
                : size == TextOverlayBoxSize.Large ? new Vector2(0f, 0.79f)
                : throw new Exception($"Invalid TextOverlayBoxSize {Enum.GetName(typeof(TextOverlayBoxSize), size)}");
        }
    }

}