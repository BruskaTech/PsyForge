//Copyright (c) 2025 Bruska Technologies LLC (James Bruska)

//This file is part of OpenField.
//PsyForge is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//PsyForge is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with PsyForge. If not, see <https://www.gnu.org/licenses/>. 

using UnityEngine;

namespace PsyForge.Localization {

    public static partial class LangStrings {
        public static LangString SessionStart() { return new( new() {
            { Language.English, "Welcome!" },
            { Language.German, "Willkommen!" },
        }); }
        public static LangString Press1or2() { return new( new() {
            { Language.English, "Press 1 or 2 to continue." },
            { Language.German, "Drücken Sie 1 oder 2, um fortzufahren." },
        }); }
        public static LangString YouPressed(KeyCode key) { return new( new() {
            { Language.English, $"You pressed: {key}" },
            { Language.German, $"Sie haben gedrückt: {key}" },
        }); }
    }

}