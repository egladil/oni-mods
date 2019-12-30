using KSerialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Egladil
{
    [SerializationConfig(MemberSerialization.OptIn)]
    public class ConduitInput : KMonoBehaviour, ISaveLoadable, ISecondaryInput
    {
        [SerializeField]
        public ConduitPortInfo portInfo;

        [SerializeField]
        public bool ignoreMinMassCheck;

        [SerializeField]
        public Tag capacityTag = GameTags.Any;

        [SerializeField]
        public float capacityKG = float.PositiveInfinity;

        [SerializeField]
        public bool forceAlwaysSatisfied;

        [SerializeField]
        public bool alwaysConsume;

        [SerializeField]
        public bool keepZeroMassObject = true;

        [SerializeField]
        public bool isOn = true;

        [NonSerialized]
        public bool isConsuming = true;

        [MyCmpReq]
        public Operational operational;

        [MyCmpReq]
        private Building building;

        [MyCmpGet]
        public Storage storage;

        private int utilityCell = -1;
        private FlowUtilityNetwork.NetworkItem networkItem;
        private HandleVector<int>.Handle partitionerEntry;

        public float consumptionRate = float.PositiveInfinity;

        public SimHashes lastConsumedElement = SimHashes.Vacuum;

        private bool satisfied;

        public ConduitConsumer.WrongElementResult wrongElementResult;

        public ConduitType GetSecondaryConduitType() => portInfo.conduitType;
        public CellOffset GetSecondaryConduitOffset() => building.GetRotatedOffset(portInfo.offset);
        public int GetUtilityCell() => Grid.OffsetCell(Grid.PosToCell(base.transform.GetPosition()), GetSecondaryConduitOffset());

        public ConduitType TypeOfConduit => portInfo.conduitType;
        public ConduitFlow.ConduitContents ConduitContents => ConduitManager.GetContents(utilityCell);
        public bool IsConnected => RequireOutputs.IsConnected(utilityCell, portInfo.conduitType);
        public IUtilityNetworkMgr NetworkManager => Conduit.GetNetworkManager(portInfo.conduitType);
        public ConduitFlow ConduitManager => Conduit.GetFlowManager(portInfo.conduitType);
        public ScenePartitionerLayer ScenePartitionerLayer => portInfo.conduitType == ConduitType.Gas ? GameScenePartitioner.Instance.gasConduitsLayer : portInfo.conduitType == ConduitType.Liquid ? GameScenePartitioner.Instance.liquidConduitsLayer : GameScenePartitioner.Instance.solidConduitsLayer;
        public float MassAvailable => ConduitContents.mass;
        public bool CanConsume => IsConnected && MassAvailable > 0;

        public float stored_mass => (storage == null) ? 0f : ((!(capacityTag != GameTags.Any)) ? storage.MassStored() : storage.GetMassAvailable(capacityTag));

        public float space_remaining_kg
        {
            get
            {
                float num = capacityKG - stored_mass;
                return (!(storage == null)) ? Mathf.Min(storage.RemainingCapacity(), num) : num;
            }
        }

        public bool IsAlmostEmpty => !ignoreMinMassCheck && MassAvailable < ConsumptionRate * 30f;
        public bool IsEmpty => !ignoreMinMassCheck && (MassAvailable == 0f || MassAvailable < ConsumptionRate);
        public float ConsumptionRate => consumptionRate;

        public bool IsSatisfied
        {
            get
            {
                return satisfied || !isConsuming;
            }
            set
            {
                satisfied = (value || forceAlwaysSatisfied);
            }
        }

        protected override void OnSpawn()
        {
            base.OnSpawn();

            GameScheduler.Instance.Schedule("PlumbingTutorial", 2f, delegate
            {
                Tutorial.Instance.TutorialMessage(Tutorial.TutorialMessages.TM_Plumbing);
            });

            utilityCell = GetUtilityCell();

            networkItem = new FlowUtilityNetwork.NetworkItem(portInfo.conduitType, Endpoint.Sink, utilityCell, base.gameObject);
            NetworkManager.AddToNetworks(utilityCell, networkItem, is_endpoint: true);
            
            partitionerEntry = GameScenePartitioner.Instance.Add("ConduitInput.OnSpawn", base.gameObject, utilityCell, ScenePartitionerLayer, OnConduitConnectionChanged);

            ConduitManager.AddConduitUpdater(ConduitUpdate);
            OnConduitConnectionChanged(null);

            OnConduitConnectionChanged(null);
        }

        protected override void OnCleanUp()
        {
            NetworkManager.RemoveFromNetworks(utilityCell, networkItem, is_endpoint: true);
            ConduitManager.RemoveConduitUpdater(ConduitUpdate);
            GameScenePartitioner.Instance.Free(ref partitionerEntry);
            base.OnCleanUp();
        }

        private void OnConduitConnectionChanged(object data)
        {
            Trigger((int)GameHashes.ConduitConnectionChanged, IsConnected);
        }

        public void SetOnState(bool onState)
        {
            isOn = onState;
        }

        private void ConduitUpdate(float dt)
        {
            if (isConsuming && isOn)
            {
                Consume(dt);
            }
        }

        private void Consume(float dt)
        {
            IsSatisfied = false;

            if (!IsConnected) return;

            ConduitFlow.ConduitContents contents = ConduitContents;
            if (contents.mass <= 0f) return;

            IsSatisfied = true;

            if (!alwaysConsume && !operational.IsOperational) return;

            float massToConsume = ConsumptionRate * dt;
            massToConsume = Mathf.Min(massToConsume, space_remaining_kg);
            float consumedMass = 0f;
            if (massToConsume > 0f)
            {
                ConduitFlow.ConduitContents conduitContents = ConduitManager.RemoveElement(utilityCell, massToConsume);
                consumedMass = conduitContents.mass;
                lastConsumedElement = conduitContents.element;
            }

            Element consumedElement = ElementLoader.FindElementByHash(contents.element);
            bool validElement = consumedElement.HasTag(capacityTag);
            if (consumedMass > 0f && capacityTag != GameTags.Any && !validElement)
            {
                Trigger(-794517298, new BuildingHP.DamageSourceInfo
                {
                    damage = 1,
                    source = STRINGS.BUILDINGS.DAMAGESOURCES.BAD_INPUT_ELEMENT,
                    popString = STRINGS.UI.GAMEOBJECTEFFECTS.DAMAGE_POPS.WRONG_ELEMENT
                });
            }

            if (validElement ||
                wrongElementResult == ConduitConsumer.WrongElementResult.Store ||
                contents.element == SimHashes.Vacuum ||
                capacityTag == GameTags.Any)
            {
                if (consumedMass <= 0) return;

                int consumedGerms = (int)((float)contents.diseaseCount * (consumedMass / contents.mass));
                if (portInfo.conduitType == ConduitType.Liquid && consumedElement.IsLiquid)
                {
                    storage.AddLiquid(contents.element, consumedMass, contents.temperature, contents.diseaseIdx, consumedGerms, keepZeroMassObject, do_disease_transfer: false);
                }
                else if (portInfo.conduitType == ConduitType.Gas && consumedElement.IsGas)
                {
                    storage.AddGasChunk(contents.element, consumedMass, contents.temperature, contents.diseaseIdx, consumedGerms, keepZeroMassObject, do_disease_transfer: false);
                }
                else
                {
                    Log.Warn($"{portInfo.conduitType} consumer consuming {consumedElement.state}: {consumedElement.id}");
                }
            }
            else if (consumedMass > 0 && wrongElementResult == ConduitConsumer.WrongElementResult.Dump)
            {
                int consumedGerms = (int)((float)contents.diseaseCount * (consumedMass / contents.mass));
                int dumpCell = Grid.PosToCell(base.transform.GetPosition());
                SimMessages.AddRemoveSubstance(dumpCell, contents.element, CellEventLogger.Instance.ConduitConsumerWrongElement, consumedMass, contents.temperature, contents.diseaseIdx, consumedGerms);
            }
        }
    }
}
