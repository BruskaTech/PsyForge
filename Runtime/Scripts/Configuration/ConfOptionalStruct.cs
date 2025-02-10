//Copyright (c) 2025 University of Bonn (James Bruska)
//Copyright (c) 2025 Bruska Technologies LLC (James Bruska)
//This file is part of PsyForge.
//PsyForge is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//PsyForge is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with PsyForge. If not, see <https://www.gnu.org/licenses/>.

#nullable enable

using System;

namespace PsyForge {
    public struct ConfOptionalStruct<T> where T : struct {
        private T? value;
        private readonly string name;

        public ConfOptionalStruct(T? value, string name) {
            this.value = value;
            this.name = name;
        }

        public T? Val { 
            get => value;
            set => this.value = value;
        }

        public T Value {
            get {
                if (!value.HasValue) {
                    string expConfigNotLoaded = Config.IsExperimentConfigSetup() ? "" : "\nNote: Experiment config not loaded yet.";
                    throw new Exception("You are trying to access an optional config that has not been set yet. You may be missing the Config setting: " + name + "." + expConfigNotLoaded);
                }
                return value.Value;
            }
            set {
                this.value = value;
            }
        }

        public readonly bool HasValue => value.HasValue;

        public readonly string Name => name;

        public static implicit operator T?(ConfOptionalStruct<T> conf) => conf.Val;

        
        public override string? ToString() {
            return Val?.ToString();
        }
    }
}