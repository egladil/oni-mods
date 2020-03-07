using System.Collections.Generic;
using UnityEngine;

namespace Egladil
{
    class LogicOutputConfig : IBuildingConfig
    {
        public static string ID = "Logic4bitOutput";

        public override BuildingDef CreateBuildingDef()
        {
            BuildingDef def = BuildingTemplates.CreateBuildingDef(ID, 1, 1, "logic_4bit_output_kanim", 10, 30f, TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER0, TUNING.MATERIALS.REFINED_METALS, 1600f, BuildLocationRule.Anywhere, TUNING.BUILDINGS.DECOR.NONE, TUNING.NOISE_POLLUTION.NONE);
            def.Deprecated = false;
            def.Overheatable = false;
            def.Floodable = false;
            def.Entombable = false;
            def.PermittedRotations = PermittedRotations.R360;
            def.ViewMode = OverlayModes.Logic.ID;
            def.AudioCategory = "Metal";
            def.SceneLayer = Grid.SceneLayer.LogicGates;
            def.ObjectLayer = ObjectLayer.LogicGates;
            def.AlwaysOperational = true;

            def.LogicInputPorts = new List<LogicPorts.Port>
            {
                LogicPorts.Port.RibbonInputPort(FourBitLogic.INPUT_1, new CellOffset(0, 0), STRINGS.UI.LOGIC_PORTS.GATE_SINGLE_INPUT_ONE_NAME, STRINGS.UI.LOGIC_PORTS.GATE_SINGLE_INPUT_ONE_ACTIVE, STRINGS.UI.LOGIC_PORTS.GATE_SINGLE_INPUT_ONE_INACTIVE, show_wire_missing_icon: true, display_custom_name: true),
            };
            
            GeneratedBuildings.RegisterWithOverlay(OverlayModes.Logic.HighlightItemIDs, ID);
            return def;
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            go.AddOrGet<LogicOutput>();
        }
    }
}
