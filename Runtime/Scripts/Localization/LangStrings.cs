//Copyright (c) 2024 Jefferson University (James Bruska)
//Copyright (c) 2024 Bruska Technologies LLC (James Bruska)
//Copyright (c) 2023 University of Pennsylvania (James Bruska)

//This file is part of PsyForge.
//PsyForge is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//PsyForge is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with PsyForge. If not, see <https://www.gnu.org/licenses/>. 

using System;
using System.Collections.Generic;

using PsyForge.Extensions;
using System.Reflection;
using System.Linq;

namespace PsyForge.Localization {
    
    // Maybe change name to LangStrs or LangCtrl and chage LangString to LangStr
    public static partial class LangStrings {
        public static Language Language {get; private set;} = Language.English;

        /// <summary>
        /// Set the current language for the LangStrings
        /// </summary>
        /// <param name="lang">The language to set</param>
        public static void SetLanguage(Language lang) {
            Language = lang;
            CheckLangStrings();
        }

        /// <summary>
        /// Set the current language for the LangStrings from the config
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public static void SetLanguage() {
            try {
                SetLanguage((Language)Enum.Parse(typeof(Language), Config.language));
            } catch (ArgumentException e) {
                throw new ArgumentException($"The language \"{Config.language}\" in the config is not a valid language."
                    + $"\n\nPlease use one of the following supported lanuages: {string.Join(", ", Enum.GetNames(typeof(Language)))}", e);
            }
        }

        /// <summary>
        /// Generate a LangString for all languages
        /// ONLY USE THIS FUNCTION IF YOU ARE SURE THE STRING IS THE SAME FOR ALL LANGUAGES
        /// Examples of this would be: numbers, file paths, subject ids, rich text formatting, etc.
        /// </summary>
        /// <param name="val">The string value for all languages</param>
        /// <returns>A LangString with the value for all languages</returns>
        public static LangString GenForAllLangs(string val) {
            Dictionary<Language, string> strings = new();
            foreach (Language lang in Enum.GetValues(typeof(Language))) {
                strings.Add(lang, val);
            }
            return new(strings);
        }
        
        /// <summary>
        /// Generate a LangString for the current language
        /// ONLY USE THIS FUNCTION IF YOU ARE SURE THE STRING IS THE SAME FOR ALL LANGUAGES
        /// Examples of this would be: numbers, file paths, subject ids, rich text formatting, etc.
        /// </summary>
        /// <param name="val">The string value for the current language</param>
        /// <returns>A LangString with the value for the current language</returns>
        public static LangString GenForCurrLang(string val) {
            return new(new() { { Language, val } });
        }

        /// <summary>
        /// Calls every LangString for the current language to make sure that there isn't a missing value
        /// </summary>
        // TODO: JPB: (feature) Add tag to any LangString that is only used in the Startup scene or manager scene
        //            In this case, only check them if you are in the startup or manager scene.
        //            Otherwise, you can ignore them and not throw an error
        // TODO: JPB: (feature) Add language feature to select which languages are allowed to be used
        //            This can be done by checking which Language options are used in the LangString(s) that do NOT have the Startup tag
        //            Config will need to split Language into StartupLanguage and ExperimentLanguage
        public static void CheckLangStrings() {
            // Get the type representing MyClass.
            Type type = typeof(LangStrings);

            // Retrieve all public static methods returning LangString
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static)
                            .Where(m => m.ReturnType == typeof(LangString)
                                     && m.Name != nameof(GenForAllLangs)
                                     && m.Name != nameof(GenForCurrLang));

            foreach (var method in methods) {
                // Prepare default values for the method parameters.
                ParameterInfo[] parameters = method.GetParameters();
                object[] defaultValues = parameters.Select(p =>
                    p.ParameterType.IsValueType ? Activator.CreateInstance(p.ParameterType) : null
                ).ToArray();

                // Invoke the static method with the default parameters.
                var result = (LangString)method.Invoke(null, defaultValues);
                UnityEngine.Debug.Log(result);
                if (result.ToString() == null) {
                    throw new Exception($"The LangString \"{method.Name}\"  has not been set for the current language ({Language})");
                }
            }
        }
    }

}
