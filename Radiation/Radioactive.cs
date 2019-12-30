using Harmony;
using System.Linq;
using UnityEngine;

namespace Egladil
{
    public class Radioactive
    {
        public static readonly Tag RadioactiveTag = TagManager.Create("Radioactive");

        [HarmonyPatch(typeof(ElementLoader), nameof(ElementLoader.Load))]
        public class ElementLoader_LoadUserElementData
        {
            private static void MakeRadioactive(Element element)
            {
                if (element == null || element.oreTags.Contains(RadioactiveTag))
                {
                    return;
                }

                Log.Spam($"Marking {element.tag} as radioactive");
                var tags = element.oreTags.ToList();
                tags.Add(RadioactiveTag);
                element.oreTags = tags.ToArray();
            }

            public static void Postfix()
            {
                MakeRadioactive(ElementLoader.GetElement(SimHashes.Radium.ToString()));
                MakeRadioactive(ElementLoader.GetElement(SimHashes.UraniumOre.ToString()));
                MakeRadioactive(ElementLoader.GetElement(SimHashes.EnrichedUranium.ToString()));
                MakeRadioactive(ElementLoader.GetElement(SimHashes.DepletedUranium.ToString()));
            }
        }

        /*
        [HarmonyPatch(typeof(ElementChunk), "OnSpawn")]
        public class ElementChunk_OnSpawn_Patch
        {
            public static void Postfix(ElementChunk __instance)
            {
                var primaryElement = __instance.GetComponent<PrimaryElement>();
                if (primaryElement.Element.oreTags.Contains(RadioactiveTag))
                {
                    __instance.FindOrAdd<Radioactive>();
                }
            }
        }
        */
    }
}
