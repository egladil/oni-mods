using Harmony;
using Klei.AI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Egladil
{
    public class Radioactivity : KMonoBehaviour, ISim33ms, ISim1000ms
    {
        private int next;

        private int[] positions = new int[Grid.CellCount];

        private readonly byte radiationIdx = RadiationGerms.Index;
        private readonly Disease radiationDisease = RadiationGerms.Instance;

        protected override void OnSpawn()
        {
            base.OnSpawn();

            for (int i = 0; i < positions.Length; i++)
            {
                positions[i] = i;
            }

            positions.OrderBy(x => UnityEngine.Random.value);
        }

        private static ObjectLayer GetObjectLayer(ConduitType conduitType)
        {
            switch (conduitType)
            {
                case ConduitType.Gas:
                    return ObjectLayer.GasConduit;
                case ConduitType.Liquid:
                    return ObjectLayer.LiquidConduit;
                case ConduitType.Solid:
                    return ObjectLayer.SolidConduit;
            }

            return ObjectLayer.NumLayers;
        }

        private void UpdateFromNeighbours(int cellPos, Element cellElement, ref int cellCount)
        {
            foreach (int pos in new int[] { Grid.CellAbove(cellPos), Grid.CellBelow(cellPos), Grid.CellLeft(cellPos), Grid.CellBelow(cellPos) })
            {
                if (!Grid.IsValidCell(pos))
                {
                    continue;
                }

                var element = Grid.Element[pos];
                if (element.state == cellElement.state)
                {
                    continue;
                }

                if (Grid.DiseaseIdx[pos] != radiationIdx)
                {
                    continue;
                }

                int diff = Grid.DiseaseCount[pos] - cellCount;
                if (diff <= 0)
                {
                    continue;
                }

                var growth = radiationDisease.GetGrowthRuleForElement(element);
                int increase = Mathf.RoundToInt(diff * growth.diffusionScale / 2);
                if (increase <= 0)
                {
                    continue;
                }

                SimMessages.ModifyDiseaseOnCell(pos, radiationIdx, -increase);
                SimMessages.ModifyDiseaseOnCell(cellPos, radiationIdx, increase);
                cellCount += increase;
            }
        }

        private void UpdateConduit(ConduitFlow conduitFlow, ConduitType conduitType, int cellPos, Element cellElement, ref int cellCount)
        {
            var building = Grid.Objects[cellPos, (int)GetObjectLayer(conduitType)];
            if (building == null || building.GetComponent<BuildingComplete>() == null) return;

            var pipe = building.GetComponent<PrimaryElement>();
            var pipeElement = pipe.Element;
            int pipeCount = pipe.DiseaseIdx == radiationIdx ? pipe.DiseaseCount : 0;

            var content = conduitFlow.GetContents(cellPos);

            //string typeLog = $"{cellElement.tag}, {pipeElement.tag}";

            if (content.mass > 0)
            {
                var contentElement = ElementLoader.FindElementByHash(content.element);
                //typeLog += $", {contentElement.tag}";

                int contentCount = content.diseaseIdx == radiationIdx ? content.diseaseCount : 0;
                int diff = contentCount - pipeCount;
                
                var growth = diff > 0 ? radiationDisease.GetGrowthRuleForElement(contentElement) : radiationDisease.GetGrowthRuleForElement(pipeElement);
                float diffusionScale = Mathf.Min(growth.diffusionScale * 10, 1);
                int increase = Mathf.RoundToInt(diff * diffusionScale / 2);

                if (increase != 0)
                {
                    int oldPipe = pipeCount;
                    int oldContent = contentCount;

                    content.diseaseCount = contentCount - increase;
                    content.diseaseIdx = radiationIdx;
                    conduitFlow.SetContents(cellPos, content);

                    pipe.AddDisease(radiationIdx, increase, "Radioactivity.UpdateConduit");
                    pipeCount += increase;

                    //Log.Spam($"{cellPos} increase {increase} ({diffusionScale}), content {oldContent} => {content.diseaseCount}, pipe {oldPipe} =>{pipe.DiseaseCount}");
                }
            }

            if (cellElement.id != SimHashes.Vacuum && cellElement.id != SimHashes.Void)
            {
                int diff = pipeCount - cellCount;

                var growth = diff > 0 ? radiationDisease.GetGrowthRuleForElement(pipeElement) : radiationDisease.GetGrowthRuleForElement(cellElement);
                float diffusionScale = Mathf.Min(growth.diffusionScale * 10, 1);
                int increase = Mathf.RoundToInt(diff * diffusionScale / 2);

                if (increase != 0)
                {
                    int oldPipe = pipeCount;
                    int oldCell = cellCount;

                    pipe.AddDisease(radiationIdx, -increase, "Radioactivity.UpdateConduit");

                    SimMessages.ModifyDiseaseOnCell(cellPos, radiationIdx, increase);
                    cellCount += increase;

                    //Log.Spam($"{cellPos} increase {increase} ({diffusionScale}), pipe {oldPipe} => {pipe.DiseaseCount}, grid {oldCell} => {Grid.DiseaseCount[cellPos]}");
                }
            }

            //Log.Spam($"{cellPos} {typeLog}");
        }

        private void UpdateConduitNetwork(ConduitFlow conduitFlow, ConduitType conduitType, List<int> cells)
        {
            //Log.Spam($"Begin UpdateConduitNetwork {cells.Join()}");

            foreach(int cellPos in cells)
            {
                var cellElement = Grid.Element[cellPos];

                int cellCount = 0;
                if (Grid.DiseaseIdx[cellPos] == radiationIdx)
                {
                    cellCount = Grid.DiseaseCount[cellPos];
                }

                UpdateConduit(conduitFlow, conduitType, cellPos, cellElement, ref cellCount);
            }

            //Log.Spam($"End UpdateConduitNetwork {cells.Join()}");
        }

        private void UpdateConduitFlow(ConduitFlow conduitFlow)
        {
            var conduitType = Traverse.Create(conduitFlow).Field("conduitType").GetValue<ConduitType>();
            //Log.Spam($"Begin UpdateConduitFlow {conduitType}");

            var networks = Traverse.Create(conduitFlow).Field("networks").GetValue<IEnumerable>();

            foreach (var network in networks)
            {
                var cells = Traverse.Create(network).Field("cells").GetValue<List<int>>();

                UpdateConduitNetwork(conduitFlow, conduitType, cells);
            }

            //Log.Spam($"End UpdateConduitFlow {conduitType}");
        }

        public void Sim33ms(float dt)
        {
            if (positions.Length <= 0)
            {
                return;
            }

            for (int i = 0; i < 100; i++)
            {
                next = (next + 1) % positions.Length;
                int cellPos = positions[next];

                var cellElement = Grid.Element[cellPos];
                if (cellElement.id == SimHashes.Vacuum || cellElement.id == SimHashes.Void)
                {
                    continue;
                }

                int cellCount = 0;
                if (Grid.DiseaseIdx[cellPos] == radiationIdx)
                {
                    cellCount = Grid.DiseaseCount[cellPos];
                }

                UpdateFromNeighbours(cellPos, cellElement, ref cellCount);
            }
        }

        public void Sim1000ms(float dt)
        {
            UpdateConduitFlow(Game.Instance.gasConduitFlow);
            UpdateConduitFlow(Game.Instance.liquidConduitFlow);
        }


        [HarmonyPatch(typeof(World), "OnSpawn")]
        public class World_OnSpawn_Patch
        {
            public static void Postfix(World __instance)
            {
                __instance.FindOrAdd<Radioactivity>();
            }
        }
    }
}
