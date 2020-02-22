using KSerialization;

namespace Egladil
{
    [SerializationConfig(MemberSerialization.OptIn)]
    public class LogicAdder : LogicBase
    {
        [Serialize]
        private int output;

        [Serialize]
        private bool carryOut;
        
        protected override void OnLogicValueChanged(LogicValueChanged data)
        {
            if (data.portID == FourBitLogic.CARRY_OUT || data.portID == FourBitLogic.OUTPUT_1) return;

            int carryIn = ports.GetInputValue(FourBitLogic.CARRY_IN) & 0x1;
            int input1 = ports.GetInputValue(FourBitLogic.INPUT_1) & 0xf;
            int input2 = ports.GetInputValue(FourBitLogic.INPUT_2) & 0xf;

            int newOutput = carryIn + input1 + input2;
            bool newCarryOut = newOutput > 0xf;
            newOutput &= 0xf;

            if (newOutput != output)
            {
                output = newOutput;
                ports.SendSignal(FourBitLogic.OUTPUT_1, output);
            }

            if (newCarryOut != carryOut)
            {
                carryOut = newCarryOut;
                ports.SendSignal(FourBitLogic.CARRY_OUT, carryOut ? 1 : 0);
            }

            KBatchedAnimController kbac = GetComponent<KBatchedAnimController>();
            if (kbac != null)
            {
                int value = carryIn | (carryOut ? 2 : 0);
                kbac.Play($"on_{value}");
            }
        }
    }
}
