//Copyright (c) 2024 Columbia University (James Bruska)
//Copyright (c) 2024 Bruska Technologies LLC (James Bruska)

//This file is part of PsyForge.
//PsyForge is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//PsyForge is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with PsyForge. If not, see <https://www.gnu.org/licenses/>. 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

using PsyForge.DataManagement;
using PsyForge.ExternalDevices;
using PsyForge.GUI;
using PsyForge.Localization;

namespace PsyForge.Experiment {
    public static class ExpHelpers {
        // Keypress functions
        /// <summary>
        /// Display a message and wait for keypress
        /// </summary>
        /// <param name="description"></param>
        /// <param name="displayText"></param>
        /// <param name="displayText"></param>
        /// <returns></returns>
        public static async Task<KeyCode> PressAnyKey(string description, LangString displayText, CancellationToken ct = default) {
            return await PressAnyKey(description, null, displayText, ct);
        }
        public static async Task<KeyCode> PressAnyKey(string description, LangString displayTitle, LangString displayText, CancellationToken ct = default) {
            SetExperimentStatus(HostPcStatusMsg.WAITING());
            TextDisplayer.Instance.Display($"{description} (press any key prompt)", displayTitle, displayText, LangStrings.AnyKeyToContinue());
            var keyCode = await InputManager.Instance.WaitForKey(ct: ct);
            TextDisplayer.Instance.Clear();
            return keyCode;
        }

        public static void SetExperimentStatus(HostPcStatusMsg state, Dictionary<string, object> extraData = null) {
            var dict = (extraData ?? new()).Concat(state.dict).ToDictionary(x=>x.Key,x=>x.Value);
            EventReporter.Instance.LogTS(state.name, dict);
            // manager.hostPC?.SendStatusMsgTS(state, extraData);
        }

        // Wrapper/Replacement Functions
        public static bool IsNumericKeyCode(KeyCode keyCode) {
            bool isAlphaNum = keyCode >= KeyCode.Alpha0 && keyCode <= KeyCode.Alpha9;
            bool isKeypadNum = keyCode >= KeyCode.Keypad0 && keyCode <= KeyCode.Keypad9;
            return isAlphaNum || isKeypadNum;
        }
        public static async Awaitable RepeatUntilYes(Func<CancellationToken, Task> preFunc, string description, LangString displayText, CancellationToken ct, Func<bool, CancellationToken, Task> postFunc = null, bool unpausable = false) {
            var repeat = true;
            while (repeat && !ct.IsCancellationRequested) {
                await preFunc(ct);
                ct.ThrowIfCancellationRequested();

                await TextDisplayer.Instance.DisplayForTask(description, LangStrings.Blank(), displayText, null, ct, async (CancellationToken ct) => {
                    var keyCode = await InputManager.Instance.WaitForKey(new List<KeyCode>() { KeyCode.Y, KeyCode.N }, unpausable: unpausable, ct: ct);
                    repeat = keyCode != KeyCode.Y;
                });
                ct.ThrowIfCancellationRequested();

                if (postFunc != null) { await postFunc(repeat, ct); }
            }
        }
        public static async Awaitable RepeatUntilNo(Func<CancellationToken, Task> preFunc, string description, LangString displayText, CancellationToken ct, Func<bool, CancellationToken, Task> postFunc = null, bool unpausable = false) {
            var repeat = true;
            while (repeat && !ct.IsCancellationRequested) {
                await preFunc(ct);
                ct.ThrowIfCancellationRequested();

                await TextDisplayer.Instance.DisplayForTask(description, LangStrings.Blank(), displayText, null, ct, async (CancellationToken ct) => {
                    var keyCode = await InputManager.Instance.WaitForKey(new List<KeyCode>() { KeyCode.Y, KeyCode.N }, unpausable: unpausable, ct: ct);
                    repeat = keyCode != KeyCode.N;
                });
                ct.ThrowIfCancellationRequested();

                if (postFunc != null) { await postFunc(repeat, ct); }
            }
        }
        public static async Awaitable RepeatForever(Func<CancellationToken, Task> preFunc, string description, LangString displayText, List<KeyCode> keyCodes, CancellationToken ct, Func<CancellationToken, Task> postFunc = null, bool unpausable = false) {
            while (!ct.IsCancellationRequested) {
                await preFunc(ct);
                ct.ThrowIfCancellationRequested();

                await TextDisplayer.Instance.DisplayForTask(description, LangStrings.Blank(), displayText, null, ct, async (CancellationToken ct) => {
                    var keyCode = await InputManager.Instance.WaitForKey(keyCodes, unpausable: unpausable, ct: ct);
                });
                ct.ThrowIfCancellationRequested();

                if (postFunc != null) { await postFunc(ct); }
            }
        }
    }
}