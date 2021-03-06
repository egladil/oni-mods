﻿using System.Collections.Generic;
using UnityEngine;

namespace Egladil
{
    public class LogicAdderConfig : IBuildingConfig
    {
        public static string ID = "Logic4bitAdder";

        public override BuildingDef CreateBuildingDef()
        {
            BuildingDef def = BuildingTemplates.CreateBuildingDef(ID, 2, 3, "logic_4bit_adder_kanim", 10, 30f, TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER0, TUNING.MATERIALS.REFINED_METALS, 1600f, BuildLocationRule.Anywhere, TUNING.BUILDINGS.DECOR.NONE, TUNING.NOISE_POLLUTION.NONE);
            def.Deprecated = false;
            def.Overheatable = false;
            def.Floodable = false;
            def.Entombable = false;
            def.PermittedRotations = PermittedRotations.R360;
            def.ViewMode = OverlayModes.Logic.ID;
            def.AudioCategory = "Metal";
            def.SceneLayer = Grid.SceneLayer.LogicGates;
            def.ObjectLayer = ObjectLayer.LogicGate;
            def.AlwaysOperational = true;

            def.LogicInputPorts = new List<LogicPorts.Port>
            {
                LogicPorts.Port.InputPort(FourBitLogic.CARRY_IN, new CellOffset(0, 0), Strings.Get("STRINGS.UI.LOGIC_PORTS.CARRY_IN_NAME"), Strings.Get("STRINGS.UI.LOGIC_PORTS.CARRY_IN_ACTIVE"), Strings.Get("STRINGS.UI.LOGIC_PORTS.CARRY_IN_INACTIVE"), show_wire_missing_icon: false, display_custom_name: true),
                LogicPorts.Port.RibbonInputPort(FourBitLogic.INPUT_1, new CellOffset(0, 1), STRINGS.UI.LOGIC_PORTS.GATE_MULTI_INPUT_ONE_NAME, STRINGS.UI.LOGIC_PORTS.GATE_MULTI_INPUT_ONE_ACTIVE, STRINGS.UI.LOGIC_PORTS.GATE_MULTI_INPUT_ONE_INACTIVE, show_wire_missing_icon: true, display_custom_name: true),
                LogicPorts.Port.RibbonInputPort(FourBitLogic.INPUT_2, new CellOffset(0, 2), STRINGS.UI.LOGIC_PORTS.GATE_MULTI_INPUT_TWO_NAME, STRINGS.UI.LOGIC_PORTS.GATE_MULTI_INPUT_TWO_ACTIVE, STRINGS.UI.LOGIC_PORTS.GATE_MULTI_INPUT_TWO_INACTIVE, show_wire_missing_icon: true, display_custom_name: true),
            };

            def.LogicOutputPorts = new List<LogicPorts.Port>
            {
                LogicPorts.Port.RibbonOutputPort(FourBitLogic.OUTPUT_1, new CellOffset(1, 1), STRINGS.UI.LOGIC_PORTS.GATE_SINGLE_OUTPUT_ONE_NAME, STRINGS.UI.LOGIC_PORTS.GATE_SINGLE_OUTPUT_ONE_ACTIVE, STRINGS.UI.LOGIC_PORTS.GATE_SINGLE_OUTPUT_ONE_INACTIVE, show_wire_missing_icon: true, display_custom_name: true),
                LogicPorts.Port.OutputPort(FourBitLogic.CARRY_OUT, new CellOffset(1, 2), Strings.Get("STRINGS.UI.LOGIC_PORTS.CARRY_OUT_NAME"), Strings.Get("STRINGS.UI.LOGIC_PORTS.CARRY_OUT_ACTIVE"), Strings.Get("STRINGS.UI.LOGIC_PORTS.CARRY_OUT_INACTIVE"), show_wire_missing_icon: false, display_custom_name: true),
            };

            GeneratedBuildings.RegisterWithOverlay(OverlayModes.Logic.HighlightItemIDs, ID);
            return def;
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            go.AddOrGet<LogicAdder>();
        }
    }
}
