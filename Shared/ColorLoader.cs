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
    public class ColorLoader
    {
        private static string path = Application.streamingAssetsPath + "/named_colors/";
        
        public class ColorEntry : YamlColor
        {
            public string id { get; set; }
        }

        private class ColorEntryCollection : Yaml.Collection<ColorEntry>
        {
            public ColorEntry[] colors { get; set; }

            public ColorEntry[] Entries => colors;
        }

        public static void AddColor(Dictionary<string, Color32> namedLookup, ColorEntry entry)
        {
            Color32 color = (Color32)entry;
            Log.Spam($"Adding color {entry.id}: {color}");
            namedLookup[entry.id] = color;
        }

        [PatchOnce]
        [HarmonyPatch(typeof(ColorSet), "Init")]
        public class ColorSet_Init_Patch
        {
            public static bool Prepare() => InitOnce.Patch;

            public static bool Prefix(out bool __state, Dictionary<string, Color32> ___namedLookup)
            {
                __state = ___namedLookup == null;
                return true;
            }

            public static void Postfix(bool __state, Dictionary<string, Color32> ___namedLookup)
            {
                if (!__state) return;

                List<ColorEntry> colors = Yaml.CollectFromYAML<ColorEntry, ColorEntryCollection>(path);
                Log.Info($"Adding {colors.Count} colors");

                foreach (ColorEntry entry in colors )
                {
                    AddColor(___namedLookup, entry);
                }
            }
        }
    }
}
