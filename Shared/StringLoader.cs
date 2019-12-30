using Harmony;
using Klei;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Egladil
{
    public class StringLoader
    {
        private static string path = Application.streamingAssetsPath + "/extended_strings/";
        
        private static int AddStrings(object obj, string id)
        {
            if (obj is string)
            {
                var str = (string)obj;
                if (id == null)
                {
                    Log.Error($"String {str} has null id");
                    return 0;
                }

                Log.Spam($"Adding string {id}: {str}");
                Strings.Add(id, str);
                return 1;
            }
            else if (obj is List<object>)
            {
                int count = 0;
                foreach (var child in (List<object>)obj)
                {
                    count += AddStrings(child, id);
                }

                return count;
            }
            else if (obj is Dictionary<object, object>)
            {
                var prefix = id != null ? id + '.' : "";

                int count = 0;
                foreach (var entry in (Dictionary<object, object>)obj)
                {
                    count += AddStrings(entry.Value, prefix + entry.Key);
                }

                return count;
            }

            if (obj == null)
            {
                Log.Error($"Unexpected null string with prefix {id}");
            }
            else
            {
                Log.Error($"Unexpected string type {obj.GetType().FullName} with prefix {id}");
            }

            return 0;
        }

        public static void OnInitOnceComplete()
        {
            if (!InitOnce.Patch) return;

            List<object> strings = Yaml.CollectFromYAML<object>(path);

            int count = AddStrings(strings, null);
            Log.Info($"Added {count} strings");
        }

        private static HashSet<string> missingStrings = new HashSet<string>();
        
        [PatchOnce]
        [HarmonyPatch(typeof(Strings), "GetInvalidString")]
        public class Strings_GetInvalidString_Patch
        {
            public static bool Prepare() => InitOnce.Patch && Log.EnableSpamLog;

            public static bool Prefix(StringKey[] keys)
            {
                if (keys != null)
                {
                    var key = keys.Join(x => x.String, ".");
                    lock (missingStrings)
                    {
                        if (!missingStrings.Contains(key))
                        {
                            missingStrings.Add(key);
                            Log.Spam($"Missing string {key}");
                        }
                    }
                }
                return true;
            }
        }
    }
}
