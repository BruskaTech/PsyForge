//Copyright (c) 2024 Columbia University (James Bruska)
//Copyright (c) 2024 Bruska Technologies LLC (James Bruska)

//This file is part of CityBlock.
//CityBlock is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//CityBlock is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with CityBlock. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

using PsyForge.DataManagement;
using PsyForge.Extensions;
using PsyForge.ExternalDevices;
using PsyForge.GUI;
using PsyForge.Utilities;
using PsyForge.Localization;
using System.Threading;

namespace PsyForge.Experiment {

    /// <summary>
    /// A simple Math Distractor task that asks participants to solve addition problems.
    /// Certain LangStrings will need to be defined in your LangStrings partial class.
    /// </summary>
    public class MathDistractor {
        protected readonly MainManager manager = MainManager.Instance;
        protected readonly InputManager inputManager = InputManager.Instance;
        protected readonly TextDisplayer textDisplayer = TextDisplayer.Instance;
        protected readonly EventReporter eventReporter = EventReporter.Instance;

        protected readonly int practiceDistractorDurationMs = 0;
        protected readonly int distractorDurationMs = 0;
        protected readonly bool mathDistractorTimeout = false;

        protected List<int> mathDistractorResponseTimesMs = new();
        protected int mathDistractorProblemTimeMs = -1;

        public MathDistractor(int practiceDistractorDurationMs, int distractorDurationMs, bool mathDistractorTimeout) {
            this.practiceDistractorDurationMs = practiceDistractorDurationMs;
            this.distractorDurationMs = distractorDurationMs;
            this.mathDistractorTimeout = mathDistractorTimeout;
        }

        public async Awaitable RunInitialTimings(CancellationToken ct = default) {
            await ExpHelpers.PressAnyKey("pre-trial math distractor", LangStrings.MathDistractorPreTrial(), ct);
            await Run(false, -1, true);
        }

        public async Awaitable Run(bool isPractice, int trialNum, bool setProblemTimings = false, CancellationToken ct = default) {
            ExpHelpers.SetExperimentStatus(HostPcStatusMsg.DISTRACT(trialNum));
            var trueDistractorDurationMs = isPractice ? practiceDistractorDurationMs : distractorDurationMs;

            int[] nums = new int[] {
                PsyForge.Utilities.Random.Rnd.Next(1, 10),
                PsyForge.Utilities.Random.Rnd.Next(1, 10),
                PsyForge.Utilities.Random.Rnd.Next(1, 10) };
            string message = "display math distractor problem";
            string problem = nums[0].ToString() + " + " +
                                nums[1].ToString() + " + " +
                                nums[2].ToString() + " = ";
            string answer = "";

            var startTime = Clock.UtcNow;
            var problemStartTime = startTime;
            while (true) {
                // Display the last changes
                textDisplayer.Display(message, text: LangStrings.GenForCurrLang(problem + answer));

                // Wait for next key
                KeyCode keyCode = KeyCode.Return;
                try {
                    if (mathDistractorTimeout && !setProblemTimings && mathDistractorProblemTimeMs > 0) {
                        int problemTimeLeftMs = (int)(problemStartTime - Clock.UtcNow).TotalMilliseconds + mathDistractorProblemTimeMs;
                        if (problemTimeLeftMs <= 0) { throw new TimeoutException(); }
                        keyCode = await inputManager.WaitForKey(ct: ct).Timeout(problemTimeLeftMs, new(), "MathDistractor timeout", true);
                    } else {
                        keyCode = await inputManager.WaitForKey(ct: ct);
                    }
                } catch (TimeoutException) {
                    textDisplayer.Display("math distractor timeout", text: LangStrings.GenForCurrLang(problem + answer));
                }
                var key = keyCode.ToString();

                // Enter only numbers
                if (IsNumericKeyCode(keyCode)) {
                    key = key[key.Length - 1].ToString(); // Unity gives numbers as Alpha# or Keypad#
                    if (answer.Length < 3) {
                        answer += key;
                    }
                    message = "modify math distractor answer";
                }
                // Delete key removes last character from answer
                else if (keyCode == KeyCode.Backspace || keyCode == KeyCode.Delete) {
                    if (answer != "") {
                        answer = answer.Substring(0, answer.Length - 1);
                    }
                    message = "modify math distractor answer";
                }
                // Submit answer
                else if (keyCode == KeyCode.Return || keyCode == KeyCode.KeypadEnter) {
                    int responseTimeMs = (int)(Clock.UtcNow - problemStartTime).TotalMilliseconds;
                    bool correct = answer != "" ? int.Parse(answer) == nums.Sum() : false;

                    // Play tone depending on right or wrong answer
                    if (correct) {
                        manager.lowBeep.Play();
                    } else {
                        manager.lowerBeep.Play();
                    }

                    // Set the problem time for the math distractor
                    mathDistractorResponseTimesMs.Add(responseTimeMs);
                    if (setProblemTimings) {
                        mathDistractorProblemTimeMs = (int)mathDistractorResponseTimesMs.Percentile(0.9);
                        eventReporter.LogTS("math distractor set problem time", new() {
                            { "problemTime", mathDistractorProblemTimeMs },
                            { "method", "90th percentile" },
                            { "responseTimes", mathDistractorResponseTimesMs }
                        });
                    }

                    // Report results
                    message = "math distractor answered";
                    textDisplayer.Display(message, text: LangStrings.GenForCurrLang(problem + answer));
                    Dictionary<string, object> dict = new() {
                        { "correct", correct },
                        { "problem", problem },
                        { "answer", answer },
                        { "responseTime", responseTimeMs }
                    };
                    eventReporter.LogTS(message, dict);
                    ExpHelpers.SetExperimentStatus(HostPcStatusMsg.MATH(correct, problem, answer, responseTimeMs));

                    // Show the answer for a bit
                    textDisplayer.Display(message,
                        text: LangStrings.GenForCurrLang(problem + "<color=" + (correct ? "green" : "red") + ">" + answer + "</color>"));
                    await manager.Delay(1000);

                    // End distractor or setup next math problem
                    if ((Clock.UtcNow - startTime).TotalMilliseconds > trueDistractorDurationMs) {
                        textDisplayer.Clear();
                        break;
                    } else {
                        nums = new int[] { PsyForge.Utilities.Random.Rnd.Next(1, 10),
                                        PsyForge.Utilities.Random.Rnd.Next(1, 10),
                                        PsyForge.Utilities.Random.Rnd.Next(1, 10) };
                        message = "display math distractor problem";
                        problem = nums[0].ToString() + " + " +
                                    nums[1].ToString() + " + " +
                                    nums[2].ToString() + " = ";
                        answer = "";
                        textDisplayer.Display(message, text: LangStrings.GenForCurrLang(problem + answer));
                        problemStartTime = Clock.UtcNow;
                    }
                }
                textDisplayer.Clear();
            }
        }

        protected static bool IsNumericKeyCode(KeyCode keyCode) {
                bool isAlphaNum = keyCode >= KeyCode.Alpha0 && keyCode <= KeyCode.Alpha9;
                bool isKeypadNum = keyCode >= KeyCode.Keypad0 && keyCode <= KeyCode.Keypad9;
                return isAlphaNum || isKeypadNum;
            }
    }
}