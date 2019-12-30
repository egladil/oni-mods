using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Egladil
{
    public class Test
    {
        [HarmonyPatch(typeof(HarmonyInstance), "PatchAll", typeof(Assembly))]
        public class HarmonyInstance_PatchAll_Patch
        {
            public static bool Prefix(HarmonyInstance __instance, Assembly assembly)
            {
                Debug.Log($"Begin HarmonyInstance.PatchAll: {assembly.GetName().Name}");

                try
                {
                    assembly.GetTypes().Do(delegate (Type type)
                    {
                        List<HarmonyMethod> harmonyMethods = type.GetHarmonyMethods();
                        if (harmonyMethods != null && harmonyMethods.Count() > 0)
                        {
                            HarmonyMethod attributes = HarmonyMethod.Merge(harmonyMethods);
                            PatchProcessor patchProcessor = new PatchProcessor(__instance, type, attributes);
                            patchProcessor.Patch();
                        }
                    });
                } catch (Exception ex)
                {
                    Debug.LogException(ex);
                    throw ex;
                }

                Debug.Log($"End HarmonyInstance.PatchAll: {assembly.GetName().Name}");

                return false;
            }
        }
    }
}
