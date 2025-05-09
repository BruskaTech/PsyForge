//Copyright (c) 2025 University of Bonn (James Bruska)
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
using PsyForge.Localization;
using System.Threading;


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
        // Devices that can be accessed by managed scripts, but are also pre-fabs
        //////////
        private VideoControl _videoControl = null;
        public VideoControl videoControl {
            get {
                if (!isVideoControlAvailable) {
                    throw new Exception("VideoControl not found in scene."
                        + "\nPlease add the VideoPlayer prefab to the scene."
                        + "\n\nThis can be found at PsyForge/Runtime/Prefabs/VideoPlayer/VideoPlayer.prefab");
                }
                return _videoControl;
            }
            private set => _videoControl = value;
        }
        public bool isVideoControlAvailable => _videoControl != null;

        private SoundRecorder _recorder = null;
        public SoundRecorder recorder {
            get {
                if (!isSoundRecorderAvailable) {
                    throw new Exception("SoundRecorder not found in scene."
                        + "\nPlease add the SoundRecorder prefab to the scene."
                        + "\n\nThis can be found at PsyForge/Runtime/Prefabs/SoundRecorder/SoundRecorder.prefab");
                }
                return _recorder;
            }
            private set => _recorder = value;
        }
        public bool isSoundRecorderAvailable => _recorder != null;

        //////////
        // Devices that can be accessed by managed scripts
        //////////
        public HostPC hostPC;
        public SyncBoxes syncBoxes = new();

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
            Config.Init();
        }

        protected async Task LoadSyncBox(string name) {
            SyncBox syncBox;
            try {
                var syncBoxObj = new GameObject(name);
                var syncBoxTypePath = $"PsyForge.ExternalDevices.{name}, PsyForge";
                syncBox = (SyncBox) syncBoxObj.AddComponentByName(syncBoxTypePath);
                DontDestroyOnLoad(syncBoxObj);
            } catch (Exception e) {
                throw new Exception($"Syncbox class {name} could not be created", e);
            }

            await syncBoxes.AddSyncBox(syncBox);
        }

        protected async void Start() {
            // Unity internal event handling
            SceneManager.sceneLoaded += onSceneLoaded;

            // Create objects not tied to unity
            // Nothing for now

            // Setup Configs
            var configs = await SetupConfigs();
            GetExperiments(configs);
            FileManager.CreateDataFolder();
            LangStrings.SetLanguage();

            // Setup Syncbox Interface
            if (!Config.isTest && Config.syncBoxOn) {
                await Task.WhenAll(Config.syncBoxClasses.Val.Select(LoadSyncBox));
                await syncBoxes.Init();
            }

            // Launch Startup Scene
            LaunchLauncher();
        }

        protected async Task<string[]> SetupConfigs() {
            await Config.SetupSystemConfig();

            // Get all configuration files
            return Config.GetExperimentConfigs();
        }

        protected void GetExperiments(string[] configs) {
            List<string> exps = new List<string>();

            UnityEngine.Debug.Log("Experiment Options:\n" + string.Join("\n", configs));
            for (int i = 0, j = 0; i < configs.Length; i++) {
                if (!configs[i].Contains(SYSTEM_CONFIG))
                    exps.Add(Path.GetFileNameWithoutExtension(configs[i]));
                j++;
            }
            Config.availableExperiments.Val = exps.ToArray();
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

        ConcurrentDictionary<PsyForge.Utilities.Timer, PsyForge.Utilities.Timer> timers = new();
        internal void AddTimerTS(PsyForge.Utilities.Timer timer) {
            timers.TryAdd(timer, timer);
        }
        internal bool TryRemoveTimerTS(PsyForge.Utilities.Timer timer) {
            return timers.TryRemove(timer, out _);
        }

        public void Pause(bool pause) {
            Do<Bool>(PauseHelper, pause);
        }
        protected void PauseHelper(Bool pause) {
            float oldTimeScale = 0;
            if (pause) {
                pauseTimescales.Push(Time.timeScale);
                Time.timeScale = 0;
                if (pauseTimescales.Count == 1) { // The pause actually started
                    foreach (var (timer, _) in timers) {
                        timer.Pause();
                    }
                }

            } else {
                if (pauseTimescales.TryPop(out oldTimeScale) ) {
                    Time.timeScale = oldTimeScale;
                    if (pauseTimescales.Count == 0) { // The pause actually ended
                        foreach (var (timer, _) in timers) {
                            timer.UnPause();
                        }
                    }
                }
            }
            if (isVideoControlAvailable) { videoControl.PauseVideo(oldTimeScale == 0); }
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
            manager.syncBoxes?.StopContinuousPulsing();
            manager.syncBoxes?.TearDown();

            // TODO: JPB: (feature) Make EventLoops stop gracefully by awaiting the stop with a timeout that gets logged if triggered
            foreach (var eventLoop in eventLoops) {
                _ = eventLoop.Abort();
            }

            EventReporter.Instance.LogTS("experiment quitted");
            await Delay(500);
            this.Quit();
        }

        // Helpful functions
        public void LockCursor(CursorLockMode lockMode) {
            Do(LockCursorHelper, lockMode);
        }
        public void LockCursorTS(CursorLockMode lockMode) {
            DoTS(LockCursorHelper, lockMode);
        }
        public void LockCursorHelper(CursorLockMode lockMode) {
            Cursor.lockState = lockMode;
            Cursor.visible = lockMode != CursorLockMode.Locked;
        }

        public CursorLockMode CursorLockState() {
            return DoGet(CursorLockStateHelper);
        }
        public async Task<CursorLockMode> CursorLockStateTS() {
            return await DoGetTS(CursorLockStateHelper);
        }
        public CursorLockMode CursorLockStateHelper() {
            return Cursor.lockState;
        }

        // Timing Functions
        public async Awaitable Delay(int millisecondsDelay, bool pauseAware = true, CancellationToken ct = default) {
            if (millisecondsDelay < 0) {
                throw new ArgumentOutOfRangeException($"millisecondsDelay <= 0 ({millisecondsDelay})");
            } else if (millisecondsDelay == 0) {
                return;
            }

            PsyForge.Utilities.Timer timer = new(millisecondsDelay, pauseAware);
            while (!timer.IsFinished()) {
                await Awaitable.NextFrameAsync(ct);
            }
        }
        public async Task DelayTS(int millisecondsDelay, bool pauseAware = true, CancellationToken ct = default) {
            if (millisecondsDelay != 0) {
                await DoWaitForTS(Delay, millisecondsDelay, pauseAware, ct);
            }
        }
    }
}
