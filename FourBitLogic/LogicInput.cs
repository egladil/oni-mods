using KSerialization;
using UnityEngine;

namespace Egladil
{
    [SerializationConfig(MemberSerialization.OptIn)]
    public class LogicInput : LogicBase, ISingleSliderControl
    {
        [MyCmpAdd]
        private CopyBuildingSettings copyBuildingSettings;

        [Serialize]
        private int value = 0;
        
        public int Value
        {
            get { return value; }
            set { this.value = value & 0xf; Update(); }
        }
        
        private static readonly EventSystem.IntraObjectHandler<LogicInput> OnCopySettingsDelegate = new EventSystem.IntraObjectHandler<LogicInput>((LogicInput component, object data) =>
        {
            if (component == null || component.gameObject == null || component.ports == null) return;
            component.OnCopySettings((GameObject)data);
        });

        public string SliderTitleKey => "STRINGS.UI.UISIDESCREENS.LOGIC_4BIT_INPUT_SIDE_SCREEN.TITLE";

        public string SliderUnits => "";

        public int SliderDecimalPlaces(int index) => 0;
        public float GetSliderMin(int index) => 0x0;
        public float GetSliderMax(int index) => 0xf;

        public float GetSliderValue(int index) => Value;
        public void SetSliderValue(float percent, int index) => Value = Mathf.RoundToInt(percent);

        public string GetSliderTooltipKey(int index) => "STRINGS.UI.UISIDESCREENS.LOGIC_4BIT_INPUT_SIDE_SCREEN.TOOLTIP";
        public string GetSliderTooltip() => string.Format(Strings.Get("STRINGS.UI.UISIDESCREENS.LOGIC_4BIT_INPUT_SIDE_SCREEN.TOOLTIP"), Value);

        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();
            Subscribe(-905833192, OnCopySettingsDelegate);
        }

        protected override void OnSpawn()
        {
            base.OnSpawn();
            Update();
        }

        private void OnCopySettings(GameObject data)
        {
            var source = data.GetComponent<LogicInput>();
            if (source == null) return;

            Value = source.Value;
        }

        private void Update()
        {
            KBatchedAnimController controller = GetComponent<KBatchedAnimController>();
            controller.Play($"{value}");

            ports.SendSignal(FourBitLogic.OUTPUT_1, value);
        }
    }
}
