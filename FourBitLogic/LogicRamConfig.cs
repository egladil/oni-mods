using System.Collections.Generic;
using UnityEngine;

namespace Egladil
{
    public class LogicRamConfig : IBuildingConfig
    {
        public static string ID = "Logic4bitMemory";

        public override BuildingDef CreateBuildingDef()
        {
            BuildingDef def = BuildingTemplates.CreateBuildingDef(ID, 2, 3, "logic_4bit_memory_kanim", 10, 30f, TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER0, TUNING.MATERIALS.REFINED_METALS, 1600f, BuildLocationRule.Anywhere, TUNING.BUILDINGS.DECOR.NONE, TUNING.NOISE_POLLUTION.NONE);
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
                LogicPorts.Port.InputPort(FourBitLogic.READ, new CellOffset(0, 0), Strings.Get("STRINGS.UI.LOGIC_PORTS.READ_NAME"), Strings.Get("STRINGS.UI.LOGIC_PORTS.READ_ACTIVE"), Strings.Get("STRINGS.UI.LOGIC_PORTS.READ_INACTIVE"), show_wire_missing_icon: true, display_custom_name: true),
                LogicPorts.Port.InputPort(FourBitLogic.WRITE, new CellOffset(1, 0), Strings.Get("STRINGS.UI.LOGIC_PORTS.WRITE_NAME"), Strings.Get("STRINGS.UI.LOGIC_PORTS.WRITE_ACTIVE"), Strings.Get("STRINGS.UI.LOGIC_PORTS.WRITE_INACTIVE"), show_wire_missing_icon: true, display_custom_name: true),
                LogicPorts.Port.RibbonInputPort(FourBitLogic.ADDRESS_1, new CellOffset(0, 1), Strings.Get("STRINGS.UI.LOGIC_PORTS.ADDRESS_1_NAME"), Strings.Get("STRINGS.UI.LOGIC_PORTS.ADDRESS_1_ACTIVE"), Strings.Get("STRINGS.UI.LOGIC_PORTS.ADDRESS_1_INACTIVE"), show_wire_missing_icon: true, display_custom_name: true),
                LogicPorts.Port.RibbonInputPort(FourBitLogic.ADDRESS_2, new CellOffset(0, 2), Strings.Get("STRINGS.UI.LOGIC_PORTS.ADDRESS_2_NAME"), Strings.Get("STRINGS.UI.LOGIC_PORTS.ADDRESS_2_ACTIVE"), Strings.Get("STRINGS.UI.LOGIC_PORTS.ADDRESS_2_INACTIVE"), show_wire_missing_icon: false, display_custom_name: true),
            };

            def.LogicOutputPorts = new List<LogicPorts.Port>
            {
                LogicPorts.Port.RibbonOutputPort(FourBitLogic.DATA, new CellOffset(1, 2), Strings.Get("STRINGS.UI.LOGIC_PORTS.DATA_NAME"), Strings.Get("STRINGS.UI.LOGIC_PORTS.DATA_ACTIVE"), Strings.Get("STRINGS.UI.LOGIC_PORTS.DATA_INACTIVE"), show_wire_missing_icon: true, display_custom_name: true),
            };

            GeneratedBuildings.RegisterWithOverlay(OverlayModes.Logic.HighlightItemIDs, ID);
            return def;
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            go.AddOrGet<LogicRam>();
        }
    }
}
