using Harmony;
using Klei.AI;
using Klei.AI.DiseaseGrowthRules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Egladil
{
    public class RadiationGerms : Disease
    {
        public const string ID = "Radiation";

        public static readonly HashedString HashedId = (HashedString)ID;

        private static Disease instance;
        public static Disease Instance => instance ?? (instance = Db.Get().Diseases.TryGet(ID));

        private static byte index = byte.MaxValue;
        public static byte Index => index != byte.MaxValue ? index : (index = Db.Get().Diseases.GetIndex(HashedId));

        static RadiationGerms()
        {
            Assets.instance.DiseaseVisualization.info.Add(new DiseaseVisualization.Info(ID)
            {
                overlayColourName = "germRadiation",
            });
        }

        public RadiationGerms()
        : base(ID, 255, 
              temperature_range: new RangeInfo(0.0f, 0, float.PositiveInfinity, float.PositiveInfinity),
              temperature_half_lives: RangeInfo.Idempotent(),
              pressure_range: new RangeInfo(0.0f, 0, float.PositiveInfinity, float.PositiveInfinity),
              pressure_half_lives: RangeInfo.Idempotent())
        {
            strength = 255;
        }

        public static readonly ElemGrowthInfo RADIATION_GROWTH_INFO = new ElemGrowthInfo
        {
            underPopulationDeathRate = 0f,
            populationHalfLife = float.PositiveInfinity,
            overPopulationHalfLife = float.PositiveInfinity,
            minCountPerKG = 0f,
            maxCountPerKG = float.PositiveInfinity,
            minDiffusionCount = 2,
            diffusionScale = 1f,
            minDiffusionInfestationTickCount = 1,
        };

        public static readonly ElemExposureInfo RADIATION_EXPOSURE_INFO = new ElemExposureInfo
        {
            populationHalfLife = float.PositiveInfinity,
        };

        protected override void PopulateElemGrowthInfo()
        {
            InitializeElemGrowthArray(ref elemGrowthInfo, RADIATION_GROWTH_INFO);
            elemGrowthInfo[ElementLoader.GetElementIndex(SimHashes.Polypropylene)] = RADIATION_GROWTH_INFO;
            elemGrowthInfo[ElementLoader.GetElementIndex(SimHashes.Vacuum)] = RADIATION_GROWTH_INFO;

            elemGrowthInfo[ElementLoader.GetElementIndex(SimHashes.Vacuum)].populationHalfLife = 5 * 600;
            elemGrowthInfo[ElementLoader.GetElementIndex(SimHashes.Vacuum)].diffusionScale = 1;

            AddGrowthRule(new GrowthRule
            {
                underPopulationDeathRate = new float?(RADIATION_GROWTH_INFO.underPopulationDeathRate),
                populationHalfLife = new float?(RADIATION_GROWTH_INFO.populationHalfLife),
                overPopulationHalfLife = new float?(RADIATION_GROWTH_INFO.overPopulationHalfLife),
                minCountPerKG = new float?(RADIATION_GROWTH_INFO.minCountPerKG),
                maxCountPerKG = new float?(RADIATION_GROWTH_INFO.maxCountPerKG),
                minDiffusionCount = new int?(RADIATION_GROWTH_INFO.minDiffusionCount),
                diffusionScale = new float?(RADIATION_GROWTH_INFO.diffusionScale),
                minDiffusionInfestationTickCount = new byte?(RADIATION_GROWTH_INFO.minDiffusionInfestationTickCount)
            });

            AddGrowthRule(new StateGrowthRule(Element.State.Solid)
            {
                populationHalfLife = 500 * 600,
                diffusionScale = 0.0001f,
            });

            AddGrowthRule(new StateGrowthRule(Element.State.Liquid)
            {
                populationHalfLife = 50 * 600,
                diffusionScale = 0.01f,
            });

            AddGrowthRule(new StateGrowthRule(Element.State.Gas)
            {
                populationHalfLife = 5 * 600,
                diffusionScale = 1f,
            });

            AddGrowthRule(new ElementGrowthRule(SimHashes.Katairite)
            {
                diffusionScale = 0.0000001f,
            });

            AddGrowthRule(new ElementGrowthRule(SimHashes.Lead)
            {
                diffusionScale = 0.0000001f,
            });

            AddGrowthRule(new ElementGrowthRule(SimHashes.MoltenLead)
            {
                diffusionScale = 0.0000001f,
            });

            AddGrowthRule(new ElementGrowthRule(SimHashes.MoltenLead)
            {
                diffusionScale = 0.0000001f,
            });

            AddGrowthRule(new ElementGrowthRule(SimHashes.LeadGas)
            {
                diffusionScale = 0.0000001f,
            });

            AddGrowthRule(new TagGrowthRule(Radioactive.RadioactiveTag)
            {
                populationHalfLife = float.PositiveInfinity,
                diffusionScale = 1f,
            });

            InitializeElemExposureArray(ref elemExposureInfo, RADIATION_EXPOSURE_INFO);

            AddExposureRule(new ExposureRule
            {
                populationHalfLife = RADIATION_EXPOSURE_INFO.populationHalfLife,
            });
        }


        [HarmonyPatch(typeof(Database.Diseases), MethodType.Constructor, typeof(ResourceSet))]
        public class Diseases_Constructor_Patch
        {
            public static void Postfix(Database.Diseases __instance)
            {
                __instance.Add(new RadiationGerms());
            }
        }

        [HarmonyPatch(typeof(AutoDisinfectable), nameof(AutoDisinfectable.RefreshChore))]
        public class AutoDisinfectable_RefreshChore_Patch
        {
            public static bool Prefix(AutoDisinfectable __instance, PrimaryElement ___primaryElement, ref Chore ___chore)
            {
                if (KMonoBehaviour.isLoadingScene) return true;
                if (___primaryElement.DiseaseIdx != RadiationGerms.Index) return true;

                if (___chore != null)
                {
                    ___chore.Cancel("AutoDisinfect Radiation");
                    ___chore = null;
                }

                return false;
            }
        }

        [HarmonyPatch(typeof(Disinfectable), nameof(Disinfectable.MarkForDisinfect))]
        public class Disinfectable_MarkForDisinfect_Patch
        {
            public static bool Prefix(AutoDisinfectable __instance)
            {
                var primaryElement = __instance.GetComponent<PrimaryElement>();
                if (primaryElement.DiseaseIdx != RadiationGerms.Index) return true;

                return false;
            }
        }
    }
}
