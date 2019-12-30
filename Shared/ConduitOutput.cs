using KSerialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Egladil
{
    [SerializationConfig(MemberSerialization.OptIn)]
    public class ConduitOutput : KMonoBehaviour, ISaveLoadable, ISecondaryOutput
    {
        [SerializeField]
        public ConduitPortInfo portInfo;

        [SerializeField]
        public SimHashes[] elementFilter;

        [SerializeField]
        public bool invertElementFilter;

        [SerializeField]
        public bool alwaysDispense;

        [SerializeField]
        public bool isOn = true;

        [SerializeField]
        public bool blocked;

        private static readonly Operational.Flag outputConduitFlag = new Operational.Flag("output_conduit", Operational.Flag.Type.Functional);

        [MyCmpReq]
        private Building building;

        [MyCmpReq]
        private Operational operational;

        [MyCmpReq]
        public Storage storage;

        private int utilityCell = -1;
        private FlowUtilityNetwork.NetworkItem networkItem;
        private HandleVector<int>.Handle partitionerEntry;

        private int elementOutputOffset;

        public ConduitType GetSecondaryConduitType() => portInfo.conduitType;
        public CellOffset GetSecondaryConduitOffset() => building.GetRotatedOffset(portInfo.offset);
        public int GetUtilityCell() => Grid.OffsetCell(Grid.PosToCell(base.transform.GetPosition()), GetSecondaryConduitOffset());

        public ConduitType TypeOfConduit => portInfo.conduitType;
        public ConduitFlow.ConduitContents ConduitContents => ConduitManager.GetContents(utilityCell);
        public bool IsConnected => RequireOutputs.IsConnected(utilityCell, portInfo.conduitType);
        public IUtilityNetworkMgr NetworkManager => Conduit.GetNetworkManager(portInfo.conduitType);
        public ConduitFlow ConduitManager => Conduit.GetFlowManager(portInfo.conduitType);
        public ScenePartitionerLayer ScenePartitionerLayer => portInfo.conduitType == ConduitType.Gas ? GameScenePartitioner.Instance.gasConduitsLayer : portInfo.conduitType == ConduitType.Liquid ? GameScenePartitioner.Instance.liquidConduitsLayer : GameScenePartitioner.Instance.solidConduitsLayer;

        protected override void OnSpawn()
        {
            base.OnSpawn();

            GameScheduler.Instance.Schedule("PlumbingTutorial", 2f, delegate
            {
                Tutorial.Instance.TutorialMessage(Tutorial.TutorialMessages.TM_Plumbing);
            });

            utilityCell = GetUtilityCell();

            networkItem = new FlowUtilityNetwork.NetworkItem(portInfo.conduitType, Endpoint.Source, utilityCell, base.gameObject);
            NetworkManager.AddToNetworks(utilityCell, networkItem, is_endpoint: true);
            
            partitionerEntry = GameScenePartitioner.Instance.Add("ConduitOutput.OnSpawn", base.gameObject, utilityCell, ScenePartitionerLayer, OnConduitConnectionChanged);

            ConduitManager.AddConduitUpdater(ConduitUpdate, ConduitFlowPriority.Dispense);
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
            operational.SetFlag(outputConduitFlag, IsConnected);
            blocked = false;
            if (isOn)
            {
                Dispense(dt);
            }
        }

        private void Dispense(float dt)
        {
            if (!operational.IsOperational && !alwaysDispense)
            {
                return;
            }

            PrimaryElement primaryElement = FindSuitableElement();
            if (primaryElement == null) return;

            primaryElement.KeepZeroMassObject = true;

            float dispensed = ConduitManager.AddElement(utilityCell, primaryElement.ElementID, primaryElement.Mass, primaryElement.Temperature, primaryElement.DiseaseIdx, primaryElement.DiseaseCount);
            if (dispensed > 0)
            {
                float dispensedFraction = dispensed / primaryElement.Mass;
                int dispensedGerms = (int)(dispensedFraction * (float)primaryElement.DiseaseCount);
                primaryElement.ModifyDiseaseCount(-dispensedGerms, "ConduitOutput.Dispense");
                primaryElement.Mass -= dispensed;
                Trigger(-1697596308, primaryElement.gameObject);
            }
            else
            {
                blocked = true;
            }
        }

        private PrimaryElement FindSuitableElement()
        {
            List<GameObject> items = storage.items;
            int count = items.Count;
            for (int i = 0; i < count; i++)
            {
                int index = (i + elementOutputOffset) % count;
                PrimaryElement component = items[index].GetComponent<PrimaryElement>();
                if (component != null &&
                    component.Mass > 0 &&
                    (portInfo.conduitType != ConduitType.Liquid ? component.Element.IsGas : component.Element.IsLiquid) &&
                    IsAllowedElement(component.ElementID))
                {
                    elementOutputOffset = (elementOutputOffset + 1) % count;
                    return component;
                }
            }
            return null;
        }

        private bool IsFilteredElement(SimHashes element)
        {
            for (int i = 0; i != elementFilter.Length; i++)
            {
                if (elementFilter[i] == element)
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsAllowedElement(SimHashes element)
        {
            if (elementFilter == null || elementFilter.Length == 0) return true;

            if (invertElementFilter)
            {
                return !IsFilteredElement(element);
            }
            else
            {
                return IsFilteredElement(element);
            }
        }
    }
}
