using KSerialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Egladil
{
    [SkipSaveFileSerialization]
    public class RequireConduitInput : KMonoBehaviour
    {
        public bool requireConduitHasMass = true;

        private static readonly Operational.Flag inputConnectedFlag = new Operational.Flag("inputConnected", Operational.Flag.Type.Requirement);

        private static readonly Operational.Flag pipesHaveMassFlag = new Operational.Flag("pipesHaveMass", Operational.Flag.Type.Requirement);
        
        [MyCmpReq]
        private KSelectable selectable;

        [MyCmpGet]
        private Operational operational;
        
        [MyCmpGet]
        public ConduitInput conduitInput;

        private bool previouslyConnected = true;

        private bool previouslySatisfied = true;

        public bool RequirementsMet { get; private set; }

        private HandleVector<int>.Handle partitionerEntry;

        private Guid inputConnectedStatusItem;
        private Guid pipesHaveMassStatusItem;

        protected override void OnSpawn()
        {
            CheckRequirements(forceEvent: true);

            partitionerEntry = GameScenePartitioner.Instance.Add("RequireConduitOutput.OnSpawn", base.gameObject, conduitInput.GetUtilityCell(), conduitInput.ScenePartitionerLayer, OnConduitConnectionChanged);
            conduitInput.ConduitManager.AddConduitUpdater(OnConduitUpdate, ConduitFlowPriority.First);
        }

        protected override void OnCleanUp()
        {
            GameScenePartitioner.Instance.Free(ref partitionerEntry);
            conduitInput.ConduitManager.RemoveConduitUpdater(OnConduitUpdate);
        }

        private void OnConduitConnectionChanged(object data)
        {
            CheckRequirements(forceEvent: false);
        }

        private void OnConduitUpdate(float dt)
        {
            CheckRequirements(forceEvent: false);
        }

        private void CheckRequirements(bool forceEvent)
        {
            bool connected = !conduitInput.enabled || conduitInput.IsConnected;
            if (connected != previouslyConnected)
            {
                previouslyConnected = connected;

                StatusItem statusItem = null;
                switch (conduitInput.TypeOfConduit)
                {
                    case ConduitType.Liquid:
                        statusItem = Db.Get().BuildingStatusItems.NeedLiquidIn;
                        break;
                    case ConduitType.Gas:
                        statusItem = Db.Get().BuildingStatusItems.NeedGasIn;
                        break;
                }

                if (statusItem != null)
                {
                    inputConnectedStatusItem = selectable.ToggleStatusItem(statusItem, inputConnectedStatusItem, !connected, new Tuple<ConduitType, Tag>(conduitInput.TypeOfConduit, conduitInput.capacityTag));
                }

                operational.SetFlag(inputConnectedFlag, connected);
            }

            bool satisfied = !conduitInput.enabled || conduitInput.IsSatisfied;
            if (previouslySatisfied != satisfied)
            {
                previouslySatisfied = satisfied;

                if (requireConduitHasMass)
                {
                    StatusItem statusItem = null;
                    switch (conduitInput.TypeOfConduit)
                    {
                        case ConduitType.Liquid:
                            statusItem = Db.Get().BuildingStatusItems.LiquidPipeEmpty;
                            break;
                        case ConduitType.Gas:
                            statusItem = Db.Get().BuildingStatusItems.GasPipeEmpty;
                            break;
                    }

                    if (statusItem != null)
                    {
                        pipesHaveMassStatusItem = selectable.ToggleStatusItem(statusItem, pipesHaveMassStatusItem, !satisfied, this);
                    }

                    operational.SetFlag(pipesHaveMassFlag, satisfied);
                }
            }

            RequirementsMet = connected &&
                (!requireConduitHasMass || satisfied);
        }
    }
}
