//Copyright (c) 2024 Jefferson University
//Copyright (c) 2023 University of Pennsylvania

//This file is part of UnityExperiments.
//UnityExperiments is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//UnityExperiments is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with UnityExperiments. If not, see <https://www.gnu.org/licenses/>. 

using TMPro;
using UnityEngine;
using System.Collections.Generic;

using PsyForge;
using PsyForge.Extensions;
using PsyForge.Utilities;

[RequireComponent(typeof(RectTransform))]
public abstract class TextOverlay<T> : SingletonEventMonoBehaviour<T> where T : TextOverlay<T> {
    [SerializeField] protected TextMeshProUGUI textBox;

    protected Dictionary<string, object> textData = null;

    protected override void AwakeOverride() {
        DontDestroyOnLoad(this.transform.parent.gameObject);
        gameObject.SetActive(false);
        textBox.text = "";
    }

    /// <summary>
    /// Turn off the text box and clear the text.
    /// </summary>
    public virtual void TurnOff() {
        ClearText();
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Set the text size of the text box to fit the largest text in the list.
    /// </summary>
    /// <param name="texts"></param>
    public virtual void SetTextSize(List<LangString> texts) {
        SetTextSize(texts.ConvertAll(text => text.ToString()));
    }

    /// <summary>
    /// Set the text size of the text box to fit the largest text in the list.
    /// </summary>
    /// <param name="texts"></param>
    public virtual void SetTextSize(List<string> texts) {
        gameObject.SetActive(true);
        int fontSize = (int)textBox.FindMaxFittingFontSize(texts);
        gameObject.SetActive(false);

        textBox.enableAutoSizing = false;
        textBox.fontSizeMax = fontSize;
        textBox.fontSize = fontSize;
    }

    /// <summary>
    /// Clear the text from the text box.
    /// </summary>
    public void ClearText() {
        eventReporter.LogTS($"clear {GetType().Name}", textData);
        textBox.text = "";
        textData = null;
    }

    /// <summary>
    /// Sets the size of the text box.
    /// It's abstract because the text box size may be different for different types of text overlays (ex: top vs bottom).
    /// </summary>
    /// <param name="size"></param>
    protected abstract void ResizeBox(TextOverlayBoxSize size);
    
    /// <summary>
    /// Display the text in the text box.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="data"></param>
    /// <param name="size"></param>
    public void DisplayText(LangString text, Dictionary<string, object> data = null, TextOverlayBoxSize size = TextOverlayBoxSize.Normal) {
        ResizeBox(size);

        textData = data != null ? new(data) : new();
        textData.Add("text", text.ToString());
        eventReporter.LogTS($"show {GetType().Name}", textData);

        gameObject.SetActive(true);
        textBox.text = text;
    }
}

/// <summary>
/// The size of the text overlay box.
/// </summary>
public enum TextOverlayBoxSize {
        Small,
        Normal,
        Large,
    }
