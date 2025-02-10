//Copyright (c) 2025 University of Bonn (James Bruska)
//Copyright (c) 2024 Columbia University (James Bruska)
//Copyright (c) 2024 Jefferson University (James Bruska)
//Copyright (c) 2024 Bruska Technologies LLC (James Bruska)

//This file is part of PsyForge.
//PsyForge is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//PsyForge is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with PsyForge. If not, see <https://www.gnu.org/licenses/>. 

#nullable enable
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. The class is static...
#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

namespace PsyForge {
    public static partial class Config {
        // System Settings

        /// <summary>
        /// This is the language of the experiment.
        /// </summary>
        public static Conf<string> language;

        /// <summary>
        /// This will cause the experiment to log all network messages.
        /// </summary>
        public static Conf<bool> logNetworkMessages;
        /// <summary>
        /// This will cause all EventLoop errors to have full stack traces that include where it came from.
        /// <br/>This is not turned on by default because it is very slow (since has to create a backtrace for every Do function).
        /// </summary>
        public static ConfOptionalStruct<bool> debugEventLoopExtendedStackTrace;
        /// <summary>
        /// The IP for the host server.
        /// </summary>
        public static Conf<string> hostServerIP;
        /// <summary>
        /// The port for the host server.
        /// </summary>
        public static Conf<int> hostServerPort;
        /// <summary>
        /// This will cause the experiment to use the Elemem.
        /// </summary>
        public static Conf<bool> elememOn;
        /// <summary>
        /// The interval in milliseconds for the elemem heartbeat.
        /// </summary>
        public static Conf<int> elememHeartbeatInterval;

        // Syncbox

        /// <summary>
        /// This will cause the experiment to use the sync box.
        /// </summary>
        public static Conf<bool> syncBoxOn;
        /// <summary>
        /// This will cause the experiment to use the sync box for continuous pulsing.
        /// <br/>The other option is to call the Pulse() function manually in your experiment.
        /// <br/> This should likely be put in the experiment config file.
        /// </summary>
        public static Conf<bool> syncBoxContinuousPulsing;
        /// <summary>
        /// The class for the sync box.
        /// <br/>The class must be in the namespace PsyForge.ExternalDevices. This may require a reference assembly definition. Check the "Adding a SyncBox" section of the documentation for more information.
        /// </summary>
        public static Conf<string> syncBoxClass;
        /// <summary>
        /// The duration of the sync box test in milliseconds.
        /// </summary>
        public static Conf<int> syncBoxTestDurationMs;
        
        /// <summary>
        /// The width of the photo diode image in inches
        /// </summary>
        public static Conf<float> photoDiodeSyncBoxImageWidthInch;
        /// <summary>
        /// The height of the photo diode image in inches
        /// </summary>
        public static Conf<float> photoDiodeSyncBoxImageHeightInch;
        /// <summary>
        /// Which corner of the screen of the photo diode image is in
        /// <br/>The first element is the horizontal position (0 = left, 1 = right)
        /// <br/>The second element is the vertical position (0 = bottom, 1 = top)
        /// </summary>
        public static Conf<uint[]> photoDiodeSyncBoxImagePosition;
        /// <summary>
        /// The color of the photo diode image when it is off
        /// <br/>This is done as an HTML color string. In other words, it can be a hex code or a color name.
        /// </summary>
        public static Conf<string> photoDiodeSyncBoxImageOffColor;
        /// <summary>
        /// The color of the photo diode image when it is on
        /// <br/>This is done as an HTML color string. In other words, it can be a hex code or a color name.
        /// </summary>
        public static Conf<string> photoDiodeSyncBoxImageOnColor;

        // Hardware

        /// <summary>
        /// This will allow the expeiment to use a PS4 controller.
        /// </summary>
        // public static bool ps4Controller;


        // Programmer Conveniences

        /// <summary>
        /// This will show the FPS on the screen at all times.
        /// <br/>NOT IMPLEMENTED YET
        /// </summary>
        // public static bool showFps;
        /// <summary>
        /// This is a flag to determine if the application is in test mode.
        /// <br/>The default should be: false
        /// </summary>
        public static Conf<bool> isTest;

        // ExperimentBase.cs
        /// <summary>
        /// The duration of the microphone test in milliseconds.
        /// </summary>
        public static Conf<int> micTestDurationMs;
        /// <summary>
        /// The path to the introduction video.
        /// </summary>
        public static Conf<string> introductionVideo;

        // Local variables


        /// <summary>
        /// The subject ID.
        /// <br/>DO NOT INCLUDE IN CONFIG FILE (it will be set automatically by the experiment launcher)
        /// </summary>
        public static Conf<string> subject;

        /// <summary>
        /// The session number.
        /// <br/>DO NOT INCLUDE IN CONFIG FILE (it will be set automatically by the experiment launcher)
        /// </summary>
        public static ConfOptionalStruct<int> sessionNum;
        /// <summary>
        /// The list of available experiments.
        /// <br/>DO NOT INCLUDE IN CONFIG FILE (it will be set automatically by the MainManager)
        /// </summary>
        public static Conf<string[]> availableExperiments;

        // MainManager.cs

        /// <summary>
        /// The target frame rate of the application.
        /// <br/>If it is not set, then the game will try to run as fast as the monitor/screen refresh rate.
        /// <br/>If you want it to run as fast as possible (on desktop), then set the value to -1. This does not work on mobile devices or the web.
        /// <br/>WARNING: Setting this to a smaller value than the screen refresh rate is NOT usually good for psychology experiments because it means the input logging will be less accurate (since it only happens once every frame)
        /// <br/>WARNING: Setting this to -1 (or a non-multiple of the screen refresh rate) is NOT usually good for psychology experiments because it causes a timing issue between when your game thinks it is showing something and when it actually shows up on the screen.
        /// <br/>
        /// </summary>
        public static ConfOptionalStruct<int> targetFrameRate;

        /// <summary>
        /// The name of the experiment scene.
        /// <br/>This is the scene that will be loaded when the experiment is launched.
        /// </summary>
        public static Conf<string> experimentScene;
        /// <summary>
        /// The experiment class to use.
        /// <br/>This class should always inherit from ExperimentBase (or something else that does).
        /// </summary>
        public static Conf<string> experimentClass;
        /// <summary>
        /// The name of the experiment.
        /// </summary>
        public static Conf<string> experimentName;
        /// <summary>
        /// The name of the launcher scene.
        /// <br/>The default should be: "Startup"
        /// </summary>
        public static Conf<string> launcherScene;

        // FileManager.cs

        /// <summary>
        /// The path to the data folder.
        /// <br/>DO NOT USE THIS VARIABLE DIRECTLY. Use FileManager.DataPath() instead.
        /// <br/>If not set, defaults to the location of the application (or desktop for development).
        /// </summary>
        internal static ConfOptionalClass<string> dataPath;
        /// <summary>
        /// The path to the wordpool file.
        /// </summary>
        public static Conf<string> wordpool;
        /// <summary>
        /// The path to the practice wordpool file.
        /// </summary>
        public static Conf<string> practiceWordpool;
        /// <summary>
        /// The regex for participant IDs.
        /// <br/>If set to "", any participant ID is valid.
        /// </summary>
        public static ConfOptionalClass<string> participantIdRegex;
        /// <summary>
        /// The regex for the prefix for participant IDs.
        /// <br/>If set to "", there is no expected prefix.
        /// </summary>
        public static ConfOptionalClass<string> participantIdPrefixRegex;
        /// <summary>
        /// The regex for the postfix for participant IDs.
        /// If set to "", there is no expected postfix.
        /// </summary>
        public static ConfOptionalClass<string> participantIdPostfixRegex;


        // ExperimentBase.cs
        
        /// <summary>
        /// This will allow the experiment to quit at any time by pressing the quit button ('Q').
        /// </summary>
        public static Conf<bool> quitAnytime;
        /// <summary>
        /// This will allow the experiment to pause at any time by pressing the pause button ('P').
        /// </summary>
        public static Conf<bool> pauseAnytime;
        /// <summary>
        /// This causes the experiment to log the display times of each frame.
        /// <br/> This is much closer to the actual time things are displayed on the screen 
        ///     as opposed to the time they changed in the code.
        /// <br/> This is the closest thing you can get to when the frame is actually displayed on the screen in Unity.
        /// <br/>
        /// <br/> NOTE: This value is not exact because: 
        /// <br/>    1) Unity is not exactly locked to the vBlank of the the monitor.
        ///  <br/>   2) When the game lags this number will be off.
        /// <br/>
        /// <br/> If you need something better, then you will need to do one of the following:
        /// <br/>    1) Make an EventLoop that waits for the vBlank of the monitor and logs it
        /// <br/>    2) Make an EventLoop that waits for a photodiode and logs it
        /// <br/>    3) Create a InputDevice in the new Unity InputSystem for your photodiode and log it
        ///             (https://www.youtube.com/watch?v=YNNVGGulscc)
        /// <br/>
        /// <br/> More info can be found at these locations:
        /// <br/>    https://discussions.unity.com/t/timing-of-waitforendofframe-relative-to-vertical-blank-onset/173764/3
        /// <br/>    https://discussions.unity.com/t/time-deltatime-not-constant-vsync-camerafollow-and-jitter/639394/280
        /// </summary>
        public static Conf<bool> logFrameDisplayTimes;

        // ElememInterface.cs

        /// <summary>
        /// The type of stimulation to use in this experiment.
        /// <br/>The options are: ReadOnly, OpenLoop, and ClosedLoop
        /// </summary>
        public static Conf<string> stimMode;
    }
}