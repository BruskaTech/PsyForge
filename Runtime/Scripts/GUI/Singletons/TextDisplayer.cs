//Copyright (c) 2024 Columbia University (James Bruska)
//Copyright (c) 2024 Jefferson University (James Bruska)
//Copyright (c) 2024 Bruska Technologies LLC (James Bruska)
//Copyright (c) 2023 University of Pennsylvania (James Bruska)

//This file is part of PsyForge.
//PsyForge is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//PsyForge is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with PsyForge. If not, see <https://www.gnu.org/licenses/>. 

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using TMPro;
using System.Linq;

using PsyForge.Utilities;
using PsyForge.Threading;
using PsyForge.Extensions;

namespace PsyForge.GUI {

    public class TextDisplayer : SingletonEventMonoBehaviour<TextDisplayer> {
        /// <summary>
        /// Subscribe to this event to be notified of changes in the displayed text.
        /// 
        /// Single string argument is the new text which is being displayed.
        /// </summary>
        public delegate void TextDisplayed(string text);

        /// <summary>
        /// These text elements will all be updated when this monobehaviors public methods are used.
        /// </summary>
        public TextMeshProUGUI titleElement;
        public TextMeshProUGUI textElement;
        public TextMeshProUGUI footerElement;

        private RectTransform titleRect;
        private RectTransform textRect;
        private RectTransform footerRect;

        private Color[] originalColors;

        protected override void AwakeOverride() {
            titleRect = titleElement.GetComponentInParent<RectTransform>();
            textRect = textElement.GetComponentInParent<RectTransform>();
            footerRect = footerElement.GetComponentInParent<RectTransform>();
            gameObject.SetActive(false);
        }

        protected void Start() {
            originalColors = new Color[2];
            originalColors[0] = textElement.color;
            originalColors[1] = titleElement.color;
        }

        /// <summary>
        /// Hides the Text display by deactivating it
        /// </summary>
        public void Hide() {

            Do(HideHelper);
        }
        public void HideTS() {
            DoTS(HideHelper);
        }
        protected void HideHelper() {
            gameObject.SetActive(false);
            eventReporter.LogTS("text displayer hide");
        }

        /// <summary>
        /// Is the TextDisplayer Active
        /// </summary>
        /// <returns>if the TextDisplayer is active</returns>
        public bool IsActive() {
            return DoGet(IsActiveHelper);
        }
        public async Task<bool> IsActiveTS() {
            return await DoGetTS(IsActiveHelper);
        }
        protected Bool IsActiveHelper() {
            return gameObject.activeSelf;
        }

        /// <summary>
        /// Returns the color of the assigned text elements to whatever they were when this monobehavior initialized (usually scene load).
        /// </summary>
        public void OriginalColor() {
            Do(OriginalColorHelper);
        }
        public void OriginalColorTS() {
            DoTS(OriginalColorHelper);
        }     
        protected void OriginalColorHelper() {
            textElement.color = originalColors[0];
            titleElement.color = originalColors[1];
            eventReporter.LogTS("restore original text color", new());
        }

        // public void Display(string description, LangString title, LangString text, float textFontSize = 0) {
        //     Do(DisplayHelper, description.ToNativeText(), title.ToNativeText(), text.ToNativeText(), textFontSize);
        // }
        // protected void DisplayHelper(NativeText description, NativeText title, NativeText text, float textFontSize) {
        //     var displayedTitle = title.ToString();
        //     var displayedText = text.ToString();
                
        //     if (titleElement == null || textElement == null) {
        //         return;
        //     }

        //     if (textFontSize > 0) {
        //         textElement.enableAutoSizing = false;
        //         textElement.fontSize = textFontSize;
        //     }

        //     titleElement.text = displayedTitle;
        //     textElement.text = displayedText;
        //     Dictionary<string, object> dataDict = new() {
        //         { "displayed title", displayedTitle },
        //         { "displayed text", displayedText },
        //     };
        //     gameObject.SetActive(true);
        //     eventReporter.LogTS(description.ToString(), dataDict);

        //     description.Dispose();
        //     title.Dispose();
        //     text.Dispose();
        // }

        public async Task DisplayForTask(string description, LangString title, LangString text, LangString footer, float textFontSize, CancellationToken ct, Func<CancellationToken, Task> func) {
            // Remember the current state
            var activeOld = IsActive();
            var titleOld = titleElement.text;
            var textOld = textElement.text;
            var footerOld = footerElement.text;
            var textAutoSizingOld = textElement.enableAutoSizing;

            // Display the new text and wait for the task to complete
            Display(description, title, text, null, textFontSize);
            await Awaitable.NextFrameAsync();
            await func(ct);

            // Put the old state back
            titleElement.text = titleOld;
            textElement.text = textOld;
            footerElement.text = footerOld;
            textElement.enableAutoSizing = textAutoSizingOld;
            if (!activeOld) { Hide(); }
        }
        public async Task DisplayForTask(string description, LangString title, LangString text, LangString footer, CancellationToken ct, Func<CancellationToken, Task> func) {
            await DisplayForTask(description, title, text, footer, 0, ct, func);
        }


        public void Display(string description, LangString title = null, LangString text = null, LangString footer = null, float textFontSize = 0) {
            DisplayItems textDisplayerItems = new(title, text, footer);
            Do(DisplayHelper, description.ToNativeText(), textDisplayerItems, textFontSize);
        }
        public void DisplayTS(string description, LangString title = null, LangString text = null, LangString footer = null, float textFontSize = 0) {
            DisplayItems textDisplayerItems = new(title, text, footer);
            DoTS(DisplayHelper, description.ToNativeText(), textDisplayerItems, textFontSize);
        }
       protected void DisplayHelper(NativeText description, DisplayItems items, float textFontSize) {
            Dictionary<string, object> dataDict = new();

            // Setup text
            if (items.text.HasValue) {
                var text = items.text.Value.ToString();
                dataDict.Add("displayed text", text);
                textElement.text = text;

                textRect.anchorMax = new Vector2(textRect.anchorMax.x, 0.9f);
                textRect.anchorMin = new Vector2(textRect.anchorMin.x, 0.1f);
            } else {
                textElement.text = "";
            }

            // Setup title (and adjust text box if needed)
            if (items.title.HasValue) {
                var title = items.title.Value.ToString();
                dataDict.Add("displayed title", title);
                titleElement.text = title;

                titleRect.anchorMax = new Vector2(titleRect.anchorMax.x, 1f);
                titleRect.anchorMin = new Vector2(titleRect.anchorMin.x, 0.9f);

                textRect.anchorMax = new Vector2(textRect.anchorMax.x, 0.8f);
            } else {
                titleElement.text = "";
            }

            // Setup footer (and adjust text box if needed)
            if (items.footer.HasValue) {
                var footer = items.footer.Value.ToString();
                dataDict.Add("displayed footer", footer);
                footerElement.text = footer;

                footerRect.anchorMax = new Vector2(footerRect.anchorMax.x, 0.1f);
                footerRect.anchorMin = new Vector2(footerRect.anchorMin.x, 0f);

                textRect.anchorMin = new Vector2(textRect.anchorMin.x, 0.2f);
            } else {
                footerElement.text = "";
            }

            // Set the font size
            if (textFontSize > 0) {
                textElement.enableAutoSizing = false;
                textElement.fontSize = textFontSize;
            } else {
                textElement.enableAutoSizing = true;
            }

            // Show the text and log the event
            gameObject.SetActive(true);
            eventReporter.LogTS("text display show " + description.ToStringAndDispose(), dataDict);

            // Cleanup
            items.Dispose();
        }

        /// <summary>
        /// Clears the text of all textElements.  This is logged if the wordEventReporter field is populated in the editor.
        /// </summary>
        public void ClearText() {
            Do(ClearTextHelper);
        }
        public void ClearTextTS() {
            DoTS(ClearTextHelper);
        }
        protected void ClearTextHelper() {
            textElement.text = "";
            textElement.enableAutoSizing = true;
            eventReporter.LogTS("text display cleared text", new());
        }
       
        public void ClearTitle() {
            Do(ClearTitleHelper);
        }
        public void ClearTitleTS() {
            DoTS(ClearTitleHelper);
        }
        protected void ClearTitleHelper() {
            titleElement.text = "";
            eventReporter.LogTS("title display cleared title", new());
        }

        public void ClearOnly() {
            Do(ClearOnlyHelper);
        }
        public void ClearOnlyTS() {
            DoTS(ClearOnlyHelper);
        }
        protected void ClearOnlyHelper() {
            titleElement.text = "";
            textElement.text = "";
            footerElement.text = "";
            textElement.enableAutoSizing = true;
            eventReporter.LogTS("title display cleared", new());
        }

        public void Clear() {
            Do(ClearHelper);
        }
        public void ClearTS() {
            DoTS(ClearHelper);
        }
        protected void ClearHelper() {
            ClearOnlyHelper();
            HideHelper();
        }

        /// <summary>
        /// Returns the current text being displayed on the first textElement.  Throws an error if there are no textElements.
        /// </summary>
        public string CurrentText() {
            var text = DoGet(CurrentTextHelper);
            return text.ToStringAndDispose();
        }
        public async Task<string> CurrentTextTS() {
            var text = await DoGetTS(CurrentTextHelper);
            return text.ToStringAndDispose();
        }
        protected NativeText CurrentTextHelper() {
            if (textElement == null)
                throw new UnityException("There aren't any text elements assigned to this TextDisplayer.");
            return textElement.text.ToNativeText();
        }

        public float FindMaxFittingFontSize(List<LangString> strings, bool title, bool text, bool footer) {
            List<string> strs = strings.Select(str => str.ToString()).ToList();
            DisplayItems items = new(title ? LangStrings.Blank() : null, text ? LangStrings.Blank() : null, footer ? LangStrings.Blank() : null);
            return DoGet(FindMaxFittingFontSizeHelper, strs.ToNativeArray(), items);
        }
        public async Task<float> FindMaxFittingFontSizeTS(List<LangString> strings, bool title, bool text, bool footer) {
            List<string> strs = strings.Select(str => str.ToString()).ToList();
            DisplayItems items = new(title ? LangStrings.Blank() : null, text ? LangStrings.Blank() : null, footer ? LangStrings.Blank() : null);
            return await DoGetTS(FindMaxFittingFontSizeHelper, strs.ToNativeArray(), items);
        }
        protected float FindMaxFittingFontSizeHelper(NativeArray<NativeText> strings, DisplayItems items) {
            // Remember the current state (text, anchor min, and anchor max)
            var activeOld = IsActive();
            var titleOld = titleElement.text;
            var titleAnchorMaxOld = titleRect.anchorMax;
            var titleAnchorMinOld = titleRect.anchorMin;
            var textOld = textElement.text;
            var textAnchorMaxOld = textRect.anchorMax;
            var textAnchorMinOld = textRect.anchorMin;
            var textAutoSizingOld = textElement.enableAutoSizing;
            var footerOld = footerElement.text;
            var footerAnchorMaxOld = footerRect.anchorMax;
            var footerAnchorMinOld = footerRect.anchorMin;

            // Find the max fitting font size
            DisplayHelper("FindMaxFittingFontSize".ToNativeText(), items, 0);
            var size = textElement.FindMaxFittingFontSize(strings.ToListAndDispose());

            // Put the old state back
            titleElement.text = titleOld;
            titleRect.anchorMax = titleAnchorMaxOld;
            titleRect.anchorMin = titleAnchorMinOld;
            textElement.text = textOld;
            textRect.anchorMax = textAnchorMaxOld;
            textRect.anchorMin = textAnchorMinOld;
            textElement.enableAutoSizing = textAutoSizingOld;
            footerElement.text = footerOld;
            footerRect.anchorMax = footerAnchorMaxOld;
            footerRect.anchorMin = footerAnchorMinOld;
            if (!activeOld) { Hide(); }
            
            return size;
        }
    
        protected struct DisplayItems : IDisposable {
            public readonly NativeNullable<NativeText> title;
            public readonly NativeNullable<NativeText> text;
            public readonly NativeNullable<NativeText> footer;

            public DisplayItems(LangString title = null, LangString text = null, LangString footer = null) {
                if (title == null && text == null && footer == null) {
                    throw new ArgumentException("At least one of title, text, or footer must be non-null.");
                }
                this.title = new(title?.ToNativeText());
                this.text = new(text?.ToNativeText());
                this.footer = new(footer?.ToNativeText());
            }

            public void Dispose() {
                title.Dispose();
                text.Dispose();
                footer.Dispose();
            }
        }
    }

}