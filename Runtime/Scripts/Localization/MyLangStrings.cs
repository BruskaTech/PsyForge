//Copyright (c) 2024 Jefferson University (James Bruska)
//Copyright (c) 2024 Bruska Technologies LLC (James Bruska)

//This file is part of PsyForge.
//PsyForge is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//PsyForge is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with PsyForge. If not, see <https://www.gnu.org/licenses/>. 

namespace PsyForge.Localization {

    public static partial class LangStrings {
        public static LangString Blank() { return GenForCurrLang(""); }
        public static LangString NewLine() { return GenForCurrLang("\n"); }
        public static LangString DoubleNewLine() { return GenForCurrLang("\n\n"); }

        public static LangString ErrorTitle() { return new( new() {
            { Language.English, "Error" },
            { Language.German, "Fehler" },
        }); }
        public static LangString ErrorFooter() { return new( new() {
            { Language.English, "Press [Q] to quit" },
            { Language.German, "Drücken Sie [Q], um das Programm zu beenden" },
        }); }
        public static LangString WarningTitle() { return new( new() {
            { Language.English, "Warning" },
            { Language.German, "Warnung" },
        }); }
        public static LangString WarningFooter() { return new( new() {
            { Language.English, "Press [Enter] to continue or press [Q] to quit" },
            { Language.German, "Drücken Sie [Enter], um fortzufahren, oder drücken Sie [Q], um aufzuhören" },
        }); }
        public static LangString ShowInstructionVideo() { return new( new() {
            { Language.English, "Press any key to show instruction video" },
            { Language.German, "Drücken Sie eine beliebige Taste, um das Anleitungsvideo anzuzeigen" },
        }); }
        public static LangString MicrophoneTestTitle() { return new( new() {
            { Language.English, "Microphone Test" },
            { Language.German, "Mikrofontest" },
        }); }
        public static LangString MicrophoneTest() { return new( new() {
            { Language.English, "Press any key to record a sound after the beep." },
            { Language.German, "Drücken Sie eine beliebige Taste, um nach dem Signalton einen Ton aufzunehmen." },
        }); }
        public static LangString ConfirmStart() { return new( new() {
            { Language.English, "Please let the experimenter know if you have any questions about the task."
                + "\n\nIf you think you understand, please explain the task to the experimenter in your own words." },
            { Language.German, "Bitte informieren Sie den Versuchsleiter, wenn Sie Fragen zur Aufgabe haben."
                + "\n\nWenn Sie glauben, die Aufgabe zu verstehen, erklären Sie sie bitte dem Versuchsleiter in Ihren eigenen Worten." },
        }); }
        public static LangString ExperimentQuit() { return new( new() {
            { Language.English, "Do you want to quit?\n\nPress [Y] to Quit.\nPress [N] to Resume." },
            { Language.German, "Möchten Sie aufhören?\n\nDrücken Sie [Y], um aufzuhören.\nDrücken Sie [N], um fortzufahren." },
        }); }
        public static LangString ExperimentPaused() { return new( new() {
            { Language.English, "<b>Paused</b>\n\nPress [P] to unpause." },
            { Language.German, "<b>Pausiert</b>\n\nDrücken Sie [P], um fortzufahren." },
        }); }
        public static LangString RepeatIntroductionVideo() { return new( new() {
            { Language.English, "Press [Y] to continue, \n Press [N] to replay instructional video." },
            { Language.German, "Drücken Sie [Y], um fortzufahren, \n Drücken Sie [N], um das Anleitung erneut abzuspielen." },
        }); }
        public static LangString RepeatMicTest() { return new( new() {
            { Language.English, "Did you hear the recording? \n([Y] = Continue / [N] = Try Again)." },
            { Language.German, "Haben Sie die Aufnahme gehört? \n([Y] = Fortfahren / [N] = Erneut versuchen)." },
        }); }
        public static LangString AnyKeyToContinue() { return new( new() {
            { Language.English, "\nPress any key to continue\n\n" },
            { Language.German, "\nDrücken Sie eine beliebige Taste, um fortzufahren\n\n" },
        }); }
        public static LangString SlideControlLine() { return new( new() {
            { Language.English, "\n(go backward) '[⬅]'   |   '[⮕]' (go forward)\n\n" },
            { Language.German, "\n(zurück) '[⬅]'   |   '[⮕]' (vorwärts)\n\n" },
        }); }
        public static LangString RatingQuestionnaireFooter() { return new( new() {
            { Language.English, "Press [Enter] to continue" },
            { Language.German, "Drücken Sie [Enter], um fortzufahren" },
        }); }
        public static LangString MicrophoneTestRecording() { return new( new() {
            { Language.English, "Recording..." },
            { Language.German, "Aufnahme..." },
        }); }
        public static LangString MicrophoneTestPlaying() { return new( new() {
            { Language.English, "Playing..." },
            { Language.German, "Wiedergabe..." },
        }); }
        public static LangString SubjectSessionConfirmation(string subject, int sessionNum, string experimentName) { return new( new() {
            { Language.English, $"Running {subject} in session {sessionNum} of {experimentName}."
                + "\n\nPress [Y] to continue, [N] to quit." },
            { Language.German, $"Patient {subject} in Session {sessionNum} für Experiment: {experimentName}."
                + "\n\nDrücken Sie [Y], um fortzufahren, [N], um aufzuhören." },
        }); }
        public static LangString VerbalRecallDisplay() { return new( new() {
            { Language.English, "*****" },
            { Language.German, "*****" },
        }); }
        public static LangString MathDistractorPreTrial() { return new( new() {
            { Language.English, "Press any key to start the math task." },
            { Language.German, "Drücken Sie eine beliebige Taste, um die Mathematikaufgabe zu starten." },
        }); }
        public static LangString ElememConnection() { return new( new() {
            { Language.English, "Waiting for Elemem connection..." },
            { Language.German, "Warte auf Elemem-Verbindung..." },
        }); }
        public static LangString IncompatibleTargetFrameRate(int targetFps, uint screenFps) { return new( new() {
            { Language.English, $"Config variable targetFrameRate ({targetFps}) should be a factor of the screen refresh rate ({screenFps})."
                + "\n\nIf you are using a new device or monitor, consider changing the targetFrameRate. Also consider what this means for your experiment."
                + "\n\nIf changing the frame rate is not possible for your experiment, then you can continue on with the old frame rate. Please note that this will mean the frames of the game do not necessarily align with the frames of the screen (impacting timing analyses)."
                + $"\n\nPress [Y] to continue with the provided frame rate ({targetFps}).\nPress [N] to quit." },
            { Language.German, $"Die Konfigurationsvariable targetFrameRate ({targetFps}) sollte ein Bruchteil der Bildwiederholrate des Bildschirms ({screenFps}) sein."
                + "\n\nWenn Sie ein neues Gerät oder einen neuen Monitor verwenden, erwägen Sie, die targetFrameRate zu ändern. Bedenken Sie auch, was das für Ihr Experiment bedeutet."
                + "\n\nWenn es für Ihr Experiment nicht möglich ist, die Bildrate zu ändern, können Sie mit der alten Bildrate fortfahren. Bitte beachten Sie, dass dies bedeutet, dass die Frames des Spiels nicht unbedingt mit den Frames des Bildschirms übereinstimmen (was die Timing-Analysen beeinflusst)."
                + $"\n\nDrücken Sie [Y], um mit der angegebenen Bildrate ({targetFps}) fortzufahren.\nDrücken Sie [N], um aufzuhören." },
        }); }

        public static LangString StartupExperimentLauncher() { return new( new() {
            { Language.English, "Experiment Launcher" },
            { Language.German, "" },
        }); }
        public static LangString StartupExperiment() { return new( new() {
            { Language.English, "Experiment:" },
            { Language.German, "" },
        }); }
        public static LangString StartupSubject() { return new( new() {
            { Language.English, "Subject:" },
            { Language.German, "" },
        }); }
        public static LangString StartupSession() { return new( new() {
            { Language.English, "Session:" },
            { Language.German, "" },
        }); }
        public static LangString StartupParticipantCodePlaceholder() { return new( new() {
            { Language.English, "Participant code" },
            { Language.German, "" },
        }); }
        public static LangString StartupTestSyncboxButton() { return new( new() {
            { Language.English, "Test Syncbox" },
            { Language.German, "" },
        }); }
        public static LangString StartupLaunchButton(int sessionNum) { return new( new() {
            { Language.English, $"Start Session {sessionNum}" },
            { Language.German, "" },
        }); }
        public static LangString StartupGreyedLaunchButtonSelectExp() { return new( new() {
            { Language.English, "Please select an experiment" },
            { Language.German, "" },
        }); }
        public static LangString StartupGreyedLaunchButtonEnterParticipant() { return new( new() {
            { Language.English, "Please enter participant code" },
            { Language.German, "" },
        }); }
        public static LangString StartupGreyedLaunchButtonEnterValidParticipant() { return new( new() {
            { Language.English, "Please enter a <i>valid</i> participant code..." },
            { Language.German, "" },
        }); }
        public static LangString StartupGreyedLaunchButtonSyncboxTest() { return new( new() {
            { Language.English, "Please wait, syncbox test running..." },
            { Language.German, "" },
        }); }
        public static LangString StartupLoadingButton() { return new( new() {
            { Language.English, "Loading..." },
            { Language.German, "" },
        }); }
    }
}
