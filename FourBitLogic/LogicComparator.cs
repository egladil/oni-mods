using KSerialization;

namespace Egladil
{
    [SerializationConfig(MemberSerialization.OptIn)]
    public class LogicComparator : LogicBase
    {
        [Serialize]
        private bool greaterThan;

        [Serialize]
        private bool lessThan;

        protected override void OnLogicValueChanged(LogicValueChanged data)
        {
            if (data.portID == FourBitLogic.GREATER_THAN || data.portID == FourBitLogic.LESS_THAN) return;

            int input1 = ports.GetInputValue(FourBitLogic.INPUT_1) & 0xf;
            int input2 = ports.GetInputValue(FourBitLogic.INPUT_2) & 0xf;

            bool newGreaterThan = input1 > input2;
            bool newLessThan = input1 < input2;

            if (newGreaterThan != greaterThan)
            {
                greaterThan = newGreaterThan;
                ports.SendSignal(FourBitLogic.GREATER_THAN, greaterThan ? 1 : 0);
            }

            if (newLessThan != lessThan)
            {
                lessThan = newLessThan;
                ports.SendSignal(FourBitLogic.LESS_THAN, lessThan ? 1 : 0);
            }

            KBatchedAnimController kbac = GetComponent<KBatchedAnimController>();
            if (kbac != null)
            {
                int value = (greaterThan ? 1 : 0) | (lessThan ? 2 : 0);
                kbac.Play($"on_{value}");
            }
        }
    }
}
