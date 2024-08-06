//Copyright (c) 2024 Columbia University (James Bruska)
//Copyright (c) 2024 Jefferson University (James Bruska)
//Copyright (c) 2024 Bruska Technologies LLC (James Bruska)

//This file is part of UnityEPL.
//UnityEPL is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//UnityEPL is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with UnityEPL. If not, see <https://www.gnu.org/licenses/>. 

using System;

namespace UnityEPL {
    public static partial class Config {
        // System Settings

        /// <summary>
        /// This will cause the experiment to log all network messages.
        /// </summary>
        public static bool logNetworkMessages { get { return GetSetting<bool>("logNetworkMessages"); } }
        /// <summary>
        /// This will cause all EventLoop errors to have full stack traces that include where it came from.
        /// This is not turned on by default because it is very slow (since has to create a backtrace for every Do function).
        /// </summary>
        public static bool debugEventLoopExtendedStackTrace { get { return GetSetting<bool>("debugEventLoopExtendedStackTrace"); } }
        /// <summary>
        /// The IP for the host server.
        /// </summary>
        public static string hostServerIP { get { return GetSetting<string>("hostServerIP"); } }
        /// <summary>
        /// The port for the host server.
        /// </summary>
        public static int hostServerPort { get { return GetSetting<int>("hostServerPort"); } }
        /// <summary>
        /// This will cause the experiment to use Ramulator.
        /// </summary>
        public static bool ramulatorOn { get { return GetSetting<bool>("ramulatorOn"); } }
        /// <summary>
        /// This will cause the experiment to use the Elemem.
        /// </summary>
        public static bool elememOn { get { return GetSetting<bool>("elememOn"); } }
        /// <summary>
        /// The interval in milliseconds for the elemem heartbeat.
        /// </summary>
        public static int elememHeartbeatInterval { get { return GetSetting<int>("elememHeartbeatInterval"); } }

        // Hardware

        /// <summary>
        /// This will cause the experiment to use the sync box.
        /// NOT IMPLEMENTED YET
        /// </summary>
        public static bool syncboxOn { get { throw new NotImplementedException(); } } //return GetSetting<bool>("syncboxOn"); } }
        /// <summary>
        /// The duration of the sync box test in milliseconds.
        /// </summary>
        public static int syncBoxTestDurationMs { get { return GetSetting<int>("syncBoxTestDurationMs"); } }
        /// <summary>
        /// This will allow the expeiment to use a PS4 controller.
        /// NOT IMPLEMENTED YET
        /// </summary>
        public static bool ps4Controller { get { throw new NotImplementedException(); } } //return GetSetting<bool>("ps4Contoller"); } }

        // Programmer Conveniences

        /// <summary>
        /// This will show the FPS on the screen at all times.
        /// NOT IMPLEMENTED YET
        /// </summary>
        public static bool showFps { get { throw new NotImplementedException(); } } //return GetSetting<bool>("showFps"); } }
        /// <summary>
        /// This is a flag to determine if the application is in test mode.
        /// The default should be: false
        /// </summary>
        public static bool isTest { get { return GetSetting<bool>("isTest"); } }

        // ExperimentBase.cs
        /// <summary>
        /// The duration of the microphone test in milliseconds.
        /// </summary>
        public static int micTestDurationMs { get { return GetSetting<int>("micTestDurationMs"); } }
        /// <summary>
        /// The path to the introduction video.
        /// </summary>
        public static string introductionVideo { get { return GetSetting<string>("introductionVideo"); } }

        // Local variables
        
#nullable enable
        /// <summary>
        /// The subject ID.
        /// DO NOT INCLUDE IN CONFIG FILE (it will be set automatically by the experiment launcher)
        /// </summary>
        public static string? subject {
            get { return GetOptionalClassSetting<string>("subject"); }
            internal set { SetSetting("subject", value); }
        }
#nullable disable
        /// <summary>
        /// The session number.
        /// DO NOT INCLUDE IN CONFIG FILE (it will be set automatically by the experiment launcher)
        /// </summary>
        public static int? sessionNum {
            get { return GetOptionalSetting<int>("session"); }
            internal set { SetSetting("session", value); }
        }
        /// <summary>
        /// The list of available experiments.
        /// DO NOT INCLUDE IN CONFIG FILE (it will be set automatically by the MainManager)
        /// </summary>
        public static string[] availableExperiments {
            get { return GetSetting<string[]>("availableExperiments"); }
            internal set { SetSetting("availableExperiments", value); }
        }

        // InterfaceManager.cs

        /// <summary>
        /// This adjusts the vSync count of the application (what multiple of the screen frame rate the game should run at).
        /// The default should be: 1
        /// For more information: https://docs.unity3d.com/ScriptReference/QualitySettings-vSyncCount.html
        /// </summary>
        public static int vSync { get { return GetSetting<int>("vSync"); } }
        /// <summary>
        /// This adjusts the target frame rate of the application.
        /// The default should be: -1
        /// This should really only be used on mobile applications.
        /// Also, beware that setting this value to something that is not a multiple of the screen refresh rate can cause issues about when you assume something ACTUALLY shows up on the screen...
        /// For more information: https://docs.unity3d.com/ScriptReference/Application-targetFrameRate.html
        /// </summary>
        public static int frameRate { get { return GetSetting<int>("frameRate"); } }

        public static string experimentScene { get { return GetSetting<string>("experimentScene"); } }
        /// <summary>
        /// The experiment class to use.
        /// This class should always inherit from ExperimentBase (or something else that does).
        /// </summary>
        public static string experimentClass { get { return GetSetting<string>("experimentClass"); } }
        /// <summary>
        /// The name of the experiment.
        /// </summary>
        public static string experimentName { get { return GetSetting<string>("experimentName"); } }
        /// <summary>
        /// The name of the launcher scene.
        /// The default should be: "Startup"
        /// </summary>
        public static string launcherScene { get { return GetSetting<string>("launcherScene"); } }

        // FileManager.cs

#nullable enable
        /// <summary>
        /// DO NOT USE THIS VARIABLE DIRECTLY. Use FileManager.DataPath() instead.
        /// The path to the data folder.
        /// If not set, defaults to the location of the application (or desktop for development).
        /// </summary>
        public static string? dataPath { get { return GetOptionalClassSetting<string>("dataPath"); } }
        /// <summary>
        /// The path to the wordpool file.
        /// </summary>
        public static string wordpool { get { return GetSetting<string>("wordpool"); } }
        /// <summary>
        /// The path to the practice wordpool file.
        /// </summary>
        public static string practiceWordpool { get { return GetSetting<string>("practiceWordpool"); } }
        /// <summary>
        /// The regex for participant IDs.
        /// If set to "", any participant ID is valid.
        /// </summary>
        public static string? participantIdRegex { get { return GetOptionalClassSetting<string>("participantIdRegex"); } }
        /// <summary>
        /// The prefix for participant IDs.
        /// If set to "", there is no expected prefix.
        /// </summary>
        public static string? participantIdPrefixRegex { get { return GetOptionalClassSetting<string>("participantIdPrefixRegex"); } }
        /// <summary>
        /// If set to "", there is no expected postfix.
        /// </summary>
        public static string? participantIdPostfixRegex { get { return GetOptionalClassSetting<string>("participantIdPostfixRegex"); } }
#nullable disable

        // ExperimentBase.cs
        
        /// <summary>
        /// This will allow the experiment to quit at any time by pressing the quit button ('Q').
        /// </summary>
        public static bool quitAnytime { get { return GetSetting<bool>("quitAnytime"); } }
        /// <summary>
        /// This will allow the experiment to pause at any time by pressing the pause button ('P').
        /// </summary>
        public static bool pauseAnytime { get { return GetSetting<bool>("pauseAnytime"); } }

        // ElememInterface.cs

        /// <summary>
        /// The type of stimulation to use in this experiment.
        /// The options are: ReadOnly, OpenLoop, and ClosedLoop
        /// </summary>
        public static string stimMode { get { return GetSetting<string>("stimMode"); } }
    }
}