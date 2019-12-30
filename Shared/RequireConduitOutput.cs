using KSerialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Egladil
{
    [SkipSaveFileSerialization]
    public class RequireConduitOutput : KMonoBehaviour
    {
        public bool requireConduitHasRoom = true;

        private static readonly Operational.Flag outputConnectedFlag = new Operational.Flag("output_connected", Operational.Flag.Type.Requirement);

        private static readonly Operational.Flag pipesHaveRoomFlag = new Operational.Flag("pipesHaveRoom", Operational.Flag.Type.Requirement);

        [MyCmpReq]
        private KSelectable selectable;

        [MyCmpGet]
        private Operational operational;

        [MyCmpGet]
        public ConduitOutput conduitOutput;

        private bool previouslyConnected = true;

        private bool previouslyEmpty = true;

        public bool RequirementsMet { get; private set; }

        private HandleVector<int>.Handle partitionerEntry;

        private Guid outputConnectedStatusItem;
        private Guid pipesHaveRoomStatusItem;

        protected override void OnSpawn()
        {
            CheckRequirements(forceEvent: true);

            partitionerEntry = GameScenePartitioner.Instance.Add("RequireConduitOutput.OnSpawn", base.gameObject, conduitOutput.GetUtilityCell(), conduitOutput.ScenePartitionerLayer, OnConduitConnectionChanged);
            conduitOutput.ConduitManager.AddConduitUpdater(OnConduitUpdate, ConduitFlowPriority.First);
        }

        protected override void OnCleanUp()
        {
            GameScenePartitioner.Instance.Free(ref partitionerEntry);
            conduitOutput.ConduitManager.RemoveConduitUpdater(OnConduitUpdate);
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
            bool connected = !conduitOutput.enabled || conduitOutput.IsConnected;
            if (connected != previouslyConnected)
            {
                previouslyConnected = connected;

                StatusItem statusItem = null;
                switch (conduitOutput.TypeOfConduit)
                {
                    case ConduitType.Liquid:
                        statusItem = Db.Get().BuildingStatusItems.NeedLiquidOut;
                        break;
                    case ConduitType.Gas:
                        statusItem = Db.Get().BuildingStatusItems.NeedGasOut;
                        break;
                }

                if (statusItem != null)
                {
                    outputConnectedStatusItem = selectable.ToggleStatusItem(statusItem, outputConnectedStatusItem, !connected);
                }

                operational.SetFlag(outputConnectedFlag, connected);
            }

            bool empty = !conduitOutput.enabled || conduitOutput.ConduitContents.mass <= 0;
            if (previouslyEmpty != empty)
            {
                previouslyEmpty = empty;

                if (requireConduitHasRoom)
                {
                    StatusItem statusItem = Db.Get().BuildingStatusItems.ConduitBlockedMultiples;

                    if (statusItem != null)
                    {
                        pipesHaveRoomStatusItem = selectable.ToggleStatusItem(statusItem, pipesHaveRoomStatusItem, !empty, this);
                    }

                    operational.SetFlag(pipesHaveRoomFlag, empty);
                }
            }

            RequirementsMet = connected &&
                (!requireConduitHasRoom || empty);
        }
    }
}
