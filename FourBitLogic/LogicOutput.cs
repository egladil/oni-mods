using KSerialization;
using UnityEngine;

namespace Egladil
{
    [SerializationConfig(MemberSerialization.OptIn)]
    public class LogicOutput : LogicBase
    {
        protected override void OnLogicValueChanged(LogicValueChanged data)
        {
            int value = ports.GetInputValue(FourBitLogic.INPUT_1) & 0xf;

            KBatchedAnimController controller = GetComponent<KBatchedAnimController>();
            controller.Play($"on_{value}");
        }
    }
}
