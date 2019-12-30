using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Egladil
{
    public class CloningBayConfig : IBuildingConfig
    {
        public const string ID = "CloningBay";
        private const float OUTPUT_TEMPERATURE = 310.15f;

        public override BuildingDef CreateBuildingDef()
        {
            int width = 2;
            int height = 3;
            string anim = "bed_medical_kanim";
            int hitpoints = 30;
            float constructionTime = 50;
            var mass = TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER4;
            var material = TUNING.MATERIALS.REFINED_METALS;
            float meltingPoint = 1600;
            var buildLocationRule = BuildLocationRule.OnFloor;
            var noise = TUNING.NOISE_POLLUTION.NOISY.TIER6;
            var decor = TUNING.BUILDINGS.DECOR.PENALTY.TIER2;

            BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef(ID, width, height, anim, hitpoints, constructionTime, mass, material, meltingPoint, buildLocationRule, decor, noise);

            buildingDef.RequiresPowerInput = true;
            buildingDef.EnergyConsumptionWhenActive = 1000f;
            buildingDef.SelfHeatKilowattsWhenActive = 16f;
            buildingDef.ViewMode = OverlayModes.Power.ID;
            buildingDef.AudioCategory = "HollowMetal";

            return buildingDef;
        }

        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            go.AddOrGet<DropAllWorkable>();
            go.AddOrGet<BuildingComplete>().isManuallyOperated = true;

            ComplexFabricator complexFabricator = go.AddOrGet<ComplexFabricator>();
            complexFabricator.resultState = ComplexFabricator.ResultState.Heated;
            complexFabricator.heatedTemperature = OUTPUT_TEMPERATURE;
            complexFabricator.sideScreenStyle = ComplexFabricatorSideScreen.StyleSetting.ListQueueHybrid;
            complexFabricator.duplicantOperated = true;

            go.AddOrGet<FabricatorIngredientStatusManager>();
            go.AddOrGet<CopyBuildingSettings>();

            ComplexFabricatorWorkable complexFabricatorWorkable = go.AddOrGet<ComplexFabricatorWorkable>();
            BuildingTemplates.CreateComplexFabricatorStorage(go, complexFabricator);
            complexFabricatorWorkable.overrideAnims = new KAnimFile[1] { Assets.GetAnim("anim_interacts_supermaterial_refinery_kanim") };

            Prioritizable.AddRef(go);

            AddRecipes();
        }

        private void AddRecipes()
        {
            List<Type> allTypes = new List<Type>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var types = assembly.GetTypes();
                if (types != null)
                {
                    allTypes.AddRange(types);
                }
            }

            Log.Spam($"Adding cloning recipes from {allTypes.Count} types");

            var eggIn = new RecipeLoader.ElementEntry() { amount = 1 };
            var eggOut = new RecipeLoader.ElementEntry() { amount = 2 };
            RecipeLoader.RecipeEntry recipe = new RecipeLoader.RecipeEntry();
            recipe.fabricator = ID;
            recipe.time = 100;
            recipe.nameDisplay = ComplexRecipe.RecipeNameDisplay.Result;
            recipe.ingredients = new RecipeLoader.ElementEntry[]
            {
                eggIn,
                new RecipeLoader.ElementEntry() { material = SimHashes.Creature.ToString(), amount = 100 },
                new RecipeLoader.ElementEntry() { material = SimHashes.Water.ToString(), amount = 100 },
            };
            recipe.results = new RecipeLoader.ElementEntry[] { eggOut };

            var added = new HashSet<string>();

            foreach (var type in allTypes)
            {
                if (!typeof(IEntityConfig).IsAssignableFrom(type) || type.IsAbstract || type.IsInterface) continue;

                var field = type.GetField("EGG_ID") ?? type.GetField("EggId");
                if (field == null)
                {
                    Log.Spam($"Entity config {type.FullName} has no egg id");
                    continue;
                }

                var eggId = (string)field.GetValue(null);
                if (added.Contains(eggId)) continue;
                added.Add(eggId);

                eggIn.material = eggId;
                eggOut.material = eggId;

                RecipeLoader.AddRecipe(recipe);
            }

            Log.Info($"Added {added.Count} cloning recipes");
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            go.GetComponent<KPrefabID>().prefabSpawnFn += (GameObject gameObject) =>
            {
                ComplexFabricatorWorkable component = gameObject.GetComponent<ComplexFabricatorWorkable>();
                component.WorkerStatusItem = Db.Get().DuplicantStatusItems.Processing;
                component.AttributeConverter = Db.Get().AttributeConverters.MachinerySpeed;
                component.AttributeExperienceMultiplier = TUNING.DUPLICANTSTATS.ATTRIBUTE_LEVELING.PART_DAY_EXPERIENCE;
                component.SkillExperienceSkillGroup = Db.Get().SkillGroups.Technicals.Id;
                component.SkillExperienceMultiplier = TUNING.SKILLS.PART_DAY_EXPERIENCE;
            };
        }

        [HarmonyPatch(typeof(GeneratedBuildings), nameof(GeneratedBuildings.LoadGeneratedBuildings))]
        public static class GeneratedBuildings_LoadGeneratedBuildings_Patch
        {
            public static void Prefix()
            {
                HashedString category = "Refining";
                var plan = TUNING.BUILDINGS.PLANORDER.Find(x => x.category == category);
                if (plan.category == category)
                {
                    ((IList<string>)plan.data).Add(ID);
                }
            }
        }

        [HarmonyPatch(typeof(Db), nameof(Db.Initialize))]
        public static class Db_Initialize_Patch
        {
            public static void Prefix()
            {
                string category = "MedicineIV";
                var tech = Database.Techs.TECH_GROUPING[category].ToList();
                tech.Add(ID);
                Database.Techs.TECH_GROUPING[category] = tech.ToArray();
            }
        }
    }
}
