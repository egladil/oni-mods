﻿using Harmony;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Egladil
{
    public static class ExtendedWire
    {
        public enum WattageRating
        {
            Max5W,
            Max1kW,
            Max2kW,
            Max20kW,
            Max50kW,
            Max1MW,
            Max1GW,
            NumRatings
        }

        public static Wire.WattageRating ToWireWattageRating(this WattageRating rating) => (Wire.WattageRating)(int)rating;
        public static WattageRating ToExtendedWireWattageRating(this Wire.WattageRating rating) => (WattageRating)(int)rating;

        public static GameUtil.WattageFormatterUnit GetFormatterUnit(this Wire.WattageRating rating) => rating.ToExtendedWireWattageRating().GetFormatterUnit();
        public static GameUtil.WattageFormatterUnit GetFormatterUnit(this WattageRating rating)
        {
            switch(rating)
            {
                case WattageRating.Max5W:
                case WattageRating.Max2kW:
                    return GameUtil.WattageFormatterUnit.Watts;
                case WattageRating.Max20kW:
                case WattageRating.Max50kW:
                    return GameUtil.WattageFormatterUnit.Kilowatts;
                case WattageRating.Max1MW:
                case WattageRating.Max1GW:
                    return GameUtil.WattageFormatterUnit.Automatic;
                default:
                    return GameUtil.WattageFormatterUnit.Watts;
            }
        }

        public const string CONDUCTOR = "Conductor";
        public static readonly Tag ConductorTag = TagManager.Create(CONDUCTOR);

        public static readonly string[] MEGAWATT_WIRE_MATERIALS = new string[] { CONDUCTOR, TUNING.MATERIALS.PLASTIC };
        public static readonly float[] MEGAWATT_WIRE_MASS_KG = new float[] { TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER3[0], TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER0[0] };

        public static readonly string[] GIGAWATT_WIRE_MATERIALS = new string[] { SimHashes.Ceramic.ToString(), SimHashes.Fullerene.ToString(), TUNING.MATERIALS.PLASTIC };
        public static readonly float[] GIGAWATT_WIRE_MASS_KG = new float[] { TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER3[0], TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER0[0], TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER1[0] };

        [HarmonyPatch(typeof(ElementLoader), nameof(ElementLoader.Load))]
        public class ElementLoader_LoadUserElementData
        {
            private static void MakeConductor(Element element)
            {
                if (element == null || element.oreTags.Contains(ConductorTag))
                {
                    return;
                }

                Log.Spam($"Marking {element.tag} as conductor");
                var tags = element.oreTags.ToList();
                tags.Add(ConductorTag);
                element.oreTags = tags.ToArray();
            }

            public static void Postfix()
            {
                MakeConductor(ElementLoader.GetElement(SimHashes.Copper.ToString()));
                MakeConductor(ElementLoader.GetElement(SimHashes.Gold.ToString()));
                MakeConductor(ElementLoader.GetElement(SimHashes.Aluminum.ToString()));
            }
        }
        
        [HarmonyPatch(typeof(BuildingDef), "Build")]
        public class BuildingDef_Build_Patch
        {
            public static bool Prefix(BuildingDef __instance, ref float temperature)
            {
                if (__instance.Overheatable && (__instance.PrefabID == GigawattWireConfig.ID || __instance.PrefabID == GigawattWireBridgeConfig.ID))
                {
                    temperature = Mathf.Min(temperature, __instance.OverheatTemperature + 199);
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(Wire), "GetMaxWattageAsFloat")]
        public class Wire_GetMaxWattageAsFloat_Patch
        {
            public static bool Prefix(Wire.WattageRating rating, ref float __result)
            {
                if (rating < Wire.WattageRating.NumRatings)
                {
                    return true;
                }

                switch ((WattageRating)rating)
                {
                    case WattageRating.Max1MW:
                        __result = 1e6f;
                        break;
                    case WattageRating.Max1GW:
                        __result = 1e9f;
                        break;
                    default:
                        __result = 0;
                        break;
                }

                return false;
            }
        }

        [HarmonyPatch(typeof(Wire), "OnPrefabInit")]
        public class Wire_OnPrefabInit_Patch
        {
            public static bool Prefix(Wire __instance, ref StatusItem ___WireCircuitStatus, ref StatusItem ___WireMaxWattageStatus)
            {
                if (___WireCircuitStatus == null)
                {
                    ___WireCircuitStatus = new StatusItem("WireCircuitStatus", "BUILDING", string.Empty, StatusItem.IconType.Info, NotificationType.Neutral, false, OverlayModes.None.ID).SetResolveStringCallback(delegate (string str, object data)
                    {
                        Wire wire2 = (Wire)data;
                        int cell2 = Grid.PosToCell(wire2.transform.GetPosition());
                        CircuitManager circuitManager2 = Game.Instance.circuitManager;
                        ushort circuitID2 = circuitManager2.GetCircuitID(cell2);
                        float wattsUsedByCircuit = circuitManager2.GetWattsUsedByCircuit(circuitID2);
                        GameUtil.WattageFormatterUnit unit2 = wire2.MaxWattageRating.GetFormatterUnit();
                        float maxWattageAsFloat2 = Wire.GetMaxWattageAsFloat(wire2.MaxWattageRating);
                        string wireLoadColor = GameUtil.GetWireLoadColor(wattsUsedByCircuit, maxWattageAsFloat2);
                        str = str.Replace("{CurrentLoadAndColor}", (!(wireLoadColor == Color.white.ToHexString())) ? ("<color=#" + wireLoadColor + ">" + GameUtil.GetFormattedWattage(wattsUsedByCircuit, unit2) + "</color>") : GameUtil.GetFormattedWattage(wattsUsedByCircuit, unit2));
                        str = str.Replace("{MaxLoad}", GameUtil.GetFormattedWattage(maxWattageAsFloat2, unit2));
                        str = str.Replace("{WireType}", __instance.GetProperName());
                        return str;
                    });
                }
                if (___WireMaxWattageStatus == null)
                {
                    ___WireMaxWattageStatus = new StatusItem("WireMaxWattageStatus", "BUILDING", string.Empty, StatusItem.IconType.Info, NotificationType.Neutral, false, OverlayModes.None.ID).SetResolveStringCallback(delegate (string str, object data)
                    {
                        Wire wire = (Wire)data;
                        GameUtil.WattageFormatterUnit unit = wire.MaxWattageRating.GetFormatterUnit();
                        int cell = Grid.PosToCell(wire.transform.GetPosition());
                        CircuitManager circuitManager = Game.Instance.circuitManager;
                        ushort circuitID = circuitManager.GetCircuitID(cell);
                        float wattsNeededWhenActive = circuitManager.GetWattsNeededWhenActive(circuitID);
                        float maxWattageAsFloat = Wire.GetMaxWattageAsFloat(wire.MaxWattageRating);
                        str = str.Replace("{TotalPotentialLoadAndColor}", (!(wattsNeededWhenActive > maxWattageAsFloat)) ? GameUtil.GetFormattedWattage(wattsNeededWhenActive, unit) : ("<color=#" + new Color(251f / 255f, 176f / 255f, 59f / 255f).ToHexString() + ">" + GameUtil.GetFormattedWattage(wattsNeededWhenActive, unit) + "</color>"));
                        str = str.Replace("{MaxLoad}", GameUtil.GetFormattedWattage(maxWattageAsFloat, unit));
                        return str;
                    });
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(GameUtil), "GetFormattedWattage")]
        public class GameUtil_GetFormattedWattage_Patch
        {
            private static readonly StringKey MEGAWATT = new StringKey("STRINGS.UI.UNITSUFFIXES.ELECTRICAL.MEGAWATT");
            private static readonly StringKey GIGAWATT = new StringKey("STRINGS.UI.UNITSUFFIXES.ELECTRICAL.GIGAWATT");
            private static readonly StringKey TERAWATT = new StringKey("STRINGS.UI.UNITSUFFIXES.ELECTRICAL.TERAWATT");

            public static bool Prefix(float watts, GameUtil.WattageFormatterUnit unit, ref string __result)
            {
                if (unit != GameUtil.WattageFormatterUnit.Automatic) return true;
                if (watts <= 1e6f) return true;

                StringKey key;
                
                if (watts <= 1e9f)
                {
                    key = MEGAWATT;
                    watts /= 1e6f;
                }
                else if (watts <= 1e12f)
                {
                    key = GIGAWATT;
                    watts /= 1e9f;
                }
                else
                {
                    key = TERAWATT;
                    watts /= 1e12f;
                }

                __result = GameUtil.FloatToString(watts, "###0.##") + Strings.Get(key);
                return false;
            }
        }
        
        [HarmonyPatch(typeof(CircuitManager), "Rebuild")]
        public class CircuitManager_Rebuild_Patch
        {
            public static bool Prefix(object ___circuitInfo)
            {
                var circuitInfo = (IList)___circuitInfo;

                for (int i = 0; i < circuitInfo.Count; i++)
                {
                    object info = circuitInfo[i];

                    var bridgeGroups = Traverse.Create(info).Field<List<WireUtilityNetworkLink>[]>("bridgeGroups");

                    Log.Spam($"FixBridgeGroups A {i}: {bridgeGroups.Value.Length}");

                    if (bridgeGroups.Value.Length >= (int)WattageRating.NumRatings) continue;

                    var list = bridgeGroups.Value.ToList();
                    while (list.Count < (int)WattageRating.NumRatings)
                    {
                        list.Add(new List<WireUtilityNetworkLink>());
                    }
                    bridgeGroups.Value = list.ToArray();

                    Log.Spam($"FixBridgeGroups B {i}: {bridgeGroups.Value.Length}");

                    circuitInfo[i] = info;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(ElectricalUtilityNetwork), MethodType.Constructor)]
        public class ElectricalUtilityNetwork_CTor_Patch
        {
            public static void Postfix(ref List<Wire>[] ___wireGroups)
            {
                ___wireGroups = new List<Wire>[(int)WattageRating.NumRatings];
            }
        }

        [HarmonyPatch(typeof(ElectricalUtilityNetwork), "Reset")]
        public class ElectricalUtilityNetwork_Reset_Patch
        {
            public static bool Prefix(ElectricalUtilityNetwork __instance, UtilityNetworkGridNode[] grid, float ___timeOverloaded, List<Wire>[] ___wireGroups)
            {
                for (int i = 0; i < ___wireGroups.Length; i++)
                {
                    List<Wire> list = ___wireGroups[i];
                    if (list == null)
                    {
                        continue;
                    }
                    for (int j = 0; j < list.Count; j++)
                    {
                        Wire wire = list[j];
                        if (wire != null)
                        {
                            wire.circuitOverloadTime = ___timeOverloaded;
                            int num = Grid.PosToCell(wire.transform.GetPosition());
                            UtilityNetworkGridNode utilityNetworkGridNode = grid[num];
                            utilityNetworkGridNode.networkIdx = -1;
                            grid[num] = utilityNetworkGridNode;
                        }
                    }
                    list.Clear();
                }

                Traverse.Create(__instance).Method("RemoveOverloadedNotification").GetValue();

                return false;
            }
        }

        [HarmonyPatch(typeof(ElectricalUtilityNetwork), "UpdateOverloadTime")]
        public class ElectricalUtilityNetwork_UpdateOverloadTime_Patch
        {
            public static bool Prefix(ElectricalUtilityNetwork __instance, float dt, float watts_used, List<WireUtilityNetworkLink>[] bridgeGroups, ref float ___timeOverloaded, ref GameObject ___targetOverloadedWire, ref Notification ___overloadedNotification, ref float ___timeOverloadNotificationDisplayed, List<Wire>[] ___wireGroups)
            {
                Log.Spam($"UpdateOverloadTime: {___wireGroups.Length}, {bridgeGroups.Length}");

                bool flag = false;
                List<Wire> list = null;
                List<WireUtilityNetworkLink> list2 = null;
                for (int i = 0; i < ___wireGroups.Length; i++)
                {
                    List<Wire> list3 = ___wireGroups[i];
                    List<WireUtilityNetworkLink> list4 = bridgeGroups[i];
                    Wire.WattageRating rating = (Wire.WattageRating)i;
                    float maxWattageAsFloat = Wire.GetMaxWattageAsFloat(rating);
                    if (watts_used > maxWattageAsFloat && ((list4 != null && list4.Count > 0) || (list3 != null && list3.Count > 0)))
                    {
                        flag = true;
                        list = list3;
                        list2 = list4;
                        break;
                    }
                }
                list?.RemoveAll((Wire x) => x == null);
                list2?.RemoveAll((WireUtilityNetworkLink x) => x == null);
                if (flag)
                {
                    ___timeOverloaded += dt;
                    if (!(___timeOverloaded > 6f))
                    {
                        return false;
                    }
                    ___timeOverloaded = 0f;
                    if (___targetOverloadedWire == null)
                    {
                        if (list2 != null && list2.Count > 0)
                        {
                            int index = Random.Range(0, list2.Count);
                            ___targetOverloadedWire = list2[index].gameObject;
                        }
                        else if (list != null && list.Count > 0)
                        {
                            int index2 = Random.Range(0, list.Count);
                            ___targetOverloadedWire = list[index2].gameObject;
                        }
                    }
                    if (___targetOverloadedWire != null)
                    {
                        ___targetOverloadedWire.Trigger(-794517298, new BuildingHP.DamageSourceInfo
                        {
                            damage = 1,
                            source = STRINGS.BUILDINGS.DAMAGESOURCES.CIRCUIT_OVERLOADED,
                            popString = STRINGS.UI.GAMEOBJECTEFFECTS.DAMAGE_POPS.CIRCUIT_OVERLOADED,
                            takeDamageEffect = SpawnFXHashes.BuildingSpark,
                            fullDamageEffectName = "spark_damage_kanim",
                            statusItemID = Db.Get().BuildingStatusItems.Overloaded.Id
                        });
                    }
                    if (___overloadedNotification == null)
                    {
                        ___timeOverloadNotificationDisplayed = 0f;
                        string title = STRINGS.MISC.NOTIFICATIONS.CIRCUIT_OVERLOADED.NAME;
                        NotificationType type = NotificationType.BadMinor;
                        HashedString invalid = HashedString.Invalid;
                        Transform transform = ___targetOverloadedWire.transform;
                        ___overloadedNotification = new Notification(title, type, invalid, null, null, true, 0f, null, null, transform);
                        GameScheduler.Instance.Schedule("Power Tutorial", 2f, delegate
                        {
                            Tutorial.Instance.TutorialMessage(Tutorial.TutorialMessages.TM_Power);
                        });
                        Notifier notifier = Game.Instance.FindOrAdd<Notifier>();
                        notifier.Add(___overloadedNotification, string.Empty);
                    }
                }
                else
                {
                    ___timeOverloaded = Mathf.Max(0f, ___timeOverloaded - dt * 0.95f);
                    ___timeOverloadNotificationDisplayed += dt;
                    if (___timeOverloadNotificationDisplayed > 5f)
                    {
                        Traverse.Create(__instance).Method("RemoveOverloadedNotification").GetValue();
                    }
                }

                return false;
            }
        }

        [HarmonyPatch(typeof(GeneratedBuildings), nameof(GeneratedBuildings.LoadGeneratedBuildings))]
        public static class GeneratedBuildings_LoadGeneratedBuildings_Patch
        {
            public static void Prefix()
            {
                Buildings.AddToPlan(MegawattWireConfig.ID, "Power", after: "WireRefinedBridgeHighWattage");
                Buildings.AddToPlan(MegawattWireBridgeConfig.ID, "Power", after: MegawattWireConfig.ID);
                Buildings.AddToPlan(GigawattWireConfig.ID, "Power", after: MegawattWireBridgeConfig.ID);
                Buildings.AddToPlan(GigawattWireBridgeConfig.ID, "Power", after: GigawattWireConfig.ID);

                Buildings.AddToPlan(PowerTransformer100kWConfig.ID, "Power", after: "PowerTransformer");
                Buildings.AddToPlan(PowerTransformer2MWConfig.ID, "Power", after: PowerTransformer100kWConfig.ID);
            }
        }

        [HarmonyPatch(typeof(Db), nameof(Db.Initialize))]
        public static class Db_Initialize_Patch
        {
            public static void Prefix()
            {
                Buildings.AddToTech(MegawattWireConfig.ID, "RenewableEnergy");
                Buildings.AddToTech(MegawattWireBridgeConfig.ID, "RenewableEnergy");
                Buildings.AddToTech(PowerTransformer100kWConfig.ID, "RenewableEnergy");

                Buildings.AddToTech(GigawattWireConfig.ID, "CargoI");
                Buildings.AddToTech(GigawattWireBridgeConfig.ID, "CargoI");
                Buildings.AddToTech(PowerTransformer2MWConfig.ID, "CargoI");
            }
        }
    }
}