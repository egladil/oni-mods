using Harmony;
using KSerialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Egladil
{
    [SerializationConfig(MemberSerialization.OptIn)]
    public class ComboSensor : KMonoBehaviour, ISim200ms
    {
        public static readonly HashedString PORT_ID = "ComboSensor";

        [MyCmpAdd]
        private CopyBuildingSettings copyBuildingSettings;

        [MyCmpGet]
        private LogicPorts ports;

        [SerializeField]
        public ConduitType conduitType;

        [SerializeField]
        public float targetTemperature = 273.15f;

        [SerializeField]
        public bool invertTemperature;

        [SerializeField]
        public float targetMass = 1000;

        [SerializeField]
        public bool invertMass;

        [SerializeField]
        public int targetGerms = 0;

        [SerializeField]
        public bool invertGerms;

        [SerializeField]
        public List<Tag> targetMaterial = new List<Tag>();

        [SerializeField]
        public bool invertMaterial;

        [Serialize]
        private int output;

        private static readonly EventSystem.IntraObjectHandler<ComboSensor> OnCopySettingsDelegate = new EventSystem.IntraObjectHandler<ComboSensor>((ComboSensor component, object data) =>
        {
            if (component == null || component.gameObject == null) return;
            component.OnCopySettings((GameObject)data);
        });
        
        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();
            Subscribe(-905833192, OnCopySettingsDelegate);
        }

        private void OnCopySettings(GameObject data)
        {
            var source = data.GetComponent<ComboSensor>();
            if (source == null) return;

            targetTemperature = source.targetTemperature;
            targetMass = source.targetMass;
            targetGerms = source.targetGerms;

            targetMaterial.Clear();
            targetMaterial.AddRange(source.targetMaterial);
        }

        private void GetWorldData(out float temperature, out float mass, out int germs, out Element element)
        {
            int cell = Grid.PosToCell(this);

            temperature = Grid.Temperature[cell];
            mass = Grid.Mass[cell];
            germs = Grid.DiseaseCount[cell];
            element = Grid.Element[cell];
        }

        private void GetConduitData(out float temperature, out float mass, out int germs, out Element element)
        {
            int cell = Grid.PosToCell(this);
            var content = Conduit.GetFlowManager(conduitType).GetContents(cell);

            temperature = content.temperature;
            mass = content.mass;
            germs = content.diseaseCount;
            element = ElementLoader.FindElementByHash(content.element);
        }

        private void GetSolidConduitData(out float temperature, out float mass, out int germs, out Element element)
        {
            int cell = Grid.PosToCell(this);
            var manager = SolidConduit.GetFlowManager();
            var content = manager.GetPickupable(manager.GetContents(cell).pickupableHandle);

            if (content == null)
            {
                temperature = 0;
                mass = 0;
                germs = 0;
                element = ElementLoader.FindElementByHash(SimHashes.Vacuum);
            }
            else
            {
                temperature = content.PrimaryElement.Temperature;
                mass = content.PrimaryElement.Mass;
                germs = content.PrimaryElement.DiseaseCount;
                element = content.PrimaryElement.Element;
            }
        }

        protected virtual void GetSpecialData(out float temperature, out float mass, out int germs, out Element element)
        {
            temperature = 0;
            mass = 0;
            germs = 0;
            element = ElementLoader.FindElementByHash(SimHashes.Vacuum);
        }

        public void GetData(out float temperature, out float mass, out int germs, out Element element)
        {
            switch (conduitType)
            {
                case ConduitType.None:
                    GetWorldData(out temperature, out mass, out germs, out element);
                    break;
                case ConduitType.Gas:
                case ConduitType.Liquid:
                    GetConduitData(out temperature, out mass, out germs, out element);
                    break;
                case ConduitType.Solid:
                    GetSolidConduitData(out temperature, out mass, out germs, out element);
                    break;
                default:
                    GetSpecialData(out temperature, out mass, out germs, out element);
                    break;
            }
        }

        public void Sim200ms(float dt)
        {
            GetData(out float temperature, out float mass, out int germs, out Element element);

            bool temperatureInRange = invertTemperature ? temperature > targetTemperature : temperature < targetTemperature;
            bool massInRange = invertMass ? mass > targetMass : mass < targetMass;
            bool germsInRange = invertGerms ? germs > targetGerms : germs < targetGerms;
            bool elementInRage = invertMaterial != targetMaterial.Contains(element.tag);

            int newOutput = (temperatureInRange ? 1 : 0) | (massInRange ? 2 : 0) | (germsInRange ? 4 : 0) | (elementInRage ? 8 : 0);
            if (newOutput != output)
            {
                output = newOutput;
                ports.SendSignal(PORT_ID, output);
            }
        }


        [HarmonyPatch(typeof(GeneratedBuildings), nameof(GeneratedBuildings.LoadGeneratedBuildings))]
        public static class GeneratedBuildings_LoadGeneratedBuildings_Patch
        {
            public static void Prefix()
            {
                Buildings.AddToPlan(LogicComboSensorConfig.ID, "Automation");
                //Buildings.AddToPlan(LogicOutputConfig.ID, "Automation", after: LogicComboSensorConfig.ID);
            }
        }

        [HarmonyPatch(typeof(Db), nameof(Db.Initialize))]
        public static class Db_Initialize_Patch
        {
            public static void Prefix()
            {
                Buildings.AddToTech(LogicComboSensorConfig.ID, "LogicCircuits");
            }
        }
    }
}
