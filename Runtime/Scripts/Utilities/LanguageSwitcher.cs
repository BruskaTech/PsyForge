//Copyright (c) 2024 Jefferson University (James Bruska)
//Copyright (c) 2024 Bruska Technologies LLC (James Bruska)
//Copyright (c) 2023 University of Pennsylvania (James Bruska)

//This file is part of UnityEPL.
//UnityEPL is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//UnityEPL is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with UnityEPL. If not, see <https://www.gnu.org/licenses/>. 


using System;
using System.Collections.Generic;
using Unity.Collections;

namespace UnityEPL {

    // TODO: JPB: (needed) Figure out partial enum for Language so it is extensible to the user.
    public enum Language {
        English = 0,
        German = 1,
        Spanish = 2,
    }

    // Maybe change name to LangStrs or LangCtrl and chage LangString to LangStr
    public static partial class LangStrings {
        public static Language Language {get; private set;} = Language.English;

        /// <summary>
        /// Set the current language for the LangStrings
        /// </summary>
        /// <param name="lang">The language to set</param>
        public static void SetLanguage(Language lang) {
            Language = lang;
        }

        /// <summary>
        /// Generate a LangString for all languages
        /// ONLY USE THIS FUNCTION IF YOU ARE SURE THE STRING IS THE SAME FOR ALL LANGUAGES
        /// Examples of this would be: numbers, file paths, subject ids, rich text formatting, etc.
        /// </summary>
        /// <param name="val">The string value for all languages</param>
        /// <returns>A LangString with the value for all languages</returns>
        public static LangString GenForAllLangs(string val) {
            Dictionary<Language, string> strings = new();
            foreach (Language lang in Enum.GetValues(typeof(Language))) {
                strings.Add(lang, val);
            }
            return new(strings);
        }
        
        /// <summary>
        /// Generate a LangString for the current language
        /// ONLY USE THIS FUNCTION IF YOU ARE SURE THE STRING IS THE SAME FOR ALL LANGUAGES
        /// Examples of this would be: numbers, file paths, subject ids, rich text formatting, etc.
        /// </summary>
        /// <param name="val">The string value for the current language</param>
        /// <returns>A LangString with the value for the current language</returns>
        public static LangString GenForCurrLang(string val) {
            return new(new() { { Language, val } });
        }

        public static LangString Blank() { return GenForCurrLang(""); }
        public static LangString NewLine() { return GenForCurrLang("\n"); }

        public static LangString Error() { return new( new() {
            { Language.English, "Error" },
        }); }
        public static LangString Warning() { return new( new() {
            { Language.English, "Warning" },
        }); }
        public static LangString ShowInstructionVideo() { return new( new() {
            { Language.English, "Press any key to show instruction video" },
        }); }
        public static LangString MicrophoneTestTitle() { return new( new() {
            { Language.English, "Microphone Test" },
        }); }
        public static LangString MicrophoneTest() { return new( new() {
            { Language.English, "Press any key to record a sound after the beep." },
        }); }
        public static LangString ConfirmStart() { return new( new() {
            { Language.English, "Please let the experimenter know if you have any questions about the task.\n\n" +
                "If you think you understand, please explain the task to the experimenter in your own words.\n\n" +
                "Press any key to continue to start." },
        }); }
        public static LangString PauseBeforeTrial(uint trialNum) { return new( new() {
            { Language.English, $"Press any key to start Trial {trialNum}." },
        }); }
        public static LangString PauseBeforePracticeTrial(uint trialNum) { return new( new() {
            { Language.English, $"Press any key to start Practice Trial {trialNum}." },
        }); }
        public static LangString ExperimentQuit() { return new( new() {
            { Language.English, "Do you want to quit\nPress Y to Quit, N to Resume." },
        }); }
        public static LangString RepeatIntroductionVideo() { return new( new() {
            { Language.English, "Press Y to continue, \n Press N to replay instructional video." },
        }); }
        public static LangString RepeatMicTest() { return new( new() {
            { Language.English, "Did you hear the recording ? \n(Y = Continue / N = Try Again)." },
        }); }
        public static LangString SlideControlLine() { return new( new() {
            { Language.English, "\n\n(go backward) '<-'   |   '->' (go forward) " },
        }); }
        public static LangString MicrophoneTestRecording() { return new( new() {
            { Language.English, "Recording..." },
        }); }
        public static LangString MicrophoneTestPlaying() { return new( new() {
            { Language.English, "Playing..." },
        }); }
        public static LangString SubjectSessionConfirmation(string subject, int sessionNum, string experimentName) { return new( new() {
            { Language.English, $"Running {subject} in session {sessionNum} of {experimentName}."
                + "\nPress Y to continue, N to quit." },
        }); }
        public static LangString VerbalRecallDisplay() { return new( new() {
            { Language.English, "*****" },
        }); }
        public static LangString ElememConnection() { return new( new() {
            { Language.English, "Waiting for Elemem connection..." },
        }); }
        public static LangString RamulatorConnection() { return new( new() {
            { Language.English, "Waiting for Ramulator connection..." },
        }); }


        // TODO: JPB: (needed) Move these LangStrings to MyLanguageSwitcher.cs
        public static LangString RepeatSpatialRecallSelection() { return new( new() {
            { Language.English, "Do you want to select this location?\n\nPress Y to continue, \n Press N to select another spot." },
        }); }
        public static LangString IntroSlidesGeneralTitle() { return new( new() {
            { Language.English, "Instructions" },
        }); }
        public static LangString IntroSlidesGeneral1() { return new( new() {
            { Language.English, "In this task, you will try to remember different items and their locations around a city block."
                + "\n\nThere will be multiple rounds of exploring the city block, with different items for each round."
                + "\n\nThe coming slides will tell you about each phase of a single round." },
        }); }
        public static LangString IntroSlidesGeneral2() { return new( new() {
            { Language.English, "In this task, you will try to remember different items and their locations around a city block."
                + "\n\nThere will be multiple rounds of exploring the city block, with different items for each round."
                + "\n\nThe coming slides will tell you about each phase of a single round." },
        }); }
        public static LangString IntroSlidesEncodingTitle() { return new( new() {
            { Language.English, "Instructions (Item Presentation)" },
        }); }
        public static LangString IntroSlidesEncoding() { return new( new() {
            { Language.English, "You will first be driven around the city block and shown items to remember." },
        }); }
        public static LangString IntroSlidesMathDistractorTitle() { return new( new() {
            { Language.English, "Instructions (Math)" },
        }); }
        public static LangString IntroSlidesMathDistractor() { return new( new() {
            { Language.English, "Then you will have to solve a number of math problems."
                + "\n\nUse the number keys on your keyboard to write your answer."
                + "\nUse the 'Enter' key on your keyboard to submit your answer." },
        }); }
        public static LangString IntroSlidesVerbalFreeRecallTitle() { return new( new() {
            { Language.English, "Instructions (Word Recall)" },
        }); }
        public static LangString IntroSlidesVerbalFreeRecall() { return new( new() {
            { Language.English, "You will then try to verbally recall all of the items that you saw on this round." },
        }); }
        public static LangString IntroSlidesSpatialRecallTitle() { return new( new() {
            { Language.English, "Instructions (Location Recall)" },
        }); }
        public static LangString IntroSlidesSpatialRecall() { return new( new() {
            { Language.English, "Each item you saw (and some items you didn't) will then be shown to you and you must do two things."
                + "\n\nFirst, select whether you 1-'remember' the item, 2-'recognize' the item, or if the item was 3-'unseen'."
                + "\nYou can select your choice by typing the 'left arrow', 'down arrow', or 'right arrow' on the keyboard and then typing 'Enter' to submit it."
                + "\n\nSecond, if you 'remember' or 'recognize' the item, you will drive around the city block and select where you think you saw it."
                + "\nYou can drive forward with the 'up arrow', backward with the 'down arrow' and type 'Enter' to submit your location." },
        }); }
        public static LangString IntroSlidesScoreDisplayTitle() { return new( new() {
            { Language.English, "Instructions (Score)" },
        }); }
        public static LangString IntroSlidesScoreDisplay() { return new( new() {
            { Language.English, "Finally, you will be presented with your score for the round."
                + "\n\nDon't worry if you only get one or two item location(s) right. This is totally normal! Just keep trying." },
        }); }
        public static LangString IntroSlidesAskStaffTitle() { return new( new() {
            { Language.English, "Instructions (Questions)" },
        }); }
        public static LangString IntroSlidesAskStaff() { return new( new() {
            { Language.English, "Please ask the staff now if you have any questions." },
        }); }
        public static LangString DisplayScoresCongratulations() { return new( new() {
            { Language.English, "Congratulations! You got half of the spatial recalls correct!\n" },
        }); }
        public static LangString DisplayScoresEncoragement() { return new( new() {
            { Language.English, "It's very hard! Keep trying!\n" },
        }); }
        public static LangString DisplayScoresRecall(int correctRecalls, int closeRecalls, int incorrectRecalls, int totalRecalls) { return new( new() {
            { Language.English, $"You got {correctRecalls} out of {totalRecalls} correct.\n"
            + $"You got {closeRecalls} out of {totalRecalls} close.\n"
            + $"You got {incorrectRecalls} out of {totalRecalls} incorrect.\n"
            + "\nPress any key to continue." },
        }); }
        public static LangString RepeatPracticeQuestion() { return new( new() {
            { Language.English, "Would you like to do another practice?"
                + "\n\nPress Y to do another practice round."
                + "\nPress N to continue to the real task." },
        }); }
        public static LangString SessionEnd() { return new( new() {
            { Language.English, "Yay! Session Complete." },
        }); }
    }

    public class LangString {
        private readonly Dictionary<Language, string> strings;

        // Make a langstring enumerable constructor that uses a dictionary, for convenience
        public LangString(Dictionary<Language, string> strings) {
            if (strings.Count <= 0) { throw new ArgumentException($"{nameof(LangString)} must have at least one language provided"); }
            this.strings = strings;
        }
        
        public override string ToString() {
            var language = LangStrings.Language;
            if (!strings.ContainsKey(language)) {
                throw new Exception($"The current {nameof(LangString)} does not have a {Enum.GetName(typeof(Language), language)} option available.\n{strings.ToJSON()}");
            }
            return strings[language];
        }
        public static implicit operator string(LangString str) {
            return str.ToString();
        }
        public NativeText ToNativeText() {
            return ToString().ToNativeText();
        }
        public static LangString operator +(LangString str1, LangString str2) {
            Dictionary<Language, string> strings = new();
            foreach (Language lang in Enum.GetValues(typeof(Language))) {
                if (str1.strings.ContainsKey(lang) && str2.strings.ContainsKey(lang)) {
                    strings.Add(lang, str1.strings[lang] + str2.strings[lang]);
                }
            }
            return new(strings);
        }

        /// <summary>
        /// Color the text of the LangString using RichText
        /// </summary>
        /// <param name="color">The color to use in RichText format</param>
        /// <returns>A new LangString with the color applied</returns>
        public LangString Color(string color) {
            Dictionary<Language, string> strings = new();
            foreach (Language lang in Enum.GetValues(typeof(Language))) {
                if (this.strings.ContainsKey(lang)) {
                    strings.Add(lang, $"<color={color}>{this.strings[lang]}</color>");
                }
            }
            return new(strings);
        }
    }

}
