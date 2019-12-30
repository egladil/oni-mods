using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Egladil
{
    public class MaterialTags
    {
        private static HashSet<Tag> CollectTags(params List<Tag>[] add)
        {
            var tagSet = new HashSet<Tag>();

            foreach (var tags in add)
            {
                foreach (Tag tag in tags)
                {
                    tagSet.Add(tag);
                }
            }

            return tagSet;
        }

        private static void SetTag(Element element, Tag tag)
        {
            if (element.tag != tag && !element.oreTags.Contains(tag))
            {
                var tags = element.oreTags.ToList();
                tags.Add(tag);
                element.oreTags = tags.ToArray();

                Log.Spam($"{element.id}: Added tag {tag}");
            }

            if (element.materialCategory != tag)
            {
                Log.Spam($"{element.id}: Changed category from {element.materialCategory} to {tag}");
                element.materialCategory = tag;
            }
        }

        private static void FixCategories()
        {
            Debug.Log("Fixing categories");

            HashSet<Tag> skipTags = CollectTags(
                TUNING.STORAGEFILTERS.BAGABLE_CREATURES,
                TUNING.STORAGEFILTERS.FOOD,
                TUNING.STORAGEFILTERS.GASES,
                TUNING.STORAGEFILTERS.LIQUIDS,
                TUNING.STORAGEFILTERS.NOT_EDIBLE_SOLIDS,
                TUNING.STORAGEFILTERS.SWIMMING_CREATURES);
            skipTags.Add(GameTags.Special);

            int updateCount = 0;
            foreach (Element element in ElementLoader.elements)
            {
                if (skipTags.Contains(element.materialCategory))
                {
                    continue;
                }

                switch (element.state)
                {
                    case Element.State.Gas:
                        SetTag(element, GameTags.Unbreathable);
                        break;
                    case Element.State.Liquid:
                        SetTag(element, GameTags.Liquid);
                        break;
                    case Element.State.Solid:
                        if (element.highTemp <= 293.15f)
                        {
                            SetTag(element, GameTags.Liquifiable);
                        }
                        else
                        {
                            SetTag(element, GameTags.Other);
                        }
                        break;
                    default:
                        continue;
                }

                updateCount++;
            }

            Log.Info($"Updated {updateCount} materials");
        }

        [HarmonyPatch(typeof(ElementLoader), nameof(ElementLoader.Load))]
        public class ElementLoader_LoadUserElementData
        {
            public static void Postfix()
            {
                FixCategories();

                foreach (Element element in ElementLoader.elements)
                {
                    var tags = element.oreTags.Join();
                    Log.Spam($"{element.tag}: {tags}");
                }
            }
        }
    }
}
