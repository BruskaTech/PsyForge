//Copyright (c) 2025 University of Bonn (James Bruska)
//Copyright (c) 2024 Columbia University (James Bruska)
//Copyright (c) 2024 Jefferson University (James Bruska)
//Copyright (c) 2024 Bruska Technologies LLC (James Bruska)
//Copyright (c) 2023 University of Pennsylvania (James Bruska)
//This file is part of PsyForge.
//PsyForge is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//PsyForge is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with PsyForge. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;
using PsyForge.DataManagement;
using PsyForge.Extensions;
using Unity.Properties;
using UnityEngine;

#if UNITY_WEBGL // System.IO
using System.Net;
using UnityEngine.Networking;
using UnityEngine;
#endif

namespace PsyForge {
    public partial class Config {
        // Private Variables
        private const string SYSTEM_CONFIG_NAME = "config.json";
        private static string systemConfigText = null;
        private static string experimentConfigText = null;
        private static List<string> unsetRequiredProperties = GetFields()
            .Where(x => !x.IsNullable()) // All non-nullable properties are required
            .Select(x => x.Name)
            .ToList();

        // Internal Variables
        internal static string experimentConfigName { get; private set; } = null;

        // Private Methods
        private static FieldInfo[] GetFields() {
            return typeof(Config).GetFields(BindingFlags.Static | BindingFlags.Public);
        }
        private static JObject Serialize() {
            var fields = GetFields();
            var json = new JObject();
            foreach (var f in fields) {
                var staticObj = f.GetValue(null);
                var pType = f.FieldType.GetGenericArguments()[0];
                var confType = f.FieldType.GetGenericTypeDefinition();
                var closedConfType = confType.MakeGenericType(pType);
                try {
                    var val = closedConfType.GetProperty("Val").GetValue(staticObj);
                    if (val == null) { continue; }
                    json[f.Name] = JToken.FromObject(val);
                } catch (Exception) {} // Do Nothing
            }
                
            return json;
        }
        private static void DeserializeIntoStatic(JObject json) {
            if (json == null) return;
            var fields = GetFields();
            foreach (var f in fields) {
                var confType = f.FieldType.GetGenericTypeDefinition();
                var innerType = f.FieldType.GetGenericArguments()[0];
                var closedConfType = confType.MakeGenericType(innerType);

                if (json.ContainsKey(f.Name)) {
                    // UnityEngine.Debug.Log($"Config: " + f.Name + " " + f.GetValue(null));
                    var newVal = json[f.Name].ToObject(innerType);
                    var objNew = f.GetValue(null);
                    closedConfType.GetProperty("Val").SetValue(objNew, newVal);
                    f.SetValue(null, objNew);

                    // unsetRequiredProperties.Remove(f.Name);
                }
            }
        }

        // Public Methods

        public static bool IsSystemConfigSetup() {
            return systemConfigText != null;
        }
        public static bool IsExperimentConfigSetup() {
            return experimentConfigText != null;
        }
        public static JObject ToJson() {
            return Serialize();
        }
        public static Dictionary<string, object> ToDict() {
            return Serialize().ToObject<Dictionary<string, object>>();
        }
        public static void SaveConfigs(string path) {
            if (systemConfigText != null) {
                var json = JObject.Parse(experimentConfigText).ToObject<Dictionary<string, object>>();
                EventReporter.Instance.LogTS("systemConfig", new(json));
#if !UNITY_WEBGL // System.IO
                File.WriteAllText(Path.Combine(path, SYSTEM_CONFIG_NAME), systemConfigText);
#endif // !UNITY_WEBGL
            }

            if (experimentConfigText != null) {
                var json = JObject.Parse(experimentConfigText).ToObject<Dictionary<string, object>>();
                EventReporter.Instance.LogTS("experimentConfig", json);
#if !UNITY_WEBGL // System.IO
                File.WriteAllText(Path.Combine(path, experimentConfigName + ".json"), experimentConfigText);
#endif // !UNITY_WEBGL
            }

            EventReporter.Instance.LogTS("combinedConfig", ToDict());
        }

        // Internal Methods

        internal static void Init() {
            var fields = GetFields();
            foreach (var f in fields) {
                var confType = f.FieldType.GetGenericTypeDefinition();
                if (!f.FieldType.IsGenericType ||
                    (confType != typeof(Conf<>) && confType != typeof(ConfOptionalStruct<>) && confType != typeof(ConfOptionalClass<>)))
                {
                    throw new InvalidOperationException($"The Config variable \"{f.Name}\" is not of type Conf, OptionalStructConf, or OptionalClassConf. It is of type {f.FieldType}."
                        + $"Please change it's type to something like this: Conf<{f.FieldType}>");
                }

                var innerType = f.FieldType.GetGenericArguments()[0];
                var closedConfType = confType.MakeGenericType(innerType);
                object defaultValue;
                if (innerType.IsValueType) {
                    if (confType == typeof(ConfOptionalStruct<>)) {
                        defaultValue = null;
                    } else {
                        defaultValue = Activator.CreateInstance(innerType);
                    }
                } else {
                    defaultValue = null;
                }
                var confInstance = Activator.CreateInstance(
                    closedConfType,
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                    null,
                    new object[] { defaultValue, f.Name },
                    CultureInfo.InvariantCulture);
                f.SetValue(null, confInstance);
            }
        }
        internal static async Awaitable SetupSystemConfig() {
            systemConfigText = await SetupConfig(SYSTEM_CONFIG_NAME);
        }
        internal static async Awaitable SetupExperimentConfig(string configName) {
            experimentConfigName = configName;
            experimentConfigText = await SetupConfig(experimentConfigName + ".json");

            // TODO: JPB: (feature) Figure out how to allow for unset required properties PER EXPERIMENT
            //            This can be done by marking required properties with a custom attribute [Required] or [Required("ExpName")]
            //            Then I can check for that with reflection or System.ComponentModel.DataAnnotations
            // if (unsetRequiredProperties.Count > 0) {
            //     throw new Exception($"The following required properties were not set: {string.Join(", ", unsetRequiredProperties)}");
            // }
        }

        internal static string[] GetExperimentConfigs() {
            // Get all configuration files
            string[] configs;
#if !UNITY_WEBGL // System.IO
            configs = Directory.GetFiles(FileManager.ConfigPath(), "*.json");
#else // UNITY_WEBGL
            TextAsset[] jsonFiles = Resources.LoadAll<TextAsset>(FileManager.ConfigPath());
            if (jsonFiles == null || jsonFiles.Length == 0) {
                throw new Exception("Configuration File Error. No configuration files found in configs folder."
                    + "\nSince you are using WebGL, you must put all configuration files in the 'Resources/configs' folder of the Unity project itself.");
            }
            configs = jsonFiles.Select(x => x.name).ToArray();
#endif // UNITY_WEBGL
            if (configs == null || configs.Length < 2) {
                throw new Exception("Configuration File Error. Missing system or experiment configuration file in configs folder");
            }
            return configs.Where(x => x != SYSTEM_CONFIG_NAME && x != Path.GetFileNameWithoutExtension(SYSTEM_CONFIG_NAME)).ToArray();
        }

        private static async Task<string> SetupConfig(string config) {
            await Task.CompletedTask;
            var configDir = FileManager.ConfigPath();
#if !UNITY_WEBGL
            var configPath = Path.Combine(configDir, config);
            if (!Directory.Exists(configDir)) {
                throw new IOException($"Config directory path does not exist: {configDir}");
            }
            string text = File.ReadAllText(configPath);
#else // UNITY_WEBGL
            var configPath = Path.Combine(configDir, Path.GetFileNameWithoutExtension(config));
            var textAsset = Resources.Load<TextAsset>(configPath);
            if (textAsset == null) {
                throw new Exception($"Configuration File Error. The file {config} was not found in the configs folder."
                    + "\nSince you are using WebGL, you must put all configuration files in the 'Resources/configs' folder of the Unity project itself.");
            }
            string text = textAsset.text;
#endif
// #if !UNITY_WEBGL // System.IO
//             string text = File.ReadAllText(configPath);
// #else // UNITY_WEBGL
//             var webReq = UnityWebRequest.Get(configPath);
//             await webReq.SendWebRequest();

//             if (webReq.result == UnityWebRequest.Result.ConnectionError) {
//                 throw new WebException($"Failed to fetch {Path.GetFileNameWithoutExtension(configPath)} due to connection error\n({configPath})\n\n({webReq.error})");
//             } else if (webReq.result == UnityWebRequest.Result.ProtocolError) {
//                 throw new WebException($"Failed to fetch {Path.GetFileNameWithoutExtension(configPath)} due to protocol error\n({configPath})\n\n({webReq.error})");
//             } else if (webReq.result == UnityWebRequest.Result.DataProcessingError) {
//                 throw new WebException($"Failed to fetch {Path.GetFileNameWithoutExtension(configPath)} due to data processing error\n({configPath})\n\n({webReq.error})");
//             }
            
//             string text = webReq.downloadHandler.text;
// #endif // UNITY_WEBGL

            var json = JObject.Parse(text);
            DeserializeIntoStatic(json);
            return text;
        }
    }
}