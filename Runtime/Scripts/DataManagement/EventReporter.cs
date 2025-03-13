﻿//Copyright (c) 2024 Jefferson University (James Bruska)
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
using Unity.Collections;
using UnityEngine;
using UnityEngine.PlayerLoop;
using PsyForge.Extensions;

using PsyForge.Threading;
using PsyForge.Utilities;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System.Net;

namespace PsyForge.DataManagement {
    [DefaultExecutionOrder(-998)]
    public class EventReporter : SingletonEventMonoBehaviour<EventReporter> {
        public enum FORMAT { JSON_LINES };

        private EventReporterLoop eventReporterLoop;

        public bool experimentConfigured = false;
        protected bool eventWrittenThisFrame = false;

        protected override void AwakeOverride() {
            eventReporterLoop = new();
        }
        protected async void Start() {
            // await eventReporterLoop.CheckDataDirectory();
            while (!Config.IsSystemConfigSetup()) { await Awaitable.NextFrameAsync(); }
            if (Config.logFrameDisplayTimes) {
                StartCoroutine(LogFrameDisplayTimes());
            }
        }

        private IEnumerator LogFrameDisplayTimes() {
            DateTime lastFrameTime = Clock.UtcNow;
            var waitForEndOfFrame = new WaitForEndOfFrame();
            Application.targetFrameRate = -1;
            QualitySettings.vSyncCount = 1;
            while (true) {
                yield return waitForEndOfFrame;
                DateTime now = Clock.UtcNow;
                if (experimentConfigured && eventWrittenThisFrame) {
                    // Debug.Log($"LogFrameDisplayTimes: {(now - lastFrameTime).TotalMilliseconds} {Time.frameCount} {now.ConvertToMillisecondsSinceEpoch()}");
                    LogTS("frameDisplayed", now, new() { 
                        { "frame", Time.frameCount },
                        { "timeSinceLastFrameMs", (now - lastFrameTime).TotalMilliseconds }
                    });
                }
                // Debug.Log($"LogFrameDisplayTimes 2: {(now - lastFrameTime).TotalMilliseconds} {Time.frameCount} {now.ConvertToMillisecondsSinceEpoch()}");
                eventWrittenThisFrame = false;
                lastFrameTime = now;
            }
        }
        // protected void FixedUpdate() {
        //     Debug.Log($"frameStarted: {Clock.UtcNow.ConvertToMillisecondsSinceEpoch()}");
        // }

        public void LogTS(string type, Dictionary<string, object> data = null) {
            LogTS(type, Clock.UtcNow, data);
        }
        public void LogTS(string type, DateTime time, Dictionary<string, object> data = null) {
            manager?.hostPC?.SendLogMsgIfConnectedTS(type, time, data ?? new());
            LogLocalTS(type, time, data);
        }

        // Do not use this unless you don't want the message logged to the HostPC or any other location.
        public void LogLocalTS(string type, DateTime time, Dictionary<string, object> data = null) {
            if (OnUnityThread()) { eventWrittenThisFrame = true; }
            eventReporterLoop.LogTS(type, time, data);
        }

        protected class EventReporterLoop : EventLoop {
            const FORMAT outputFormat = FORMAT.JSON_LINES;
            const string extensionlessFileName = "session";

            protected readonly string defaultFilePath = "";

            protected string filePath = "";
            protected int eventId = 0;

            public EventReporterLoop() {
                string directory = FileManager.DataPath();
                switch (outputFormat) {
                    case FORMAT.JSON_LINES:
                        filePath = Path.Combine(directory, extensionlessFileName + ".jsonl");
                        break;
                }
                defaultFilePath = filePath;
                File.Create(defaultFilePath).Close();
            }

            public async Task CheckDataDirectory() {
                var dir = Path.GetDirectoryName(filePath);
#if !UNITY_WEBGL // System.IO
                File.Create(dir).Close();
#else // UNITY_WEBGL
                // var webReq = UnityWebRequest.Get(dir + "/");
                // await webReq.SendWebRequest();
                // Debug.Log("CheckDataDirectory: " + webReq.result);
                // if (webReq.result != UnityWebRequest.Result.Success) {
                //     Debug.Log(webReq.error);
                // }

                // if (webReq.result == UnityWebRequest.Result.ConnectionError) {
                //     throw new WebException($"Failed to fetch {dir} due to connection error\n\n({webReq.error})");
                // } else if (webReq.result == UnityWebRequest.Result.ProtocolError) {
                //     if (webReq.responseCode == 404) {
                //         throw new WebException($"The directory {dir} does not exist on the server\n\n({webReq.error})");
                //     } else {
                //         throw new WebException($"Failed to fetch {dir} due to protocol error\n\n({webReq.error})");
                //     }
                // } else if (webReq.result == UnityWebRequest.Result.DataProcessingError) {
                //     throw new WebException($"Failed to fetch {dir} due to data processing error\n\n({webReq.error})");
                // }
#endif // UNITY_WEBGL
            }

            public void LogTS(string type, DateTime time, Dictionary<string, object> data = null) {
                NativeDataPoint dataPoint = new(type, -1, time, data);
                if (cts.IsCancellationRequested) { return; } // Ignore log attempts after thread is ended
                DoTS(LogHelper, dataPoint);
            }

            protected void LogHelper(NativeDataPoint dataPoint) {
                dataPoint.id = eventId++;
                DoWrite(dataPoint);
                dataPoint.Dispose();
            }


            protected void DoWrite(NativeDataPoint dataPoint) {
                if (filePath == defaultFilePath) {
                    var sessionPath = FileManager.SessionPath();
                    if (sessionPath != null) {
                        switch (outputFormat) {
                            case FORMAT.JSON_LINES:
                                filePath = Path.Combine(sessionPath, extensionlessFileName + ".jsonl");
                                break;
                        }
                    }
                }

                // This was an idea for stopping the hanging loop that happens when there is no configs folder
                // TODO: JPB: (bug) If there is no configs folder with configs.json inside it, the program hangs
                // if (!File.Exists(filePath)) {
                //     return;
                // }

                string lineOutput = "Unrecognized DataReporter FORMAT";
                switch (outputFormat) {
                    case FORMAT.JSON_LINES:
                        lineOutput = dataPoint.ToJSON();
                        break;
                }
#if !UNITY_WEBGL // System.IO
                File.AppendAllText(filePath, lineOutput + Environment.NewLine);
#else // UNITUY_WEBGL
                // TODO: JPB: (needed) (feature) Get WebGL to write events back to the server
#endif // UNITY_WEBGL
            }
        }
    }
}