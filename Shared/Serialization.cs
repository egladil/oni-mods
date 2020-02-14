using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Egladil
{
    public class Serialization
    {
        [HarmonyPatch(typeof(KSerialization.Manager), nameof(KSerialization.Manager.DeserializeDirectory))]
        public static class KSerialization_Manager_DeserializeDirectory_Patches
        {
            public static void Postfix(ref Dictionary<Type, KSerialization.DeserializationTemplate> ___deserializationTemplatesByType, Dictionary<string, KSerialization.DeserializationTemplate> ___deserializationTemplatesByTypeName)
            {
                foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
                {
                    if (!typeof(ISaveLoadable).IsAssignableFrom(type)) continue;
                    if (type.GetCustomAttributes(typeof(SkipSaveFileSerialization), false).Length > 0) continue;

                    if (___deserializationTemplatesByType.ContainsKey(type))
                    {
                        Log.Spam($"Skipping {type.FullName} in {Mod.Id}: Already set up");
                        continue;
                    }

                    if (!___deserializationTemplatesByTypeName.TryGetValue(type.FullName, out var template))
                    {
                        Log.Spam($"Skipping {type.FullName} in {Mod.Id}: No template");
                        continue;
                    }

                    Log.Spam($"Setting up {type.FullName} in {Mod.Id}");
                    ___deserializationTemplatesByType[type] = template;
                }
            }
        }
    }
}
