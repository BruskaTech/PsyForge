//Copyright (c) 2024 Jefferson University (James Bruska)
//Copyright (c) 2024 Bruska Technologies LLC (James Bruska)

//This file is part of PsyForge.
//PsyForge is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//PsyForge is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with PsyForge. If not, see <https://www.gnu.org/licenses/>. 

#nullable enable
using PsyForge.Threading;
using System;

/// <summary>
/// This is a struct that is used to represent a nullable value type.
/// The big difference between this and the built-in nullable types is that this one is blittable.
/// This also means that it needs to be disposed of if the value is a disposable type.
/// </summary>
/// <typeparam name="T"></typeparam>
public struct NativeNullable<T> : IDisposable where T : struct {
        readonly T value;
        readonly Bool isNull;
        public NativeNullable(T? value) {
            if (value.HasValue) {
                this.value = value.Value;
                this.isNull = false;
            } else {
                this.value = default;
                this.isNull = true;
            }
        }

    /// <summary>
    /// This disposes of the internal value if it is a disposable type.
    /// </summary>
    public void Dispose() {
        if (value is IDisposable disposableValue) {
            disposableValue.Dispose();
        }
    }

    /// <value>
    /// Whether or not the option has a value.
    /// </value>
    public bool HasValue {
        get {
            return !isNull;
        }
    }
    
    /// <value>
    /// The value of the option.
    /// </value>
    /// <exception cref="InvalidOperationException">If the option does not have a value.</exception>
    public T Value {
        get {
            if (isNull) {
                throw new InvalidOperationException("Option does not have a value.");
            }
            return value;
        }
    }
}
#nullable disable