//Copyright (c) 2024 Jefferson University (James Bruska)
//Copyright (c) 2024 Bruska Technologies LLC (James Bruska)
//Copyright (c) 2023 University of Pennsylvania (James Bruska)

//This file is part of PsyForge.
//PsyForge is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//PsyForge is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with PsyForge. If not, see <https://www.gnu.org/licenses/>. 

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

using PsyForge.Threading;
using System.Linq;
using PsyForge.Utilities;

namespace PsyForge {

    // TODO: JPB: (refactor) Use new unity input system for key input
    //            Keyboard.current.anyKey.wasPressedThisFrame
    [DefaultExecutionOrder(-99)] // This is because UnityEngine.InputSystem.InputSystem.onEvent is -100
    public class InputManager : SingletonEventMonoBehaviour<InputManager> {
        protected override void AwakeOverride() { }

        public bool GetKeyDown(KeyCode key, bool unpausable = false) {
            return DoGet<KeyCode, Bool, Bool>(GetKeyDownHelper, key, unpausable);
        }
        protected Bool GetKeyDownHelper(KeyCode key, Bool unpausable) {
            if (!unpausable && Time.timeScale == 0) { return false; }
            var newKey = KeyCodeConversions.KeyCodeToKey(GetLocalizedKey(key));
            return Keyboard.current[newKey].wasPressedThisFrame;
        }
        public KeyCode GetKeyDown(KeyCode[] keys, bool unpausable = false) {
            return DoGet<KeyCode[], Bool, KeyCode>(GetKeyDownHelper, keys, unpausable);
        }
        public KeyCode GetKeyDownHelper(KeyCode[] keys, Bool unpausable) {
            if (!unpausable && Time.timeScale == 0) { return KeyCode.None; }

            foreach (KeyCode key in keys) {
                var newKey = KeyCodeConversions.KeyCodeToKey(GetLocalizedKey(key));
                if (Keyboard.current[newKey].wasPressedThisFrame) {
                    return GetLocalizedKey(key);
                }
            }
            return KeyCode.None;
        }

        public bool GetKey(KeyCode key, bool unpausable = false) {
            return DoGet<KeyCode, Bool, Bool>(GetKeyHelper, key, unpausable);
        }
        protected Bool GetKeyHelper(KeyCode key, Bool unpausable) {
            if (!unpausable && Time.timeScale == 0) { return false; }
            var newKey = KeyCodeConversions.KeyCodeToKey(GetLocalizedKey(key));
            return Keyboard.current[newKey].isPressed;
        }
        public KeyCode GetKey(List<KeyCode> keys, bool unpausable = false) {
            return DoGet<KeyCode[], Bool, KeyCode>(GetKeyHelper, keys.ToArray(), unpausable);
        }
        public KeyCode GetKey(KeyCode[] keys, bool unpausable = false) {
            return DoGet<KeyCode[], Bool, KeyCode>(GetKeyHelper, keys, unpausable);
        }
        public KeyCode GetKeyHelper(KeyCode[] keys, Bool unpausable) {
            if (!unpausable && Time.timeScale == 0) { return KeyCode.None; }

            foreach (KeyCode key in keys) {
                var newKey = KeyCodeConversions.KeyCodeToKey(GetLocalizedKey(key));
                if (Keyboard.current[newKey].isPressed) {
                    return GetLocalizedKey(key);
                }
            }
            return KeyCode.None;
        }

        public async Task<KeyCode> WaitForKey(bool unpausable = false, CancellationToken ct = default) {
            return await DoGet<Bool, CancellationToken, KeyCode>(WaitForKeyHelper, unpausable, ct);
        }
        protected async Task<KeyCode> WaitForKeyHelper(Bool unpausable, CancellationToken ct) {
            // This first await is needed when WaitForKey is used in a tight loop.
            // If it is, then it will repeatedly be checked over and over on the same frame, causing the program to hang
            // It does add a one frame delay, but if you are using an await in the first place, you are probably not concerned about that
            while (!ct.IsCancellationRequested) {
                await Awaitable.NextFrameAsync();
                if (unpausable || Time.timeScale != 0) {
                    foreach (KeyCode vKey in Enum.GetValues(typeof(KeyCode))) {
                        var newKey = KeyCodeConversions.KeyCodeToKey(GetLocalizedKey(vKey));
                        if (Keyboard.current[newKey].wasPressedThisFrame) {
                            return GetLocalizedKey(vKey);
                        };
                    }
                }
            }
            return KeyCode.None;
        }
        public async Task WaitForKey(KeyCode key, bool unpausable = false, CancellationToken ct = default) {
            await DoWaitFor<KeyCode, Bool, CancellationToken>(WaitForKeyHelper, key, unpausable, ct);
        }
        protected async Task WaitForKeyHelper(KeyCode key, Bool unpausable, CancellationToken ct) {
            // This first await is needed when WaitForKey is used in a tight loop.
            // If it is, then it will repeatedly be checked over and over on the same frame, causing the program to hang
            // It does add a one frame delay, but if you are using an await in the first place, you are probably not concerned about that
            await Awaitable.NextFrameAsync();
            while (!GetKeyDownHelper(key, unpausable)) {
                await Awaitable.NextFrameAsync(ct);
            }
        }
        // TODO: JPB: (refactor) Make WaitForKey a static method
        public async Task<KeyCode> WaitForKey(List<KeyCode> keys, bool unpausable = false, CancellationToken ct = default) {
            return await WaitForKey(keys.ToArray(), unpausable, ct);
        }
        public async Task<KeyCode> WaitForKey(KeyCode[] keys, bool unpausable = false, CancellationToken ct = default) {
            return await DoGet<KeyCode[], Bool, CancellationToken, KeyCode>(WaitForKeyHelper, keys, unpausable, ct);
        }
        protected async Task<KeyCode> WaitForKeyHelper(KeyCode[] keys, Bool unpausable, CancellationToken ct) {
            // This first await is needed when WaitForKey is used in a tight loop.
            // If it is, then it will repeatedly be checked over and over on the same frame, causing the program to hang
            // It does add a one frame delay, but if you are using an await in the first place, you are probably not concerned about that
            var retKey = KeyCode.None;
            while (!ct.IsCancellationRequested && retKey == KeyCode.None) {
                await Awaitable.NextFrameAsync(ct);
                retKey = GetKeyDownHelper(keys, unpausable);
            }
            return GetLocalizedKey(retKey);
        }

        // TODO: JPB: (needed) This is a BIG stopgap for the new input system
        public static KeyCode GetLocalizedKey(KeyCode keyCode) {
            if (Config.keyboardLanguage.Val?.ToLower() == "german") {
                if (keyCode == KeyCode.Y) { return KeyCode.Z; }
                if (keyCode == KeyCode.Z) { return KeyCode.Y; }
            }
            return keyCode;
        }
        public static List<KeyCode> GetLocalizedKey(List<KeyCode> keyCodes) {
            return keyCodes.Select(k => GetLocalizedKey(k)).ToList();
        }
    }

}