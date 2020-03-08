using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Egladil
{
    class ComboSensorSideScreen : SideScreenContent, IRender200ms
    {
        private ComboSensor sensor;

        [SerializeField]
        private LocText temperatureCurrent;

        [SerializeField]
        private KToggle temperatureBelow;

        [SerializeField]
        private KToggle temperatureAbove;

        [SerializeField]
        private NonLinearSlider temperatureSlider;

        [SerializeField]
        private KNumberInputField temperatureInput;

        [SerializeField]
        private LocText massCurrent;

        [SerializeField]
        private KToggle massBelow;

        [SerializeField]
        private KToggle massAbove;

        [SerializeField]
        private NonLinearSlider massSlider;

        [SerializeField]
        private KNumberInputField massInput;

        [SerializeField]
        private LocText germsCurrent;

        [SerializeField]
        private KToggle germsBelow;

        [SerializeField]
        private KToggle germsAbove;

        [SerializeField]
        private NonLinearSlider germsSlider;

        [SerializeField]
        private KNumberInputField germsInput;

        [SerializeField]
        private LocText materialCurrent;

        [SerializeField]
        private KToggle materialChecked;

        [SerializeField]
        private KToggle materialUnchecked;


        public override bool IsValidForTarget(GameObject target) => target.GetComponent<ComboSensor>() != null;

        protected override void OnPrefabInit()
        {
            Log.Spam($"OnPrefabInit");

            base.OnPrefabInit();

            titleKey = "STRINGS.UI.UISIDESCREENS.COMBO_SENSOR_SIDESCREEN.TITLE";
            
            var sideScreens = Traverse.Create(DetailsScreen.Instance).Field<List<DetailsScreen.SideScreenRef>>("sideScreens").Value;
            if (sideScreens == null)
            {
                Log.Error("No sideScreens");
                return;
            }
            
            Traverse thresholdSwitchSideScreen = null;
            foreach (var sideScreen in sideScreens)
            {
                var component = sideScreen.screenPrefab?.GetComponent<ThresholdSwitchSideScreen>();
                if (component != null)
                {
                    thresholdSwitchSideScreen = Traverse.Create(component);
                    break;
                }
            }
            if (thresholdSwitchSideScreen == null)
            {
                Log.Error("No ThresholdSwitchSideScreen");
                return;
            }
            
            var locText = thresholdSwitchSideScreen.Field<LocText>("currentValue").Value;
            var aboveToggle = thresholdSwitchSideScreen.Field<KToggle>("aboveToggle").Value;
            var belowToggle = thresholdSwitchSideScreen.Field<KToggle>("belowToggle").Value;
            var thresholdSlider = thresholdSwitchSideScreen.Field<NonLinearSlider>("thresholdSlider").Value;
            var numberInput = thresholdSwitchSideScreen.Field<KNumberInputField>("numberInput").Value;

            var dummyPanel = Gui.CreatePanel();

            var mainPanel = Gui.CreatePanel();
            mainPanel.FindOrAddComponent<VerticalLayoutGroup>().padding = new RectOffset(5, 5, 5, 5);

            {
                var groupPanel = Gui.CreatePanel(parent: mainPanel);
                groupPanel.GetComponent<Image>().color = new Color(0, 0, 0, 0.1f);
                groupPanel.FindOrAddComponent<VerticalLayoutGroup>().padding = new RectOffset(5, 5, 5, 5);

                temperatureCurrent = Gui.Create<LocText>(prefab: locText.gameObject, parent: groupPanel);
                temperatureCurrent.FindOrAddComponent<LayoutElement>().minHeight = 20;

                var buttonPanel = Gui.CreatePanel(parent: groupPanel);
                buttonPanel.FindOrAddComponent<HorizontalLayoutGroup>().padding = new RectOffset(5, 5, 5, 5);
                temperatureBelow = Gui.Create<KToggle>(prefab: belowToggle.gameObject, parent: buttonPanel);
                temperatureAbove = Gui.Create<KToggle>(prefab: aboveToggle.gameObject, parent: buttonPanel);

                temperatureSlider = Gui.Create<NonLinearSlider>(prefab: thresholdSlider.gameObject, parent: groupPanel);

                temperatureInput = Gui.Create<KNumberInputField>(prefab: numberInput.gameObject, parent: groupPanel);
                temperatureInput.FindOrAddComponent<LayoutElement>().minHeight = 30;
                temperatureInput.transform.Find("UnitsLabel")?.DeleteObject();
            }

            Gui.CreatePanel(parent: mainPanel).FindOrAddComponent<LayoutElement>().minHeight = 5;

            {
                var groupPanel = Gui.CreatePanel(parent: mainPanel);
                groupPanel.GetComponent<Image>().color = new Color(0, 0, 0, 0.1f);
                groupPanel.FindOrAddComponent<VerticalLayoutGroup>().padding = new RectOffset(5, 5, 5, 5);

                massCurrent = Gui.Create<LocText>(prefab: locText.gameObject, parent: groupPanel);
                massCurrent.FindOrAddComponent<LayoutElement>().minHeight = 20;

                var buttonPanel = Gui.CreatePanel(parent: groupPanel);
                buttonPanel.FindOrAddComponent<HorizontalLayoutGroup>().padding = new RectOffset(5, 5, 5, 5);
                massBelow = Gui.Create<KToggle>(prefab: belowToggle.gameObject, parent: buttonPanel);
                massAbove = Gui.Create<KToggle>(prefab: aboveToggle.gameObject, parent: buttonPanel);

                massSlider = Gui.Create<NonLinearSlider>(prefab: thresholdSlider.gameObject, parent: groupPanel);

                massInput = Gui.Create<KNumberInputField>(prefab: numberInput.gameObject, parent: groupPanel);
                massInput.FindOrAddComponent<LayoutElement>().minHeight = 30;
                massInput.transform.Find("UnitsLabel")?.DeleteObject();
            }

            Gui.CreatePanel(parent: mainPanel).FindOrAddComponent<LayoutElement>().minHeight = 5;

            {
                var groupPanel = Gui.CreatePanel(parent: mainPanel);
                groupPanel.GetComponent<Image>().color = new Color(0, 0, 0, 0.1f);
                groupPanel.FindOrAddComponent<VerticalLayoutGroup>().padding = new RectOffset(5, 5, 5, 5);

                germsCurrent = Gui.Create<LocText>(prefab: locText.gameObject, parent: groupPanel);
                germsCurrent.FindOrAddComponent<LayoutElement>().minHeight = 20;

                var buttonPanel = Gui.CreatePanel(parent: groupPanel);
                buttonPanel.FindOrAddComponent<HorizontalLayoutGroup>().padding = new RectOffset(5, 5, 5, 5);
                germsBelow = Gui.Create<KToggle>(prefab: belowToggle.gameObject, parent: buttonPanel);
                germsAbove = Gui.Create<KToggle>(prefab: aboveToggle.gameObject, parent: buttonPanel);

                germsSlider = Gui.Create<NonLinearSlider>(prefab: thresholdSlider.gameObject, parent: groupPanel);

                germsInput = Gui.Create<KNumberInputField>(prefab: numberInput.gameObject, parent: groupPanel);
                germsInput.FindOrAddComponent<LayoutElement>().minHeight = 30;
                germsInput.transform.Find("UnitsLabel")?.DeleteObject();

                DumpTree(germsInput.gameObject);
            }

            Gui.CreatePanel(parent: mainPanel).FindOrAddComponent<LayoutElement>().minHeight = 5;

            {
                var groupPanel = Gui.CreatePanel(parent: mainPanel);
                groupPanel.GetComponent<Image>().color = new Color(0, 0, 0, 0.1f);
                groupPanel.FindOrAddComponent<VerticalLayoutGroup>().padding = new RectOffset(5, 5, 5, 5);

                materialCurrent = Gui.Create<LocText>(prefab: locText.gameObject, parent: groupPanel);
                materialCurrent.FindOrAddComponent<LayoutElement>().minHeight = 20;

                var buttonPanel = Gui.CreatePanel(parent: groupPanel);
                buttonPanel.FindOrAddComponent<HorizontalLayoutGroup>().padding = new RectOffset(5, 5, 5, 5);
                materialChecked = Gui.Create<KToggle>(prefab: belowToggle.gameObject, parent: buttonPanel);
                materialUnchecked = Gui.Create<KToggle>(prefab: aboveToggle.gameObject, parent: buttonPanel);
            }

            gameObject.FindOrAddComponent<VerticalLayoutGroup>().padding = new RectOffset(5, 5, 5, 5);
            LayoutElement layoutElement = gameObject.FindOrAddComponent<LayoutElement>();
            layoutElement.minWidth = 150;
            layoutElement.preferredWidth = 150;
            layoutElement.minHeight = 450;
            layoutElement.preferredHeight = 450;

            mainPanel.transform.SetParent(gameObject.transform);
            ContentContainer = mainPanel;
        }

        void DumpTree(GameObject go, string indent = "")
        {
            Log.Info($"{indent}{go.name}");
            foreach (var cmp in go.GetComponents<Component>())
            {
                Log.Info($"{indent}-{cmp.GetType().Name}");
                foreach(var field in cmp.GetType().GetProperties(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static))
                {
                    Log.Info($"{indent}+ {field.PropertyType.Name} {field.Name} = {field.GetValue(cmp)}");
                }
            }
            for (int i = 0; i < go.transform.childCount; i++)
            {
                DumpTree(go.transform.GetChild(i).gameObject, indent + " ");
            }
        }

        protected override void OnSpawn()
        {
            Log.Info($"OnSpawn");

            base.OnSpawn();

            Log.Info($"{germsInput.displayName}");
            Log.Info($"{germsInput.screenName}");

            temperatureBelow.onClick += () => { if (sensor != null) { sensor.invertTemperature = false; }  UpdateInputs(); };
            temperatureAbove.onClick += () => { if (sensor != null) { sensor.invertTemperature = true; }  UpdateInputs(); };
            massBelow.onClick += () => { if (sensor != null) { sensor.invertMass = false; } UpdateInputs(); };
            massAbove.onClick += () => { if (sensor != null) { sensor.invertMass = true; } UpdateInputs(); };
            germsBelow.onClick += () => { if (sensor != null) { sensor.invertGerms = false; } UpdateInputs(); };
            germsAbove.onClick += () => { if (sensor != null) { sensor.invertGerms = true; } UpdateInputs(); };
            materialChecked.onClick += () => { if (sensor != null) { sensor.invertMaterial = false; } UpdateInputs(); };
            materialUnchecked.onClick += () => { if (sensor != null) { sensor.invertMaterial = true; } UpdateInputs(); };

            temperatureBelow.transform.GetChild(0).GetComponent<LocText>().SetText(STRINGS.UI.UISIDESCREENS.TEMPERATURESWITCHSIDESCREEN.COLDER_BUTTON);
            temperatureAbove.transform.GetChild(0).GetComponent<LocText>().SetText(STRINGS.UI.UISIDESCREENS.TEMPERATURESWITCHSIDESCREEN.WARMER_BUTTON);
            massBelow.transform.GetChild(0).GetComponent<LocText>().SetText(STRINGS.UI.UISIDESCREENS.THRESHOLD_SWITCH_SIDESCREEN.BELOW_BUTTON);
            massAbove.transform.GetChild(0).GetComponent<LocText>().SetText(STRINGS.UI.UISIDESCREENS.THRESHOLD_SWITCH_SIDESCREEN.ABOVE_BUTTON);
            germsBelow.transform.GetChild(0).GetComponent<LocText>().SetText(STRINGS.UI.UISIDESCREENS.THRESHOLD_SWITCH_SIDESCREEN.BELOW_BUTTON);
            germsAbove.transform.GetChild(0).GetComponent<LocText>().SetText(STRINGS.UI.UISIDESCREENS.THRESHOLD_SWITCH_SIDESCREEN.ABOVE_BUTTON);
            materialChecked.transform.GetChild(0).GetComponent<LocText>().SetText(STRINGS.UI.UISIDESCREENS.THRESHOLD_SWITCH_SIDESCREEN.BELOW_BUTTON);
            materialUnchecked.transform.GetChild(0).GetComponent<LocText>().SetText(STRINGS.UI.UISIDESCREENS.THRESHOLD_SWITCH_SIDESCREEN.ABOVE_BUTTON);

            SetupInputs(temperatureSlider, temperatureInput,
                new NonLinearSlider.Range[] { new NonLinearSlider.Range(25, 260), new NonLinearSlider.Range(50, 400), new NonLinearSlider.Range(12, 1500), new NonLinearSlider.Range(13, 10000) },
                GameUtil.GetConvertedTemperature(0), GameUtil.GetConvertedTemperature(10000), 1,
                OnUpdateTemperature, x => GameUtil.GetTemperatureConvertedToKelvin(Mathf.Round(GameUtil.GetTemperatureConvertedFromKelvin(x, GameUtil.temperatureUnit) * 10) / 10), GameUtil.GetTemperatureConvertedToKelvin);

            SetupInputs(massSlider, massInput,
                new NonLinearSlider.Range[] { new NonLinearSlider.Range(12, 0.1f), new NonLinearSlider.Range(13, 1), new NonLinearSlider.Range(25, 10), new NonLinearSlider.Range(50, 2000) },
                0, 2000, 3,
                OnUpdateMass, x => Mathf.Round(x * 1000) / 1000, x => x);

            SetupInputs(germsSlider, germsInput,
                NonLinearSlider.GetDefaultRange(100000),
                0, 1000000, 0,
                OnUpdateGerms, x => x, x => x);

            UpdateInputs();
            UpdateLabels();
        }

        private static void SetupInputs(NonLinearSlider slider, KNumberInputField input, NonLinearSlider.Range[] sliderRanges, float inputMin, float inputMax, int inputDecimals, Action<float> onUpdate, Func<float, float> convertSlider, Func<float, float> convertInput)
        {
            slider.onDrag += () => onUpdate(convertSlider(slider.GetValueForPercentage(slider.value)));
            slider.onPointerDown += () => onUpdate(convertSlider(slider.GetValueForPercentage(slider.value)));
            slider.onMove += () => onUpdate(convertSlider(slider.GetValueForPercentage(slider.value)));
            slider.minValue = 0;
            slider.maxValue = 100;
            slider.SetRanges(sliderRanges);

            input.onEndEdit += () => onUpdate(convertInput(input.currentValue));
            input.decimalPlaces = inputDecimals;
            input.minValue = inputMin;
            input.maxValue = inputMax;
            input.Activate();
        }

        public override void SetTarget(GameObject target)
        {
            sensor = target?.GetComponent<ComboSensor>();
            if (sensor == null)
            {
                Log.Error("Invalid target");
                return;
            }

            UpdateInputs();
            UpdateLabels();
        }

        private void OnUpdateTemperature(float input)
        {
            float kelvin = Mathf.Clamp(input, 0, 9999);
            Log.Info($"Update Temperature: {input} ({kelvin})");

            if (sensor != null)
            {
                sensor.targetTemperature = kelvin;
            }

            UpdateInputs();
        }

        private void OnUpdateMass(float input)
        {
            float kg = Mathf.Clamp(input, 0, 20000);
            Log.Info($"Update Mass: {input} ({kg})");

            if (sensor != null)
            {
                sensor.targetMass = kg;
            }

            UpdateInputs();
        }

        private void OnUpdateGerms(float input)
        {
            int count = Mathf.Clamp(Mathf.RoundToInt(input), 0, 1000000);
            Log.Info($"Update Germs: {input} ({count})");

            if (sensor != null)
            {
                sensor.targetGerms = count;
            }

            UpdateInputs();
        }

        public void Render200ms(float dt)
        {
            UpdateLabels();
        }

        private static void UpdateToggle(KToggle off, KToggle on, bool isOn)
        {
            off.isOn = !isOn;
            on.isOn = isOn;
            //off.GetComponent<ImageToggleState>().SetState(off.isOn ? ImageToggleState.State.Active : ImageToggleState.State.Inactive);
            //on.GetComponent<ImageToggleState>().SetState(on.isOn ? ImageToggleState.State.Active : ImageToggleState.State.Inactive);
        }

        private void UpdateInputs()
        {
            if (sensor == null || ContentContainer == null) return;

            UpdateToggle(temperatureBelow, temperatureAbove, sensor.invertTemperature);
            UpdateToggle(massBelow, massAbove, sensor.invertMass);
            UpdateToggle(germsBelow, germsAbove, sensor.invertGerms);
            UpdateToggle(materialChecked, materialUnchecked, sensor.invertMaterial);

            temperatureSlider.value = temperatureSlider.GetPercentageFromValue(sensor.targetTemperature);
            temperatureInput.SetDisplayValue(GameUtil.GetTemperatureConvertedFromKelvin(sensor.targetTemperature, GameUtil.temperatureUnit).ToString("0.#"));

            massSlider.value = massSlider.GetPercentageFromValue(sensor.targetMass);
            massInput.SetDisplayValue(sensor.targetMass.ToString("0.###"));

            germsSlider.value = germsSlider.GetPercentageFromValue(sensor.targetGerms);
            germsInput.SetDisplayValue(sensor.targetGerms.ToString());
        }

        private void UpdateLabels()
        {
            if (sensor == null || ContentContainer == null) return;

            sensor.GetData(out float temperature, out float mass, out int germs, out Element element);
            
            temperatureCurrent.text = GameUtil.GetFormattedTemperature(temperature);
            massCurrent.text = GameUtil.GetFormattedMass(mass, massFormat: GameUtil.MetricMassFormat.Kilogram, floatFormat: "{0:0.###}");
            germsCurrent.text = GameUtil.GetFormattedDiseaseAmount(germs);
            materialCurrent.text = element.name;
        }

        [HarmonyPatch(typeof(DetailsScreen), "OnPrefabInit")]
        public static class DetailsScreen_OnPrefabInit_Patch
        {
            public static void Postfix(GameObject ___sideScreenContentBody, List<DetailsScreen.SideScreenRef> ___sideScreens)
            {
                var sideScreen = Gui.Create<ComboSensorSideScreen>(parent: ___sideScreenContentBody);

                var sideScreenRef = new DetailsScreen.SideScreenRef();
                sideScreenRef.name = typeof(ComboSensorSideScreen).Name;
                sideScreenRef.screenPrefab = sideScreen;
                sideScreenRef.screenInstance = sideScreen;

                ___sideScreens.Add(sideScreenRef);
            }
        }
    }
}
