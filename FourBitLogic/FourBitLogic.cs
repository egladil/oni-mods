using Harmony;

namespace Egladil
{
    public class FourBitLogic
    {
        public static readonly HashedString INPUT_1 = "Input1";
        public static readonly HashedString INPUT_2 = "Input2";
        public static readonly HashedString INPUT_3 = "Input3";
        public static readonly HashedString INPUT_4 = "Input4";

        public static readonly HashedString OUTPUT_1 = "Output1";
        public static readonly HashedString OUTPUT_2 = "Output2";
        public static readonly HashedString OUTPUT_3 = "Output3";
        public static readonly HashedString OUTPUT_4 = "Output4";

        public static readonly HashedString CARRY_IN = "CarryIn";
        public static readonly HashedString CARRY_OUT = "CarryOut";

        public static readonly HashedString GREATER_THAN = "GreaterThan";
        public static readonly HashedString LESS_THAN = "LessThan";

        [HarmonyPatch(typeof(GeneratedBuildings), nameof(GeneratedBuildings.LoadGeneratedBuildings))]
        public static class GeneratedBuildings_LoadGeneratedBuildings_Patch
        {
            public static void Prefix()
            {
                Buildings.AddToPlan(LogicInputConfig.ID, "Automation");
                Buildings.AddToPlan(LogicOutputConfig.ID, "Automation", after: LogicInputConfig.ID);
                Buildings.AddToPlan(LogicAdderConfig.ID, "Automation", after: LogicOutputConfig.ID);
                Buildings.AddToPlan(LogicComparatorConfig.ID, "Automation", after: LogicAdderConfig.ID);
            }
        }

        [HarmonyPatch(typeof(Db), nameof(Db.Initialize))]
        public static class Db_Initialize_Patch
        {
            public static void Prefix()
            {
                Buildings.AddToTech(LogicInputConfig.ID, "LogicCircuits");
                Buildings.AddToTech(LogicOutputConfig.ID, "LogicCircuits");
                Buildings.AddToTech(LogicAdderConfig.ID, "LogicCircuits");
                Buildings.AddToTech(LogicComparatorConfig.ID, "LogicCircuits");
            }
        }
    }
}
