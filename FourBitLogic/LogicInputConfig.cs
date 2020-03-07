using System.Collections.Generic;
using UnityEngine;

namespace Egladil
{
    class LogicInputConfig : IBuildingConfig
    {
        public static string ID = "Logic4bitInput";

        public override BuildingDef CreateBuildingDef()
        {
            BuildingDef def = BuildingTemplates.CreateBuildingDef(ID, 1, 1, "logic_4bit_input_kanim", 10, 30f, TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER0, TUNING.MATERIALS.REFINED_METALS, 1600f, BuildLocationRule.Anywhere, TUNING.BUILDINGS.DECOR.NONE, TUNING.NOISE_POLLUTION.NONE);
            def.Deprecated = false;
            def.Overheatable = false;
            def.Floodable = false;
            def.Entombable = false;
            def.ViewMode = OverlayModes.Logic.ID;
            def.AudioCategory = "Metal";
            def.SceneLayer = Grid.SceneLayer.LogicGates;
            def.ObjectLayer = ObjectLayer.LogicGates;
            def.AlwaysOperational = true;

            def.LogicOutputPorts = new List<LogicPorts.Port>
            {
                LogicPorts.Port.RibbonOutputPort(FourBitLogic.OUTPUT_1, new CellOffset(0, 0), STRINGS.UI.LOGIC_PORTS.GATE_SINGLE_OUTPUT_ONE_NAME, STRINGS.UI.LOGIC_PORTS.GATE_SINGLE_OUTPUT_ONE_ACTIVE, STRINGS.UI.LOGIC_PORTS.GATE_SINGLE_OUTPUT_ONE_INACTIVE, show_wire_missing_icon: true, display_custom_name: true),
            };

            SoundEventVolumeCache.instance.AddVolume("switchpower_kanim", "PowerSwitch_on", TUNING.NOISE_POLLUTION.NOISY.TIER3);
            SoundEventVolumeCache.instance.AddVolume("switchpower_kanim", "PowerSwitch_off", TUNING.NOISE_POLLUTION.NOISY.TIER3);
            GeneratedBuildings.RegisterWithOverlay(OverlayModes.Logic.HighlightItemIDs, ID);
            return def;
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            go.AddOrGet<LogicInput>();
        }
    }
}
