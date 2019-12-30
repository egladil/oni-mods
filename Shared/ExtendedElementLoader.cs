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
    public class ExtendedElementLoader
    {
        private static string path = Application.streamingAssetsPath + "/extended_elements/";

        public class AttributeModifierEntry
        {
            public string attributeId { get; set; }
            public float value { get; set; }
            public string description { get; set; }
            public bool multiplier { get; set; }
            public bool uiOnly { get; set; }
            public bool readOnly { get; set; }
        }

        public class ElementEntry : ElementLoader.ElementEntry
        {
            public SubstanceLoader.SubstanceEntry substance { get; set; }
            public AttributeModifierEntry[] attributeModifiers { get; set; }
        }

        private class ElementEntryCollection : Yaml.Collection<ElementEntry>
        {
            public ElementEntry[] elements { get; set; }
            public ElementEntry[] Entries => elements;
        }

        private static List<ElementEntry> elementEntries;

        public static Klei.AI.AttributeModifier MakeAttributeModifier(AttributeModifierEntry entry)
        {
            return new Klei.AI.AttributeModifier(entry.attributeId, entry.value, entry.description, entry.multiplier, entry.uiOnly, entry.readOnly);
        }

        [PatchOnce]
        [HarmonyPatch(typeof(ElementLoader), nameof(ElementLoader.Load))]
        public class ElementLoader_Load
        {
            public static bool Prepare() => InitOnce.Patch;

            public static void Prefix(ref Hashtable substanceList, SubstanceTable substanceTable)
            {
                elementEntries = Yaml.CollectFromYAML<ElementEntry, ElementEntryCollection>(path);
                Log.Info($"Adding {elementEntries.Count} elements");
                
                foreach (ElementEntry entry in elementEntries )
                {
                    if (entry.substance != null)
                    {
                        entry.substance.elementId = entry.elementId;
                        SubstanceLoader.AddSubstance(entry.substance, substanceTable);
                    }
                }
            }

            public static void Postfix()
            {
                foreach (ElementEntry entry in elementEntries)
                {
                    var element = ElementLoader.FindElementByName(entry.elementId);
                    if (element == null)
                    {
                        Log.Error($"Element {entry.elementId} was never created");
                        continue;
                    }

                    if (entry.attributeModifiers != null)
                    {
                        foreach (AttributeModifierEntry attr in entry.attributeModifiers)
                        {
                            Log.Spam($"Adding attribute {attr.attributeId} to {entry.elementId}");

                            attr.description = attr.description ?? element.name;
                            element.attributeModifiers.Add(MakeAttributeModifier(attr));
                        }
                    }
                }
            }
        }

        [PatchOnce]
        [HarmonyPatch(typeof(ElementLoader), nameof(ElementLoader.CollectElementsFromYAML))]
        public class ElementLoader_CollectElementsFromYAML
        {
            public static bool Prepare() => InitOnce.Patch;

            public static void Postfix(List<ElementLoader.ElementEntry> __result)
            {
                foreach (ElementEntry entry in elementEntries)
                {
                    Log.Spam($"Adding element {entry.elementId}");
                    __result.Add(entry);
                }
            }
        }
    }
}
