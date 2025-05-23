//Copyright (c) 2025 University of Bonn (James Bruska)
//Copyright (c) 2024 Jefferson University (James Bruska)
//Copyright (c) 2024 Bruska Technologies LLC (James Bruska)
//Copyright (c) 2023 University of Pennsylvania (James Bruska)

//This file is part of PsyForge.
//PsyForge is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//PsyForge is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with PsyForge. If not, see <https://www.gnu.org/licenses/>. 

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;
using Newtonsoft.Json;
using TMPro;

using PsyForge.DataManagement;
using PsyForge.Utilities;
using System.Collections.ObjectModel;

namespace PsyForge.Extensions {
    public static class IEnumerableExtensions {
        // https://stackoverflow.com/a/30758270
        public static int GetSequenceHashCode<T>(this IEnumerable<T> sequence) {
            const int seed = 487;
            const int modifier = 31;

            unchecked {
                return sequence.Aggregate(seed, (current, item) =>
                    (current*modifier) + item.GetHashCode());
            }            
        }
    }
    
    public static class IListExtensions {
        /// <summary>
        /// Knuth (Fisher-Yates) Shuffle
        /// Returns a shuffled copy of the IList.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        public static IList<T> Shuffle<T>(this IList<T> list, System.Random rnd) {
            var shuf = new List<T>(list);
            for (int i = shuf.Count - 1; i > 0; i--) {
                int j = rnd.Next(i + 1);
                T tmp = shuf[i];
                shuf[i] = shuf[j];
                shuf[j] = tmp;
            }

            return shuf;
        }
        public static IList<T> Shuffle<T>(this IList<T> list) {
            return list.Shuffle(Utilities.Random.Rnd);
        }

        /// <summary>
        /// Knuth (Fisher-Yates) Shuffle
        /// Returns a shuffled copy of the List.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        public static List<T> Shuffle<T>(this List<T> list, System.Random rnd) {
            var shuf = new List<T>(list);
            for (int i = shuf.Count - 1; i > 0; i--) {
                int j = rnd.Next(i + 1);
                T tmp = shuf[i];
                shuf[i] = shuf[j];
                shuf[j] = tmp;
            }

            return shuf;
        }
        public static List<T> Shuffle<T>(this List<T> list) {
            return list.Shuffle(Utilities.Random.Rnd);
        }

        /// <summary>
        /// Knuth (Fisher-Yates) Shuffle
        /// Shuffles the element order of the specified list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static IList<T> ShuffleInPlace<T>(this IList<T> list, System.Random rnd) {
            var count = list.Count;
            for (int i = 0; i < count; ++i) {
                int r = rnd.Next(i, count);
                T tmp = list[i];
                list[i] = list[r];
                list[r] = tmp;
            }
            return list;
        }
        public static IList<T> ShuffleInPlace<T>(this IList<T> list) {
            return list.ShuffleInPlace(Utilities.Random.Rnd);
        }

        /// <summary>
        /// Knuth (Fisher-Yates) Shuffle
        /// Shuffles the element order of the specified list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static List<T> ShuffleInPlace<T>(this List<T> list, System.Random rnd) {
            var count = list.Count;
            for (int i = 0; i < count; ++i) {
                int r = rnd.Next(i, count);
                T tmp = list[i];
                list[i] = list[r];
                list[r] = tmp;
            }
            return list;
        }
        public static List<T> ShuffleInPlace<T>(this List<T> list) {
            return list.ShuffleInPlace(Utilities.Random.Rnd);
        }

        // https://stackoverflow.com/a/30758270
        public static int GetSequenceHashCode<T>(this IList<T> sequence) {
            const int seed = 487;
            const int modifier = 31;

            unchecked {
                return sequence.Aggregate(seed, (current, item) =>
                    (current*modifier) + item.GetHashCode());
            }            
        }

        /// <summary>
        /// Calculates the percentile of this list.
        /// </summary>
        /// <param name="sequence"></param>
        /// <param name="percentile">Value must be between 0 and 1 (inclusive)</param>
        /// <returns></returns>
        public static double Percentile(this IList<double> sequence, double percentile) {
            return Statistics.Percentile(sequence, percentile);
        }
        /// <summary>
        /// Calculates the percentile of this list.
        /// </summary>
        /// <param name="sequence"></param>
        /// <param name="percentile">Value must be between 0 and 1 (inclusive)</param>
        /// <returns></returns>
        public static double Percentile(this IList<int> sequence, double percentile) {
            return Statistics.Percentile(sequence, percentile);
        }
    
        /// <summary>
        /// Transpose List of Lists
        /// Ex: [[1,2,3],[4,5,6]] -> [[1,4],[2,5],[3,6]]
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="nestedList"></param>
        /// <returns></returns>
        public static List<List<T>> Transpose<T>(this List<List<T>> nestedList) {
            // Check for issues
            TransposeChecks(nestedList);

            var result = new List<List<T>>();
            for (int col = 0; col < nestedList[0].Count; col++) {
                var columnList = nestedList.Select(row => row[col]).ToList();
                result.Add(columnList);
            }

            return result;
        }

        /// <summary>
        /// Transpose List of Lists, but convert the top level to a Stack
        /// Ex: [[1,2,3],[4,5,6]] -> [[1,4],[2,5],[3,6]]
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="nestedList"></param>
        /// <returns></returns>
        public static Stack<List<T>> TransposeToStack<T>(this List<List<T>> nestedList) {
            // Check for issues
            TransposeChecks(nestedList);

            var result = new Stack<List<T>>();
            for (int col = 0; col < nestedList[0].Count; col++) {
                var columnList = nestedList.Select(row => row[col]).ToList();
                result.Push(columnList);
            }

            return result;
        }
        /// <summary>
        /// Transpose List of Lists, but convert the top level to a Queue
        /// Ex: [[1,2,3],[4,5,6]] -> [[1,4],[2,5],[3,6]]
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="nestedList"></param>
        /// <returns></returns>
        public static Queue<List<T>> TransposeToQueue<T>(this List<List<T>> nestedList) {
            // Check for issues
            TransposeChecks(nestedList);

            var result = new Queue<List<T>>();
            for (int col = 0; col < nestedList[0].Count; col++) {
                var columnList = nestedList.Select(row => row[col]).ToList();
                result.Enqueue(columnList);
            }

            return result;
        }
        
        private static void TransposeChecks<T>(List<List<T>> nestedList) {
            if (nestedList == null || nestedList.Count == 0) {
                throw new ArgumentException("Input list cannot be null or empty");
            } else if (nestedList.Any(row => row == null)) {
                throw new ArgumentException("Input list cannot contain null rows");
            } else if (!nestedList.All(row => row.Count == nestedList[0].Count)) {
                throw new ArgumentException("All rows must have the same length");
            }
        }

        /// <summary>
        /// Transpose List of Lists, but allow for uneven lists
        /// This does not have nulls or defaults for missing values.
        /// Ex: [[1,2,3],[4,5],[6]] -> [[1,4,6],[2,5],[3]]
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="nestedList"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static List<List<T>> TransposeUneven<T>(this List<List<T>> nestedList) {
            // Check for issues
            if (nestedList == null || nestedList.Count == 0) {
                throw new ArgumentException("Input list cannot be null or empty");
            } else if (nestedList.Any(row => row == null)) {
                throw new ArgumentException("Input list cannot contain null rows");
            }

            var result = new List<List<T>>();
            var maxCols = nestedList.Max(row => row.Count);
            for (int col = 0; col < maxCols; col++) {
                var columnList = new List<T>();
                foreach (var row in nestedList) {
                    if (col < row.Count) {
                        columnList.Add(row[col]);
                    }
                }
                result.Add(columnList);
            }

            return result;
        }
    }

    public static class ArrayExtensions {
        /// <summary>
        /// Knuth (Fisher-Yates) Shuffle
        /// Shuffles the element order of the specified array.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static T[] ShuffleInPlace<T>(this T[] array, System.Random rnd) {
            int n = array.Length;
            while (n > 1) {
                _ = rnd.Next(2);
                int k = rnd.Next(n--);
                T temp = array[n];
                array[n] = array[k];
                array[k] = temp;
            }
            return array;
        }
        public static T[] ShuffleInPlace<T>(this T[] array) {
            return array.ShuffleInPlace(Utilities.Random.Rnd);
        }

        // https://stackoverflow.com/a/30758270
        public static int GetSequenceHashCode<T>(this T[] sequence) {
            const int seed = 487;
            const int modifier = 31;

            unchecked {
                return sequence.Aggregate(seed, (current, item) =>
                    (current*modifier) + item.GetHashCode());
            }            
        }
    }

    public static class CollectionExtensions {
        /// <summary>
        /// Allows List constructor to take a items or a list of items that gets expanded
        /// https://stackoverflow.com/a/63374611
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="itemsToAdd"></param>
        public static void Add<T>(this ICollection<T> collection, IEnumerable<T> itemsToAdd) {
            foreach (var item in itemsToAdd) {
                collection.Add(item);
            }
        }
    }

    public static class DictionaryExtensions {
        /// <summary>
        /// Convert a dictionary to a JSON string
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="dict"></param>
        /// <returns>The converted JSON string</returns>
        public static string ToJSON<T,U>(this Dictionary<T,U> dict) {
            return JsonConvert.SerializeObject(dict);
        }
    }

    public static class QueueExtensions {
        public static IEnumerable<T> Dequeue<T>(this Queue<T> queue, int numItems) {
            if (numItems <= 0) {
                throw new ArgumentException($"The number of items to dequeue ({numItems}) must be positive");
            } else if (numItems > queue.Count) {
                throw new ArgumentException($"The number of items to dequeue ({numItems}) must be less than or equal to the number of items in the queue ({queue.Count})");
            }
            for (int i = 0; i < numItems && queue.Count > 0; i++) {
                yield return queue.Dequeue();
            }
        }
    }

    public static class EnumeratorExtensions {
        /// <summary>
        /// Convert an IEnumerator to an IEnumerable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerator"></param>
        /// <returns>The IEnumerable</returns>
        public static IEnumerable<T> ToEnumerable<T>(this IEnumerator<T> enumerator) {
            while (enumerator.MoveNext())
                yield return enumerator.Current;
        }

        /// <summary>
        /// Try catch for exceptions in an IEnumerator
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerator"></param>
        /// <param name="onError"></param>
        /// <returns>The new IEnumerator</returns>
        public static IEnumerator TryCatch<T>(this IEnumerator enumerator, Action<T> onError)
            where T : Exception
        {
            object current;
            while (true) {
                try {
                    if (enumerator.MoveNext() == false) {
                        break;
                    }
                    current = enumerator.Current;
                } catch (T e) {
                    onError(e);
                    yield break;
                }
                yield return current;
            }
        }
    }

    public static class TaskExtensions {
        /// <summary>
        /// Convert awaitable task to an IEnumerator
        /// https://forum.unity.com/threads/async-await-inside-a-coroutine.952110/
        /// </summary>
        /// <param name="task"></param>
        public static IEnumerator ToEnumerator(this Task task) {
            while(!task.IsCompleted) { yield return null; }
            if (task.IsFaulted) { throw new Exception("Task Exception", task.Exception.InnerException); }
            else if (task.IsCanceled) { throw new OperationCanceledException("ToEnumerator"); }
        }

        /// <summary>
        /// Convert awaitable task to an IEnumerator
        /// https://forum.unity.com/threads/async-await-inside-a-coroutine.952110/
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="task"></param>
        public static IEnumerator ToEnumerator<T>(this Task<T> task) {
            while (!task.IsCompleted) { yield return null; }
            if (task.IsFaulted) { throw new Exception("Task Exception", task.Exception.InnerException); }
            else if (task.IsCanceled) { throw new OperationCanceledException("ToEnumerator"); }
        }

        /// <summary>
        /// Create a timeout for a task that throws a TimeoutException if the task takes too long
        /// </summary>
        /// <param name="task"></param>
        /// <param name="timeoutMs"></param>
        /// <param name="timeoutMessage"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public static async Task Timeout(this Task task, int timeoutMs, CancellationTokenSource cts, string timeoutMessage = null, bool pauseAware = false) {
            if (timeoutMs < 0) {
                throw new ArgumentException($"The timeoutMs cannot be negative, but it was {timeoutMs}");
            }
            Task timeoutTask = pauseAware 
                ? MainManager.Instance.DelayTS(timeoutMs)
                : Task.Delay(timeoutMs);
            var completedTask = await Task.WhenAny(task, timeoutTask);
            await completedTask; // Propagates exceptions thrown in the task
            if (completedTask == timeoutTask) {
                cts?.Cancel();
                var msg = timeoutMessage ?? $"Task Timed out after {timeoutMs}ms";
                throw new TimeoutException(msg);
            }
        }

        /// <summary>
        /// Create a timeout for a task that throws a TimeoutException if the task takes too long
        /// </summary>
        /// <typeparam name="Z"></typeparam>
        /// <param name="task"></param>
        /// <param name="timeoutMs"></param>
        /// <param name="timeoutMessage"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public static async Task<Z> Timeout<Z>(this Task<Z> task, int timeoutMs, CancellationTokenSource cts, string timeoutMessage = null, bool pauseAware = false) {
            if (timeoutMs < 0) {
                throw new ArgumentException($"The timeoutMs cannot be negative, but it was {timeoutMs}");
            }
            Task timeoutTask = pauseAware 
                ? MainManager.Instance.DelayTS(timeoutMs)
                : Task.Delay(timeoutMs);
            var completedTask = await Task.WhenAny(task, timeoutTask);
            await completedTask; // Propagates exceptions thrown in the task
            if (completedTask == timeoutTask) {
                cts?.Cancel();
                var msg = timeoutMessage ?? $"Task Timed out after {timeoutMs}ms";
                throw new TimeoutException(msg);
            }
            return await task;
        }

        /// <summary>
        /// Create a timeout for a task and logs if the task taskes too long
        /// Unlike the normal Timeout, this one does not throw an exception if the task times out
        /// </summary>
        /// <param name="task"></param>
        /// <param name="timeoutMs"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static async Task TimeoutQuiet(this Task task, int timeoutMs, CancellationTokenSource cts, bool pauseAware = false) {
            if (timeoutMs < 0) {
                throw new ArgumentException($"The timeoutMs cannot be negative, but it was {timeoutMs}");
            }
            Task timeoutTask = pauseAware 
                ? MainManager.Instance.DelayTS(timeoutMs)
                : Task.Delay(timeoutMs);
            var completedTask = await Task.WhenAny(task, timeoutTask);
            await completedTask; // Propagates exceptions thrown in the task
            if (completedTask == timeoutTask) {
                cts?.Cancel();
                EventReporter.Instance.LogTS("TimeoutQuiet", new() {
                    { "timeoutMs", timeoutMs }
                });
            }
        }
    }

    public static class AwaitableExtensions {
        public static async Task AsTask(this Awaitable a) {
            await a;
        }

        public static async Task<T> AsTask<T>(this Awaitable<T> a) {
            return await a;
        }
    }

    public static class MonoBehaviourExtensions {
        /// <summary>
        /// Quits the game
        /// </summary>
        /// <param name="monoBehaviour"></param>
        public static void Quit(this MonoBehaviour monoBehaviour) {
            UnityEngine.Debug.Log("Quitting");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }

    public static class AudioClipExtensions {
        /// <summary>
        /// Clone an AudioClip
        /// Allows you to optionally provide a new name for the AudioClip
        /// </summary>
        /// <param name="audioClip"></param>
        /// <param name="newName"></param>
        /// <returns></returns>
        public static AudioClip Clone(this AudioClip audioClip, string newName = null) {
            AudioClip newAudioClip = AudioClip.Create(newName ?? audioClip.name, audioClip.samples, audioClip.channels, audioClip.frequency, false);
            float[] copyData = new float[audioClip.samples * audioClip.channels];
            audioClip.GetData(copyData, 0);
            newAudioClip.SetData(copyData, 0);
            return newAudioClip;
        }
    }

    public static class RandomExtensions {
        public static double NextDouble(this System.Random random, double max) {
            return random.NextDouble() * max;
        }
        public static double NextDouble(this System.Random random, double min, double max) {
            return random.NextDouble() * (max - min) + min;
        }
        public static float NextFloat(this System.Random random) {
            return (float) random.NextDouble();
        }
        public static float NextFloat(this System.Random random, float max) {
            return random.NextFloat() * max;
        }
        public static float NextFloat(this System.Random random, float min, float max) {
            return random.NextFloat() * (max - min) + min;
        }
    }

    /// <summary>
    /// https://stackoverflow.com/a/63685720
    /// This can be replaced with ExceptionDispatchInfo.SetRemoteStackTrace in the future,
    /// </summary>
    public static class ExceptionExtensions {
        public static Exception SetStackTrace(this Exception target, StackTrace stack) => _SetStackTrace(target, stack);

        private static readonly Func<Exception, StackTrace, Exception> _SetStackTrace = new Func<Func<Exception, StackTrace, Exception>>(() => {
            ParameterExpression target = Expression.Parameter(typeof(Exception));
            ParameterExpression stack = Expression.Parameter(typeof(StackTrace));
            Type traceFormatType = typeof(StackTrace).GetNestedType("TraceFormat", BindingFlags.NonPublic);
            MethodInfo toString = typeof(StackTrace).GetMethod("ToString", BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { traceFormatType }, null);
            object normalTraceFormat = Enum.GetValues(traceFormatType).GetValue(0);
            MethodCallExpression stackTraceString = Expression.Call(stack, toString, Expression.Constant(normalTraceFormat, traceFormatType));
            FieldInfo stackTraceStringField = typeof(Exception).GetField("_stackTraceString", BindingFlags.NonPublic | BindingFlags.Instance);
            BinaryExpression assign = Expression.Assign(Expression.Field(target, stackTraceStringField), stackTraceString);
            return Expression.Lambda<Func<Exception, StackTrace, Exception>>(Expression.Block(assign, target), target, stack).Compile();
        })();
    }

    public static class TextMeshProUGUIExtensions {
        public static void Bold(this TextMeshProUGUI textComponent, bool boldOn) {
            if (boldOn) {
                textComponent.fontStyle |= FontStyles.Bold;
            } else {
                textComponent.fontStyle &= ~FontStyles.Bold;
            }
        }

        public static float FindMaxFittingFontSize(this TextMeshProUGUI textComponent, List<string> strings) {
            string oldText = textComponent.text;
            bool oldAutosizing = textComponent.enableAutoSizing;
            textComponent.enableAutoSizing = true;
            float maxFontSize = 300;
            foreach (var str in strings) {
                textComponent.text = str;
                textComponent.ForceMeshUpdate();
                if (textComponent.fontSize < maxFontSize) {
                    maxFontSize = textComponent.fontSize;
                }
            }
            textComponent.enableAutoSizing = oldAutosizing;
            textComponent.text = oldText;
            textComponent.ForceMeshUpdate();

            return maxFontSize;
        }
    }

    public static class DateTimeExtensions {
        public static double ConvertToMillisecondsSinceEpoch(this DateTime dateTime, bool convertToUTC = true) {
            var newDateTime = convertToUTC ? dateTime.ToUniversalTime() : dateTime;
            return newDateTime
                .Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc))
                .TotalMilliseconds;
        }
    }

    public static class GameObjectExtensions {

        /// <summary>
        /// Add a component to a GameObject by name as a string
        /// All strings must be the full namespace of the component (ex: "UnityEngine.UI.Image")
        /// If the component is in another assembly (such as PsyForge), you must include that as well (ex: "PsyForge.ExternalDevices.PhotoDiodeSyncbox, PsyForge")
        /// Sometimes you also have to provide the assembly name for your Unity game (ex: "MySyncBox, Assembly-CSharp")
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="componentName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static Component AddComponentByName(this GameObject gameObject, string componentName) {
            if (string.IsNullOrEmpty(componentName)) { throw new ArgumentException("The component name cannot be null or empty"); }

            Type type = Type.GetType(componentName);

            if (type == null) { throw new ArgumentException($"Could not find class {componentName}"); }
            if (!type.IsSubclassOf(typeof(MonoBehaviour))) { throw new ArgumentException($"The class {componentName} is not a MonoBehaviour"); }

            return gameObject.AddComponent(type);
        } 
    }

    public static class StringExtensions {
        /// <summary>
        /// Trim the start of a string with a specific string for all occurrences
        /// Based on: https://stackoverflow.com/a/4335913
        /// </summary>
        /// <param name="target"></param>
        /// <param name="trimString"></param>
        /// <returns></returns>
        public static string TrimStart(this string target, string trimString) {
            if (string.IsNullOrEmpty(trimString)) return target;

            string result = target;
            while (result.StartsWith(trimString)) {
                result = result.Substring(trimString.Length);
            }

            return result;
        }
        /// <summary>
        /// Trim the end of a string with a specific string for all occurrences
        /// Based on: https://stackoverflow.com/a/4335913
        /// </summary>
        /// <param name="target"></param>
        /// <param name="trimString"></param>
        /// <returns></returns>
        public static string TrimEnd(this string target, string trimString) {
            if (string.IsNullOrEmpty(trimString)) return target;

            string result = target;
            while (result.EndsWith(trimString)) {
                result = result.Substring(0, result.Length - trimString.Length);
            }

            return result;
        }
        /// <summary>
        /// Trim the start of a string with a specific string for the first occurrence
        /// Based on: https://stackoverflow.com/a/4335913
        /// </summary>
        /// <param name="target"></param>
        /// <param name="trimString"></param>
        /// <returns></returns>
        public static string TrimStartOnce(this string target, string trimString) {
            if (string.IsNullOrEmpty(trimString)) return target;

            string result = target;
            if (result.StartsWith(trimString)) {
                result = result.Substring(trimString.Length);
            }

            return result;
        }
        /// <summary>
        /// Trim the end of a string with a specific string for the first occurrence
        /// Based on: https://stackoverflow.com/a/4335913
        /// </summary>
        /// <param name="target"></param>
        /// <param name="trimString"></param>
        /// <returns></returns>
        public static string TrimEndOnce(this string target, string trimString) {
            if (string.IsNullOrEmpty(trimString)) return target;

            string result = target;
            if (result.EndsWith(trimString)) {
                result = result.Substring(0, result.Length - trimString.Length);
            }

            return result;
        }
    }

    public static class PropertyInfoExtensions {
        /// <summary>
        /// Determines if a property is nullable
        /// Nullable reference types will return true but not standard reference types
        /// https://stackoverflow.com/a/58454489
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public static bool IsNullable(this PropertyInfo property) {
            return Helpers.IsNullableHelper(property.PropertyType, property.DeclaringType, property.CustomAttributes);
        }
    }

    public static class FieldInfoExtensions {
        /// <summary>
        /// Determines if a field is nullable
        /// Nullable reference types will return true but not standard reference types
        /// https://stackoverflow.com/a/58454489
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        public static bool IsNullable(this FieldInfo field) {
            return Helpers.IsNullableHelper(field.FieldType, field.DeclaringType, field.CustomAttributes);
        }
    }

    public static class ParameterInfoExtensions {
        /// <summary>
        /// Determines if a parameter is nullable
        /// Nullable reference types will return true but not standard reference types
        /// https://stackoverflow.com/a/58454489
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static bool IsNullable(this ParameterInfo parameter) {
            return Helpers.IsNullableHelper(parameter.ParameterType, parameter.Member, parameter.CustomAttributes);
        }
    }

    internal static class Helpers {
        /// <summary>
        /// Determines if a type is nullable
        /// Nullable reference types will return true but not standard reference types
        /// https://stackoverflow.com/a/58454489
        /// </summary>
        /// <param name="memberType"></param>
        /// <param name="declaringType"></param>
        /// <param name="customAttributes"></param>
        /// <returns></returns>
        public static bool IsNullableHelper(Type memberType, MemberInfo declaringType, IEnumerable<CustomAttributeData> customAttributes) {
            if (memberType.IsValueType)
                return Nullable.GetUnderlyingType(memberType) != null;

            var nullable = customAttributes
                .FirstOrDefault(x => x.AttributeType.FullName == "System.Runtime.CompilerServices.NullableAttribute");
            if (nullable != null && nullable.ConstructorArguments.Count == 1) {
                var attributeArgument = nullable.ConstructorArguments[0];
                if (attributeArgument.ArgumentType == typeof(byte[])) {
                    var args = (ReadOnlyCollection<CustomAttributeTypedArgument>)attributeArgument.Value!;
                    if (args.Count > 0 && args[0].ArgumentType == typeof(byte)) {
                        return (byte)args[0].Value! == 2;
                    }
                }
                else if (attributeArgument.ArgumentType == typeof(byte)) {
                    return (byte)attributeArgument.Value! == 2;
                }
            }

            for (var type = declaringType; type != null; type = type.DeclaringType) {
                var context = type.CustomAttributes
                    .FirstOrDefault(x => x.AttributeType.FullName == "System.Runtime.CompilerServices.NullableContextAttribute");
                if (context != null &&
                    context.ConstructorArguments.Count == 1 &&
                    context.ConstructorArguments[0].ArgumentType == typeof(byte))
                {
                    return (byte)context.ConstructorArguments[0].Value! == 2;
                }
            }

            // Couldn't find a suitable attribute
            return false;
        }
    }

    public static class TextureExtensions {

        /// <summary>
        /// Returns a vertically flipped copy of the texture
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        public static Texture2D FlipVertically(this Texture2D original){
            Texture2D flippedTexture = new Texture2D(original.width, original.height);

            int width = original.width;
            int height = original.height;

            for(int x = 0; x < width; x++){
                for(int y = 0; y < height; y++){
                    var pixel = original.GetPixel(x,y);
                    flippedTexture.SetPixel(x, height - y - 1, pixel);
                }
            }
            flippedTexture.Apply();

            return flippedTexture;
        }
    }

    public static class Vector3Extensions {
        public static float[] ToArray(this Vector3 vector) {
            return new float[] {vector.x, vector.y, vector.z};
        }
    }
}