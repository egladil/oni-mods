using Harmony;
using Klei.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Egladil
{
    public class FusionReactor : KMonoBehaviour, ISaveLoadable, ISim200ms
    {
        private static readonly HashedString radiationId = "Radiation";

        public static byte RadiationIndex { get; private set; }
        public static Disease RadiationInfo => Db.Get().Diseases[RadiationIndex];

        public static void InitRadiation()
        {
            byte index = Db.Get().Diseases.GetIndex(radiationId);
            if (index == byte.MaxValue)
            {
                Log.Error("Could not find radiation index. Make sure to load egladil's Radiation mod or another mod that provides a disease named 'Radiation'");
            }
            else
            {
                RadiationIndex = index;
            }
        }

        [SerializeField]
        public float producedHeatPerKg;

        [SerializeField]
        public float producedRadiationPerKg;

        [SerializeField]
        public float minimumCoolantMass = 100;

        [MyCmpReq]
        private Operational operational;

        [MyCmpReq]
        protected KSelectable selectable;

        [SerializeField]
        public Storage coolantStorage;

        [SerializeField]
        public Storage fusionStorage;

        [MyCmpReq]
        public ElementConverter elementConverter;

        private Guid needsFuelStatusItemGuid;
        private Guid needsCoolantStatusItemGuid;
        private bool hasFuel;
        private bool hasCoolant;

        private HandleVector<int>.Handle heatAccumulator;
        private HandleVector<int>.Handle radiationAccumulator;

        public float FusionHeatRate => Game.Instance.accumulators.GetAverageRate(heatAccumulator);
        public float FusionRadiationRate => Game.Instance.accumulators.GetAverageRate(radiationAccumulator);

        private static StatusItem fusionHeatStatus;
        private static StatusItem fusionRadiationStatus;


        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();

            if (fusionHeatStatus == null)
            {
                fusionHeatStatus = new StatusItem("FusionHeatStatus", "BUILDING", string.Empty, StatusItem.IconType.Info, NotificationType.Neutral, false, OverlayModes.None.ID)
                    .SetResolveStringCallback((string str, object data) =>
                    {
                        var reactor = (FusionReactor)data;
                        string formattedHeatEnergy = GameUtil.GetFormattedHeatEnergyRate(reactor.FusionHeatRate * 1000f);
                        str = str.Replace("{FusionHeat}", formattedHeatEnergy);
                        return str;
                    });
            }

            if (fusionRadiationStatus == null)
            {
                fusionRadiationStatus = new StatusItem("FusionRadiationStatus", "BUILDING", string.Empty, StatusItem.IconType.Info, NotificationType.Neutral, false, OverlayModes.None.ID)
                    .SetResolveStringCallback((string str, object data) =>
                    {
                        var reactor = (FusionReactor)data;
                        string radiationName = GameUtil.GetFormattedDiseaseName(RadiationIndex);
                        string formattedRadiation = GameUtil.GetFormattedSimple(reactor.FusionRadiationRate, GameUtil.TimeSlice.PerSecond);
                        str = str.Replace("{RadiationName}", radiationName);
                        str = str.Replace("{FusionRadiation}", formattedRadiation);
                        return str;
                    });
            }
        }

        protected override void OnSpawn()
        {
            base.OnSpawn();

            heatAccumulator = Game.Instance.accumulators.Add("HeatProduced", this);
            radiationAccumulator = Game.Instance.accumulators.Add("RadiationProduced", this);

            elementConverter.SetStorage(fusionStorage);
            elementConverter.onConvertMass += OnConvertMass;
        }

        protected override void OnCleanUp()
        {
            elementConverter.onConvertMass -= OnConvertMass;

            Game.Instance.accumulators.Remove(heatAccumulator);
            Game.Instance.accumulators.Remove(radiationAccumulator);

            base.OnCleanUp();
        }

        private void UpdateHasFuelStatus()
        {
            hasFuel = elementConverter.CanConvertAtAll();

            bool statusShown = needsFuelStatusItemGuid != Guid.Empty;
            if (hasFuel != statusShown) return;

            Log.Spam($"UpdateHasFuelStatus: {hasFuel}");

            var data = new Dictionary<Tag, float>();
            foreach (var input in elementConverter.consumedElements)
            {
                data.Add(input.tag, input.massConsumptionRate);
            }

            StatusItem statusItem = Db.Get().BuildingStatusItems.MaterialsUnavailable;
            needsFuelStatusItemGuid = selectable.ToggleStatusItem(statusItem, needsFuelStatusItemGuid, !hasFuel, data);
        }

        private void UpdateCoolantStatus(float coolantMass)
        {
            hasCoolant = coolantMass >= minimumCoolantMass;

            bool statusShown = needsCoolantStatusItemGuid != Guid.Empty;
            if (hasCoolant != statusShown) return;

            Log.Spam($"UpdateCoolantStatus: {hasCoolant}");

            var data = new Dictionary<Tag, float>();
            data.Add(GameTags.Liquid, minimumCoolantMass);

            StatusItem statusItem = Db.Get().BuildingStatusItems.MaterialsUnavailable;
            needsCoolantStatusItemGuid = selectable.ToggleStatusItem(statusItem, needsCoolantStatusItemGuid, !hasCoolant, data);
        }

        private void UpdateFusionStatus()
        {
            selectable.ToggleStatusItem(fusionHeatStatus, FusionHeatRate > 0, this);
            selectable.ToggleStatusItem(fusionRadiationStatus, FusionRadiationRate > 0, this);
        }

        private bool IsFuel(PrimaryElement element)
        {
            foreach (var input in elementConverter.consumedElements)
            {
                if (element.tag == input.tag) return true;
            }

            return false;
        }

        private bool IsExhaust(PrimaryElement element)
        {
            foreach (var output in elementConverter.outputElements)
            {
                if (element.ElementID == output.elementHash) return true;
            }

            return false;
        }

        private bool IsFuelOrExhaust(PrimaryElement element) => IsFuel(element) || IsExhaust(element);

        private void GetCoolantMassAndHeatCapacity(out float totalCoolantMass, out float totalCoolantHeatCapacity)
        {
            totalCoolantMass = 0;
            totalCoolantHeatCapacity = 0;
            foreach (var go in coolantStorage.items)
            {
                var coolant = go.GetComponent<PrimaryElement>();
                if (coolant == null || coolant.Mass == 0 || IsFuelOrExhaust(coolant)) continue;

                totalCoolantMass += coolant.Mass;
                totalCoolantHeatCapacity += coolant.Mass * coolant.Element.specificHeatCapacity;
            }
        }

        private void OnConvertMass(float mass)
        {
            if (mass <= 0) return;

            Log.Spam($"Converted {mass} kg");

            GetCoolantMassAndHeatCapacity(out float totalCoolantMass, out float totalCoolantHeatCapacity);

            float radiationPerKg = mass * producedRadiationPerKg / totalCoolantMass;
            float heatPerCapacity = mass * producedHeatPerKg / totalCoolantHeatCapacity;

            int radiation = (int)(mass * producedRadiationPerKg);

            foreach (var go in coolantStorage.items)
            {
                var coolant = go.GetComponent<PrimaryElement>();
                if (coolant == null || coolant.Mass == 0 || IsFuelOrExhaust(coolant)) continue;

                int radiationPart = (int)(coolant.Mass * radiationPerKg);
                radiation -= radiationPart;
                coolant.AddDisease(RadiationIndex, radiationPart, "FusionReactor");

                float heat = coolant.Mass * coolant.Element.specificHeatCapacity * heatPerCapacity;
                float temperatureChange = GameUtil.CalculateTemperatureChange(coolant.Element.specificHeatCapacity, coolant.Mass, heat);
                coolant.Temperature += temperatureChange;

                Game.Instance.accumulators.Accumulate(heatAccumulator, heat);
                Game.Instance.accumulators.Accumulate(radiationAccumulator, radiationPart);

                Log.Spam($"Added {heat} kDTU ({temperatureChange} K) and {radiationPart} radiation to {coolant.ElementID}");
            }

            if (radiation > 0)
            {
                var coolant = coolantStorage.FindFirstWithMass(GameTags.Any);
                if (coolant != null)
                {
                    Game.Instance.accumulators.Accumulate(radiationAccumulator, radiation);
                    coolant.AddDisease(RadiationIndex, radiation, "FusionReactor");
                    Log.Spam($"Added {radiation} radiation to {coolant.ElementID}");
                }
            }

            foreach (var go in fusionStorage.items)
            {
                var fuel = go.GetComponent<PrimaryElement>();
                if (fuel == null || fuel.Mass == 0 || !IsFuel(fuel)) continue;

                fuel.Temperature = 9999;
            }
        }

        public void Sim200ms(float dt)
        {
            GetCoolantMassAndHeatCapacity(out float totalCoolantMass, out float totalCoolantHeatCapacity);
            
            UpdateHasFuelStatus();
            UpdateCoolantStatus(totalCoolantMass);

            UpdateFusionStatus();
            
            operational.SetActive(operational.IsOperational && hasFuel && hasCoolant);
        }
    }
}
