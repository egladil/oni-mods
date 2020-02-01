using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Egladil
{
    public class FusionReactorConfig : IBuildingConfig
    {
        public const string ID = "FusionReactor";

        private const SimHashes inputElement = SimHashes.Hydrogen;
        private const SimHashes outputElement = SimHashes.Helium;

        private ConduitPortInfo secondaryInput = new ConduitPortInfo(ConduitType.Gas, new CellOffset(0, 0));
        private ConduitPortInfo secondaryOutput = new ConduitPortInfo(ConduitType.Gas, new CellOffset(0, 1));

        public override BuildingDef CreateBuildingDef()
        {
            FusionReactor.InitRadiation();

            int width = 4;
            int height = 5;
            string anim = "supermaterial_refinery_kanim";
            int hitpoints = 30;
            float constructionTime = 480;
            var mass = new float[] { TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER6[0], TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER5[0], TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER5[0] };
            var material = new string[] { TUNING.MATERIALS.REFINED_METAL, SimHashes.Ceramic.ToString(), SimHashes.Lead.ToString() };
            float meltingPoint = 2600;
            var buildLocationRule = BuildLocationRule.OnFloor;
            var noise = TUNING.NOISE_POLLUTION.NOISY.TIER6;
            var decor = TUNING.BUILDINGS.DECOR.PENALTY.TIER2;

            BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef(ID, width, height, anim, hitpoints, constructionTime, mass, material, meltingPoint, buildLocationRule, decor, noise);

            buildingDef.RequiresPowerInput = true;
            buildingDef.EnergyConsumptionWhenActive = 1000f;
            buildingDef.SelfHeatKilowattsWhenActive = 16f;
            buildingDef.InputConduitType = ConduitType.Liquid;
            buildingDef.UtilityInputOffset = new CellOffset(-1, 1);
            buildingDef.OutputConduitType = ConduitType.Liquid;
            buildingDef.UtilityOutputOffset = new CellOffset(1, 0);
            buildingDef.ViewMode = OverlayModes.Power.ID;
            buildingDef.AudioCategory = "HollowMetal";
            buildingDef.PermittedRotations = PermittedRotations.FlipH;

            return buildingDef;
        }

        private void AttachPorts(GameObject go)
        {
            var gasInput = go.AddComponent<ConduitSecondaryInput>();
            gasInput.portInfo = secondaryInput;
            var gasOutput = go.AddComponent<ConduitSecondaryOutput>();
            gasOutput.portInfo = secondaryOutput;

            GeneratedBuildings.RegisterLogicPorts(go, LogicOperationalController.INPUT_PORTS_0_1);
        }

        public override void DoPostConfigurePreview(BuildingDef def, GameObject go)
        {
            base.DoPostConfigurePreview(def, go);
            AttachPorts(go);
        }

        public override void DoPostConfigureUnderConstruction(GameObject go)
        {
            base.DoPostConfigureUnderConstruction(go);
            AttachPorts(go);
        }

        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            go.GetComponent<KPrefabID>().AddTag(RoomConstraints.ConstraintTags.IndustrialMachinery);
            go.AddOrGet<Structure>();

            var coolantStorage = go.AddComponent<Storage>();
            coolantStorage.showDescriptor = true;
            coolantStorage.allowItemRemoval = false;
            coolantStorage.capacityKg = 1000;
            coolantStorage.SetDefaultStoredItemModifiers(Storage.StandardInsulatedStorage);

            var fusionStorage = go.AddComponent<Storage>();
            fusionStorage.showDescriptor = true;
            fusionStorage.allowItemRemoval = false;
            fusionStorage.capacityKg = 1;
            fusionStorage.SetDefaultStoredItemModifiers(Storage.StandardInsulatedStorage);

            var fusionReactor = go.AddOrGet<FusionReactor>();
            fusionReactor.coolantStorage = coolantStorage;
            fusionReactor.fusionStorage = fusionStorage;
            fusionReactor.producedHeatPerKg = 1e6f;
            fusionReactor.producedRadiationPerKg = 1e5f;

            var coolantConsumer = go.AddOrGet<ConduitConsumer>();
            coolantConsumer.conduitType = ConduitType.Liquid;
            coolantConsumer.storage = coolantStorage;
            coolantConsumer.ignoreMinMassCheck = true;
            coolantConsumer.forceAlwaysSatisfied = true;
            coolantConsumer.alwaysConsume = true;
            coolantConsumer.capacityKG = coolantStorage.capacityKg;

            var coolantDispenser = go.AddOrGet<ConduitDispenser>();
            coolantDispenser.conduitType = ConduitType.Liquid;
            coolantDispenser.storage = coolantStorage;
            coolantDispenser.alwaysDispense = true;

            var fuelInput = go.AddOrGet<ConduitInput>();
            fuelInput.portInfo = secondaryInput;
            fuelInput.storage = fusionStorage;
            fuelInput.consumptionRate = 1;
            fuelInput.capacityTag = inputElement.CreateTag();
            fuelInput.capacityKG = fusionStorage.capacityKg;
            fuelInput.forceAlwaysSatisfied = true;
            fuelInput.alwaysConsume = true;
            fuelInput.wrongElementResult = ConduitConsumer.WrongElementResult.Dump;

            var exhaustOutput = go.AddOrGet<ConduitOutput>();
            exhaustOutput.portInfo = secondaryOutput;
            exhaustOutput.storage = fusionStorage;
            exhaustOutput.alwaysDispense = true;
            exhaustOutput.elementFilter = new SimHashes[] { outputElement };

            var elementConvert = go.AddOrGet<ElementConverter>();
            elementConvert.SetStorage(fusionStorage);
            elementConvert.consumedElements = new ElementConverter.ConsumedElement[] {
                new ElementConverter.ConsumedElement(inputElement.CreateTag(), 0.05f)
            };
            elementConvert.outputElements = new ElementConverter.OutputElement[] {
                new ElementConverter.OutputElement(elementConvert.consumedElements[0].massConsumptionRate, outputElement, 0,
                    useEntityTemperature: true,
                    storeOutput: true,
                    addedDiseaseIdx: FusionReactor.RadiationIndex,
                    addedDiseaseCount: 10),
            };

            var requireConduitFuel = go.AddOrGet<RequireConduitInput>();
            requireConduitFuel.conduitInput = fuelInput;

            var requireConduitExhaust = go.AddOrGet<RequireConduitOutput>();
            requireConduitExhaust.conduitOutput = exhaustOutput;
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            GeneratedBuildings.RegisterLogicPorts(go, LogicOperationalController.INPUT_PORTS_0_1);
            go.AddOrGet<LogicOperationalController>();
            go.AddOrGetDef<PoweredActiveController.Def>();
        }


        [HarmonyPatch(typeof(GeneratedBuildings), nameof(GeneratedBuildings.LoadGeneratedBuildings))]
        public static class GeneratedBuildings_LoadGeneratedBuildings_Patch
        {
            public static void Prefix()
            {
                Buildings.AddToPlan(ID, "Utilities");
            }
        }

        [HarmonyPatch(typeof(Db), nameof(Db.Initialize))]
        public static class Db_Initialize_Patch
        {
            public static void Prefix()
            {
                Buildings.AddToTech(ID, "Catalytics");
            }
        }
    }
}
