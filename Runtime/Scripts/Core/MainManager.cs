//Copyright (c) 2024 Jefferson University (James Bruska)
//Copyright (c) 2024 Bruska Technologies LLC (James Bruska)
//Copyright (c) 2023 University of Pennsylvania (James Bruska)

//This file is part of PsyForge.
//PsyForge is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//PsyForge is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with PsyForge. If not, see <https://www.gnu.org/licenses/>. 

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;


using PsyForge.DataManagement;
using PsyForge.Extensions;
using PsyForge.ExternalDevices;
using PsyForge.GUI;
using PsyForge.Threading;
using PsyForge.Utilities;


namespace PsyForge {

    [DefaultExecutionOrder(-999)]
    public class MainManager : SingletonEventMonoBehaviour<MainManager> {
        public static new MainManager Instance {
            get {
                var instance = SingletonEventMonoBehaviour<MainManager>.Instance;
                if (instance == null) {
                    throw new InvalidOperationException("MainManager not initialized. The starting scene of the game MUST be the manager scene.");
                }
                return instance;
            }
        }

        const string SYSTEM_CONFIG = "config.json";

        //////////
        // ???
        //////////
        protected ConcurrentStack<float> pauseTimescales = new();

        //////////
        // Devices that can be accessed by managed
        // scripts
        //////////
        public HostPC hostPC;
        public VideoControl videoControl;
        public SoundRecorder recorder;

        private GameObject syncBoxObj;
        public SyncBox syncBox;

        //////////
        // Provided AudioSources
        //////////
        public AudioSource highBeep;
        public AudioSource lowBeep;
        public AudioSource lowerBeep;
        public AudioSource playback;

        //////////
        // Event Loop Handling
        //////////
        internal ConcurrentBag<EventLoop> eventLoops = new();
        internal ConcurrentQueue<IEnumerator> events = new();
        internal ConcurrentQueue<IEnumerator> unpausableEvents = new();

        //////////
        // StartTime
        //////////
        
        /// <summary>
        /// The time the game was started
        /// </summary>
        public DateTime StartTimeTS { get; private set; }
        /// <summary>
        /// The time since the game was started
        /// </summary>
        public TimeSpan TimeSinceStartupTS {
            get { return Clock.UtcNow - StartTimeTS; }
        }

        //////////
        // Setup
        //////////

        protected async void OnDestroy() {
            await QuitTS();
        }

        void Update() {
            while (events.TryDequeue(out IEnumerator e)) {
                StartCoroutine(e);
            }
            while (unpausableEvents.TryDequeue(out IEnumerator e)) {
                StartCoroutine(e, true);
            }
        }

        protected override void AwakeOverride() {
            // Make the MainManager StartTimeTS match the unity start time
            StartTimeTS = Clock.UtcNow - TimeSpan.FromSeconds(Time.realtimeSinceStartup);
        }

        protected async void Start() {
            // Unity internal event handling
            SceneManager.sceneLoaded += onSceneLoaded;

            // Create objects not tied to unity
            // Nothing for now

            // Setup Configs
            var configs = SetupConfigs();
            GetExperiments(configs);
            FileManager.CreateDataFolder();
            LangStrings.SetLanguage();

            // Setup Syncbox Interface
            if (!Config.isTest && Config.syncBoxOn) {
                try {
                    syncBoxObj = new GameObject("Syncbox");
                    var syncBoxTypePath = $"PsyForge.ExternalDevices.{Config.syncBoxClass}, PsyForge";
                    syncBox = (SyncBox) syncBoxObj.AddComponentByName(syncBoxTypePath);
                    DontDestroyOnLoad(syncBoxObj);
                } catch (Exception e) {
                    throw new Exception($"Syncbox class {Config.syncBoxClass} could not be created", e);
                }

                await syncBox.Init();
            }

            // Launch Startup Scene
            LaunchLauncher();
        }

        protected string[] SetupConfigs() {
#if !UNITY_WEBGL // System.IO
            Config.SetupSystemConfig(FileManager.ConfigPath());
#else // !UNITY_WEBGL
            Config.SetupSystemConfig(Application.streamingAssetsPath);
#endif // !UNITY_WEBGL

            // Get all configuration files
            string configPath = FileManager.ConfigPath();
            string[] configs = Directory.GetFiles(configPath, "*.json");
            if (configs.Length < 2) {
                throw new Exception("Configuration File Error. Missing system or experiment configuration file in configs folder");
            }
            return configs;
        }

        protected void GetExperiments(string[] configs) {
            List<string> exps = new List<string>();

            UnityEngine.Debug.Log("Experiment Options:\n" + string.Join("\n", configs));
            for (int i = 0, j = 0; i < configs.Length; i++) {
                if (!configs[i].Contains(SYSTEM_CONFIG))
                    exps.Add(Path.GetFileNameWithoutExtension(configs[i]));
                j++;
            }
            Config.availableExperiments = exps.ToArray();
        }

        //////////
        // Collect references to managed objects
        // and release references to non-active objects
        //////////
        private void onSceneLoaded(Scene scene, LoadSceneMode mode) {
            // TODO: JPB: (needed) Check
            //onKey = new ConcurrentQueue<Action<string, bool>>(); // clear keyhandler queue on scene change

            // Voice Activity Detector
            //GameObject voice = GameObject.Find("VAD");
            //if (voice != null) {
            //    voiceActity = voice.GetComponent<VoiceActivityDetection>();
            //    Debug.Log("Found VoiceActivityDetector");
            //}

            // Video Control
            GameObject video = GameObject.Find("VideoPlayer");
            if (video != null) {
                videoControl = video.GetComponent<VideoControl>();
                video.SetActive(false);
                Debug.Log("Initalized VideoPlayer");
            }

            // Beep Sounds
            GameObject sound = GameObject.Find("Sounds");
            if (sound != null) {
                lowBeep = sound.transform.Find("LowBeep").gameObject.GetComponent<AudioSource>();
                lowerBeep = sound.transform.Find("LowerBeep").gameObject.GetComponent<AudioSource>();
                highBeep = sound.transform.Find("HighBeep").gameObject.GetComponent<AudioSource>();
                playback = sound.transform.Find("Playback").gameObject.GetComponent<AudioSource>();
                Debug.Log("Initialized Sounds");
            }

            // Sound Recorder
            GameObject soundRecorder = GameObject.Find("SoundRecorder");
            if (soundRecorder != null) {
                recorder = soundRecorder.GetComponent<SoundRecorder>();
                Debug.Log("Initialized Sound Recorder");
            }
        }

        protected void LaunchLauncher() {
            // Reset external hardware state if exiting task
            hostPC?.SendExitMsgTS();

            //mainEvents.Pause(true);
            for (int i = 0; i < SceneManager.sceneCount; ++i) {
                UnityEngine.Debug.Log(SceneManager.GetSceneAt(i).name);
            }
            SceneManager.LoadScene(Config.launcherScene);
        }

        // These can be called by anything
        
        // public void PauseTS(bool pause) {
        //     // This is ONLY done for pause because it is a special case
        //     unpausableEvents.Enqueue(PauseHelperEnumerator(pause));
        // }
        // protected IEnumerator PauseHelperEnumerator(Bool pause) {
        //     PauseHelper(pause);
        //     yield return null;
        // }
        public void Pause(bool pause) {
            Do<Bool>(PauseHelper, pause);
        }
        protected void PauseHelper(Bool pause) {
            // TODO: JPB: (needed) Implement pause functionality correctly
            float oldTimeScale = 0;
            if (pause) {
                pauseTimescales.Push(Time.timeScale);
                Time.timeScale = 0;
            } else {
                if (pauseTimescales.TryPop(out oldTimeScale) ) {
                    Time.timeScale = oldTimeScale;
                }
            }
            if (videoControl != null) { videoControl.PauseVideo(oldTimeScale == 0); }
        }
        public bool IsPausedTS() {
            return IsPausedHelper();
        }
        public bool IsPaused() {
            return DoGet(IsPausedHelper);
        }
        protected bool IsPausedHelper() {
            return pauseTimescales.Count > 0;
        }

        public async Task QuitTS() {
            hostPC?.QuitTS();
            await DoWaitForTS(QuitHelper);
        }
        protected async Task QuitHelper() {
            // TODO: JPB: (needed) (bug) Fix QuitHelper quitting display
            TextDisplayer.Instance.Display("quitting", text: LangStrings.GenForCurrLang("Quitting..."));
            manager.syncBox?.StopContinuousPulsing();
            manager.syncBox?.TearDown();

            // TODO: JPB: (feature) Make EventLoops stop gracefully by awaiting the stop with a timeout that gets logged if triggered
            foreach (var eventLoop in eventLoops) {
                _ = eventLoop.Abort();
            }

            EventReporter.Instance.LogTS("experiment quitted");
            await Delay(500);
            this.Quit();
        }

        // Helpful functions
        public void LockCursor(CursorLockMode isLocked) {
            Do(LockCursorHelper, isLocked);
        }
        public void LockCursorTS(CursorLockMode isLocked) {
            DoTS(LockCursorHelper, isLocked);
        }
        public void LockCursorHelper(CursorLockMode isLocked) {
            UnityEngine.Cursor.lockState = isLocked;
            UnityEngine.Cursor.visible = isLocked == CursorLockMode.None;
        }

        // Timing Functions
        public async Task DelayWithAction(int millisecondsDelay, Action<TimeSpan> action) {
            await ToCoroutineTask(DelayWithActionE(millisecondsDelay, action));
        }
        public IEnumerator DelayWithActionE(int millisecondsDelay, Action<TimeSpan> action) {
            return DoWaitFor(DelayEHelper, millisecondsDelay, action);
        }
        public async Task DelayTS(int millisecondsDelay) {
            if (millisecondsDelay != 0) {
                await DoWaitForTS(DelayEHelper, millisecondsDelay);
            }
        }
        public async Task Delay(int millisecondsDelay) {
            if (millisecondsDelay != 0) {
                await ToCoroutineTask(DelayE(millisecondsDelay));
            }
        }
        private IEnumerator DoNothing() {
            yield break;
        }
        public IEnumerator DelayE(int millisecondsDelay) {
            if (millisecondsDelay == 0) { return DoNothing(); }
            return DoWaitFor(DelayEHelper, millisecondsDelay);
        }
        public IEnumerator DelayEHelper(int millisecondsDelay) {
            return DelayEHelper(millisecondsDelay, null);
        }
        public IEnumerator DelayEHelper(int millisecondsDelay, Action<TimeSpan> action) {
            if (millisecondsDelay < 0) {
                throw new ArgumentOutOfRangeException($"millisecondsDelay <= 0 ({millisecondsDelay})");
            } else if (millisecondsDelay == 0) {
                yield break;
            }

            yield return new Delay(millisecondsDelay, action);
        }
    }

    class Delay : CustomYieldInstruction {
        private TimeSpan timeRemaining;
        private Action<TimeSpan> action;
        private DateTime lastTime;

        public Delay(double seconds, Action<TimeSpan> action = null) {
            timeRemaining = TimeSpan.FromSeconds(seconds);
            this.action = action;
            lastTime = Clock.UtcNow;
        }

        public Delay(int millisecondsDelay, Action<TimeSpan> action = null) {
            timeRemaining = TimeSpan.FromMilliseconds(millisecondsDelay);
            this.action = action;
            lastTime = Clock.UtcNow;
        }

        public override bool keepWaiting {
            get {
                if (MainManager.Instance.IsPaused()) { return true; }
                var time = Clock.UtcNow;
                var diff = time - lastTime;
                timeRemaining -= diff;
                lastTime = time;
                action?.Invoke(timeRemaining > TimeSpan.Zero ? timeRemaining : TimeSpan.Zero);
                return timeRemaining > TimeSpan.Zero;
            }
        }
    }
}
