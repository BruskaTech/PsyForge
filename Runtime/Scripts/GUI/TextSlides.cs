//Copyright (c) 2024 Columbia University (James Bruska)
//Copyright (c) 2024 Jefferson University (James Bruska)
//Copyright (c) 2024 Bruska Technologies LLC (James Bruska)

//This file is part of PsyForge.
//PsyForge is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//PsyForge is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with PsyForge. If not, see <https://www.gnu.org/licenses/>. 

using PsyForge.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace PsyForge.GUI {

    /// <summary>
    /// This is a struct that is used to represent a slide of text.
    /// It can also used to display text incrementally.
    /// </summary>
    public readonly struct TextSlide {
        public readonly string description;
        public readonly LangString title;
        public readonly List<LangString> texts;

        /// <summary>
        /// Create a text slide with a single text string and no title.
        /// </summary>
        /// <param name="description"></param>
        /// <param name="text"></param>
        public TextSlide(string description, LangString text) 
            : this(description, null, text) { }
        /// <summary>
        /// Create a text slide with a single text string with a title.
        /// </summary>
        /// <param name="description"></param>
        /// <param name="title"></param>
        /// <param name="text"></param>
        public TextSlide(string description, LangString title, LangString text) 
            : this(description, title, new List<LangString>() { text }) { }
        /// <summary>
        /// Create a text slide with incrementally displaying text strings and no title.
        /// </summary>
        /// <param name="description"></param>
        /// <param name="texts"></param>
        public TextSlide(string description, List<LangString> texts) 
            : this(description, null, texts) { }
        /// <summary>
        /// Create a text slide with incrementally displaying text strings and a title.
        /// </summary>
        /// <param name="description"></param>
        /// <param name="title"></param>
        /// <param name="texts"></param>
        public TextSlide(string description, LangString title, List<LangString> texts) {
            this.description = description;
            this.title = title;
            this.texts = new();
            for (int i = 1; i < texts.Count; i++) {
                // This recreates all the text strings so that they include eveything prior to the current text 
                //   and make everything after the current text transparent
                // The transparency is used to make the text appear in the same positions when added to the screen inrementally
                // No </alpha> is added because it affects the autosizing in TextMeshProUGUI (and isn't required)
                var incrementalStr = texts.GetRange(0, i).Aggregate((a, b) => a + b)
                    + LangStrings.GenForAllLangs("<alpha=#00>")
                    + texts.GetRange(i, texts.Count - i).Aggregate((a, b) => a + b);
                this.texts.Add(incrementalStr);           
            }
            this.texts.Add(texts.Aggregate((a, b) => a + b));
        }
    }

    /// <summary>
    /// This class is used to display a list of text slides.
    /// </summary>
    public static class TextSlides {
        /// <summary>
        /// Display a list of text slides.
        /// </summary>
        /// <param name="textSlides"></param>
        /// <returns></returns>
        public static async Task Display(List<TextSlide> textSlides) {
            // Create a list of all the text slides (including the incrementally displayed ones)
            List<TextSlide> slides = new();
            foreach (var slide in textSlides) {
                foreach (var text in slide.texts) {
                    slides.Add(new TextSlide(slide.description, slide.title, text));
                }
            }

            // Resize based on all text item sizes
            var strList = slides.Select(item => item.texts.Last()).ToList();
            var fontSize = (int)TextDisplayer.Instance.FindMaxFittingFontSize(strList, true, true, true);

            // Display all instruction texts
            var keys = new List<KeyCode>() { KeyCode.LeftArrow, KeyCode.RightArrow };
            int i = 0;
            while (i < slides.Count) {
                var slide = slides[i];
                TextDisplayer.Instance.Display(slide.description, slide.title, slide.texts.First(), LangStrings.SlideControlLine(), fontSize);

                var keyCode = await InputManager.Instance.WaitForKey(new KeyCode[2] {KeyCode.LeftArrow, KeyCode.RightArrow});
                if (keyCode == KeyCode.LeftArrow && i > 0) { i--; }
                else if (keyCode == KeyCode.RightArrow) { i++; }
            }
        }
    }

}