//Copyright (c) 2025 Columbia University
//Copyright (c) 2025 Bruska Technologies LLC

//This file is part of UnityExperiments.
//UnityExperiments is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//UnityExperiments is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with UnityExperiments. If not, see <https://www.gnu.org/licenses/>. 

using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Threading.Tasks;

using PsyForge;
using PsyForge.Extensions;
using PsyForge.Utilities;
using PsyForge.Experiment;
using System.Linq;
using System;


namespace PsyForge.GUI {

    [RequireComponent(typeof(RectTransform))]
    public class Questionnaire : SingletonEventMonoBehaviour<Questionnaire> {
        [SerializeField] protected TextMeshProUGUI questionText;
        [SerializeField] protected TextMeshProUGUI leftLabelText;
        [SerializeField] protected TextMeshProUGUI rightLabelText;
        [SerializeField] protected HorizontalLayoutGroup options;
        [SerializeField] protected TextMeshProUGUI footerText;

        protected override void AwakeOverride() {
            gameObject.SetActive(false);
        }

        protected void Update() {
            options.spacing = Screen.width / 13;
        }

        protected TextMeshProUGUI CreateOption(LangString option, float fontSize) {
            var obj = new GameObject(option);
            obj.transform.SetParent(options.transform, false);
            TextMeshProUGUI optionText = obj.AddComponent<TextMeshProUGUI>();
            optionText.text = option;
            optionText.fontSize = fontSize;
            optionText.alignment = TextAlignmentOptions.Center;
            optionText.font = questionText.font;
            optionText.fontSizeMax = 10000;
            return optionText;
        }

        public async Task RatingQuestionnaire(List<RatingQuestion> questions) {
            gameObject.SetActive(true);

            var questionStrs = questions.Select(x => x.question.ToString()).ToList();
            questionText.fontSize = questionText.FindMaxFittingFontSize(questionStrs);

            // Display all questions
            for (int i=0; i < questions.Count; ++i) {
                var question = questions[i];

                // Set question and footer text
                questionText.text = question.question;
                leftLabelText.text = question.leftLabel;
                rightLabelText.text = question.rightLabel;
                footerText.text = "";

                // Set font sizes
                var fontSize = leftLabelText.FindMaxFittingFontSize(new List<string> {question.leftLabel, question.rightLabel});
                leftLabelText.fontSize = fontSize;
                rightLabelText.fontSize = fontSize;
                footerText.fontSize = fontSize;
                
                // Create question options
                var optionTexts = new List<TextMeshProUGUI>();
                foreach (var option in question.options) {
                    optionTexts.Add(CreateOption(option, leftLabelText.fontSize*1.2f));
                }

                // Get participant choice
                var keyCode = KeyCode.None;
                var inputKeys = Enumerable.Range((int)KeyCode.Alpha1, question.options.Count).Select(i => (KeyCode)i)
                    .Concat(Enumerable.Range((int)KeyCode.Keypad1, question.options.Count).Select(i => (KeyCode)i)).ToList();
                int chosenOption = -1;
                while (keyCode != KeyCode.Return) {
                    // Wait for user input
                    keyCode = await InputManager.Instance.WaitForKey(inputKeys);

                    // Update footer
                    footerText.text = LangStrings.RatingQuestionnaireFooter();

                    // Update option highlighting
                    if (keyCode >= KeyCode.Alpha1 && keyCode <= KeyCode.Alpha9) {
                        chosenOption = keyCode - KeyCode.Alpha1 + 1;
                    } else if (keyCode >= KeyCode.Keypad1 && keyCode <= KeyCode.Keypad9) {
                        chosenOption = keyCode - KeyCode.Keypad1 + 1;
                    }
                    for (int j = 0; j < question.options.Count; j++) {
                        if (j == chosenOption-1) { optionTexts[j].color = new Color32(46, 238, 79, 255); }
                        else { optionTexts[j].color = Color.white; }
                    }

                    // Add ability to go to next question
                    if (!inputKeys.Contains(KeyCode.Return)) {
                        inputKeys.Add(KeyCode.Return);
                    }
                }

                // Log response
                eventReporter.LogTS("rating question", new() {
                    {"index", i},
                    {"question", question.question.ToString()},
                    {"leftLabel", question.leftLabel.ToString()},
                    {"rightLabel", question.rightLabel.ToString()},
                    {"options", question.options.Select(x => x.ToString()).ToList()},
                    {"choice", question.options[chosenOption-1].ToString()},
                });
                
                // Cleanup
                foreach (Transform child in options.transform) {
                    Destroy(child.gameObject);
                }
            }

            gameObject.SetActive(false);
        }
    }

    public class Question {
        public readonly LangString title;
        public readonly LangString question;
        public readonly List<LangString> options;
        public readonly LangString leftLabel;
        public readonly LangString rightLabel;

        public Question(LangString question, List<LangString> options, LangString leftLabel, LangString rightLabel) {
            this.question = question;
            this.options = options;
            this.leftLabel = leftLabel;
            this.rightLabel = rightLabel;

            if (options.Count < 2) {
                throw new ArgumentException("options must have at least 2 elements");
            }
        }
    }

    public class RatingQuestion : Question {
        public RatingQuestion(LangString question, int maxRating, LangString leftLabel, LangString rightLabel) :
            base(
                question, 
                Enumerable.Range(1, maxRating).Select(i => LangStrings.GenForAllLangs($"{i}")).ToList(), 
                leftLabel, 
                rightLabel
            ) {
                if (maxRating < 2) {
                    throw new ArgumentException("maxRating must be at least 2");
                } else if (maxRating > 9) {
                    throw new ArgumentException("maxRating must be at most 9");
                }
            }
    }

}