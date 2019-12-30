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
    public class SimHashUtil
    {
        private static Dictionary<SimHashes, string> hashToName = new Dictionary<SimHashes, string>();
        private static Dictionary<string, object> nameToHash = new Dictionary<string, object>();
        
        [UseLatest("SimHashUtil_AddHash")]
        public static readonly Action<SimHashes, string> AddHash = (SimHashes hash, string name) =>
        {
            Log.Spam($"Adding sim hash {name}: {hash}");

            if (hashToName.TryGetValue(hash, out string oldName))
            {
                if (oldName != name)
                {
                    Log.Error($"SimHashes {hash} registered with multiple names: {oldName}, {name}");
                }

                return;
            }

            hashToName.Add(hash, name);
            nameToHash.Add(name, hash);
        };

        public static SimHashes CreateHash(string name)
        {
            SimHashes hash = (SimHashes)Hash.SDBMLower(name);
            AddHash(hash, name);

            return hash;
        }

        [PatchOnce]
        [HarmonyPatch(typeof(Enum), nameof(Enum.ToString), new Type[] { })]
        public static class SimHashes_ToString_Patch
        {
            public static bool Prepare() => InitOnce.Patch;

            public static bool Prefix(ref Enum __instance, ref string __result)
            {
                if (!(__instance is SimHashes)) return true;
                return !hashToName.TryGetValue((SimHashes)__instance, out __result);
            }
        }

        [PatchOnce]
        [HarmonyPatch(typeof(Enum), nameof(Enum.Parse), new Type[] { typeof(Type), typeof(string), typeof(bool) })]
        public static class SimHashes_Parse_Patch
        {
            public static bool Prepare() => InitOnce.Patch;

            public static bool Prefix(Type enumType, string value, ref object __result)
            {
                if (!enumType.Equals(typeof(SimHashes))) return true;
                return !nameToHash.TryGetValue(value, out __result);
            }
        }
    }
}
