//Copyright (c) 2024 Columbia University (James Bruska)
//Copyright (c) 2024 Jefferson University (James Bruska)
//Copyright (c) 2024 Bruska Technologies LLC (James Bruska)
//Copyright (c) 2023 University of Pennsylvania (James Bruska)

//This file is part of PsyForge.
//PsyForge is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//PsyForge is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with PsyForge. If not, see <https://www.gnu.org/licenses/>. 

using System;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using TMPro;

using PsyForge.DataManagement;
using PsyForge.Extensions;
using PsyForge.GUI;
using PsyForge.Threading;
using PsyForge.Localization;
using UnityEngine.UI;


namespace PsyForge {

    [DefaultExecutionOrder(-997)]
    public class ErrorNotifier : SingletonEventMonoBehaviour<ErrorNotifier> {
        public TextMeshProUGUI titleElement;
        public TextMeshProUGUI textElement;
        public TextMeshProUGUI footerElement;

        private bool errorSet = false;

        protected override void AwakeOverride() {
            Application.logMessageReceivedThreaded += (string logString, string stackTrace, LogType type) => {
                if (type == LogType.Exception) {
                    int exceptionIdx = logString.IndexOf("Exception: ");
                    if (exceptionIdx != -1) {
                        logString = logString.Substring(exceptionIdx + 11);
                    }
                    if (!Config.IsSystemConfigSetup()) {
                        ErrorHelper(logString.ToNativeText(), stackTrace.ToNativeText());
                    } else {
                        DoTS(ErrorHelper, logString.ToNativeText(), stackTrace.ToNativeText());
                    }
                }
            };

            titleElement.text = "";
            textElement.text = "";
            footerElement.text = "";
            gameObject.SetActive(false);
        }

        protected async void ErrorHelper(NativeText message, NativeText stackTrace) {
            try {
                gameObject.SetActive(true);
                // Only show first error on screen, but report all errors
                Debug.LogError($"Error: {message}\n{stackTrace}");
                if (!errorSet) {
                    errorSet = true;
                    titleElement.text = LangStrings.ErrorTitle().Color("red");
                    textElement.text = message.ToString();
                    footerElement.text = LangStrings.ErrorFooter();
                }
                if (Config.IsSystemConfigSetup()) { // This stops an unrecoverable error when EventReport.Instance isn't awake yet
                    eventReporter.LogTS("Error", new() {
                        { "message", message.ToStringAndDispose() },
                        { "stackTrace", stackTrace.ToStringAndDispose() } });
                }
                await Awaitable.NextFrameAsync(); // Without this lines, you can hit an infinite loop
                manager.Pause(true);
                await InputManager.Instance.WaitForKey(KeyCode.Q, true);
                manager.Quit();
            } catch (Exception e) {
                Debug.LogError("UNSAVEABLE ERROR IN ErrorHelper... Quitting...\n" + e);
                Debug.Assert(gameObject != null);
                Debug.Assert(manager != null);
                Debug.Assert(InputManager.Instance != null);
                Debug.Assert(EventReporter.Instance != null);
                manager.Quit();
            }
        }

        /// <summary>
        /// Throws an error and logs it to the event reporter.
        /// <br/>
        /// I'm not sure when you would want to use this over just throwing an exception, 
        /// but I provide it in case it is useful to someone.
        /// </summary>
        /// <param name="exception"></param>
        /// <exception cref="Exception"></exception>
        public static void ErrorTS(Exception exception) {
            if (!IsInstatiated) {
                throw new Exception("THIS SHOULD NOT HAPPEN! ErrorNotifier was accessed before it's awake method has been called.", exception);
            }

            var msg = exception.Message == "" ? exception.GetType().Name : exception.Message;
            var stackTrace = exception.StackTrace ?? "";
            Instance.DoTS(Instance.ErrorHelper, msg.ToNativeText(), stackTrace.ToNativeText());
            throw new Exception("ErrorNotifier", exception);
        }

        public static void WarningTS(Exception exception) {
            if (exception.StackTrace == null) {
                try { // This is used to get the stack trace
                    throw exception;
                } catch (Exception e) {
                    exception = e;
                }
            }

            // TODO: JPB: (bug) What will happen to the error in WarningTS if it errors
            _ = Instance.DoWaitFor(Instance.WarningHelper, exception.Message.ToNativeText(), exception.StackTrace.ToNativeText());
            throw new NotImplementedException("WarningTS");
        }
        // TODO: JPB: (feature) Implement WarningHelper
        protected Task WarningHelper(NativeText message, NativeText stackTrace) {
            gameObject.SetActive(true);
            manager.Pause(true);
            titleElement.text = LangStrings.WarningTitle().Color("yellow");
            textElement.text = message.ToString();
            footerElement.text = LangStrings.WarningFooter();
            Debug.Log($"Warning: {message}\n{stackTrace}");
            eventReporter.LogTS("Warning", new() {
                { "message", message.ToString() },
                { "stackTrace", stackTrace.ToString() } });
            message.Dispose();
            stackTrace.Dispose();

            // var keyCode = await InputManager.Instance.WaitForKey(new() {KeyCode.Y, KeyCode.N}, true);
            // if ()

            manager.Pause(false);
            return Task.CompletedTask;
        }
    }
}