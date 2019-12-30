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
    public class SubstanceLoader
    {
        private static string path = Application.streamingAssetsPath + "/substances/";

        public class SubstanceEntry
        {
            public string elementId { get; set; }
            public string baseSubstance { get; set; }

            public Element.State state { get; set; }

            public YamlColor color { get; set; }
            public YamlColor uiColor { get; set; }
            public YamlColor conduitColor { get; set; }
        }

        private class SubstanceEntryCollection : Yaml.Collection<SubstanceEntry>
        {
            public SubstanceEntry[] substances { get; set; }

            public SubstanceEntry[] Entries => substances;
        }

        public static void AddSubstance(SubstanceEntry entry, SubstanceTable substanceTable)
        {
            Log.Spam($"Adding substance {entry.elementId}");

            SimHashes elementHash = SimHashUtil.CreateHash(entry.elementId);

            if (entry.baseSubstance == null)
            {
                Log.Error($"Missing baseSubstance for substance {entry.elementId}");
                return;
            }

            var baseSubstance = substanceTable.GetSubstance((SimHashes)Hash.SDBMLower(entry.baseSubstance));
            if (baseSubstance == null)
            {
                Log.Error($"Invalid baseSubstance {entry.baseSubstance} for substance {entry.elementId}");
                return;
            }

            Color32 color = entry.color ?? baseSubstance.colour;
            Color32 uiColor = entry.uiColor ?? baseSubstance.uiColour;
            Color32 conduitColor = entry.conduitColor ?? baseSubstance.conduitColour;

            var substance = ModUtil.CreateSubstance(entry.elementId, entry.state, baseSubstance.anim, baseSubstance.material, color, uiColor, conduitColor);
            substanceTable.GetList().Add(substance);
        }
        
        [PatchOnce]
        [HarmonyPatch(typeof(ElementLoader), nameof(ElementLoader.Load))]
        public class ElementLoader_Load
        {
            public static bool Prepare() => InitOnce.Patch;

            public static void Prefix(ref Hashtable substanceList, SubstanceTable substanceTable)
            {
                List<SubstanceEntry> substances = Yaml.CollectFromYAML<SubstanceEntry, SubstanceEntryCollection>(path);
                Log.Info($"Adding {substances.Count} substances");

                foreach ( SubstanceEntry entry in substances )
                {
                    AddSubstance(entry, substanceTable);
                }
            }
        }
    }
}
