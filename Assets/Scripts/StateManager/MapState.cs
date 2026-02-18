using System;
using System.Collections.Generic;
using System.Linq;
using Map;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using Util;
using Random = System.Random;

namespace StateManager
{
    public class MapState: GameState
    {
        public EasePosition window;
        public bool hasSetup;

        public GOList goList;
        
        public int layers = 6; 
        public int minNodesPerLayer = 1;
        public int maxNodesPerLayer = 3;
        
        public float layerSpacing = 5f; 
        public float nodeSpacing = 3f;
        public float yjitter = 50f;
        
        public float extraEdgeChance = 0.55f;
        public int maxOutPerNode   = 3;
        public int maxInPerNode    = 2;

        public int mapOffset = 100;
        public int tempMapOffset = 0;
        public int scrollDistance = 120;

        public int numShops = 5;
        
        private MapNode currentNode;

        private List<List<MapNode>> mapLayers = new List<List<MapNode>>();
        
        private List<GameObject> linesList = new List<GameObject>();
        
        public static Random mapRandom = RunInfo.NewRandom("map".GetHashCode());
        
        public override void Enter()
        {
            if (!hasSetup)
            {
                hasSetup = true;
                SetupMap();
            }
            
            window.SendToLocation(new Vector3(0, 20, 0));
            
            DrawConnections();
            MoveMap();
            tempMapOffset = 0;
        }
        
        public void SetMapState()
        {
            GameStateManager.Instance.Change<MapState>();
        }

        private void UpdateCurrentNode()
        {
            foreach (List<MapNode> layer in mapLayers)
            {
                foreach (MapNode node in layer)
                {
                    node.GetComponent<Button>().interactable = false;
                }
            }

            foreach (MapNode child in currentNode.children)
            {
                child.GetComponent<Button>().interactable = true;
            }

            Vector3 newPos = currentNode.transform.localPosition;
            // newPos.x = -mapOffset;
            
            goList.GetValue("player").transform.localPosition = newPos;
        }

        private void ClickNode(MapNode node)
        {
            currentNode = node;
            UpdateCurrentNode();
            PlayingState.RewardMoney = node.rewardMoney;
            PlayingState.numNormalEnemy = node.numNormalEnemy;
            PlayingState.numHardEnemy = node.numHardEnemy;
            PlayingState.numBossEnemy = node.numBossEnemy;

            if (node.target == MapTarget.Enemy || node.target == MapTarget.HardEnemy)
            {
                GameStateManager.Instance.Change<PlayingState>();
            }

            if (node.target == MapTarget.Shop)
            {
                GameStateManager.Instance.Change<ShopState>();
            }

            if (node.target == MapTarget.Event)
            {
                // TODO: new state
                GameStateManager.Instance.Change<MapState>();
            }
            
        }

        public void GenerateMap()
        {
            mapLayers.Clear();

            // Ensure we have at least 2 layers to have a start and an end
            layers = Mathf.Max(2, layers);

            int start = 3;
            int end = layers - 2;
            int count = numShops;

            double step = (double)(end - start) / (count - 1);
            var shopIndecies = new List<int>();

            for (int i = 0; i < count; i++)
            {
                int value = (int)Math.Round(start + i * step);
                shopIndecies.Add(value);
            }
            
            for (int i = 0; i < layers; i++)
            {
                // --- Force single start and single end ---
                int nodeCount;
                if (i == 0) nodeCount = 1;                         // single start
                else if (i == layers - 1) nodeCount = 1;           // single end
                else nodeCount = mapRandom.Next(minNodesPerLayer, maxNodesPerLayer);

                List<MapNode> layer = new List<MapNode>();

                float totalHeight = (nodeCount - 1) * nodeSpacing;
                float startY = -totalHeight / 2f;

                int shopIndex = 0;

                for (int shopLayer = 0; shopLayer < shopIndecies.Count; shopLayer++)
                {
                    if (i == shopIndecies[shopLayer])
                    {
                        shopIndex = mapRandom.Next(0, nodeCount);
                    }
                }

                
                for (int j = 0; j < nodeCount; j++)
                {
                    Vector3 pos = new Vector3(
                        i * layerSpacing,
                        startY + j * nodeSpacing + ((float) mapRandom.NextDouble() * (-yjitter - yjitter)) + -yjitter,
                        0f
                    );

                    GameObject target = GetRandomTargetForLayer(i);


                    // If node is the picked node from the given layer and is the correct layer, spawn the shop.
                    if (j == shopIndex)
                    {
                        for (int shopLayer = 0; shopLayer < shopIndecies.Count; shopLayer++)
                        {
                            if (i == shopIndecies[shopLayer])
                            {
                                target = goList.GetValue("shop");
                            }
                        }
                    }
                    
                    GameObject nodeObj = Instantiate(target, goList.GetValue("anchor").transform);
                    nodeObj.transform.localPosition = pos;
                    MapNode node = nodeObj.GetComponent<MapNode>();
                    nodeObj.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        ClickNode(node);
                    });
                    layer.Add(node);
                }

                mapLayers.Add(layer);
            }

            // - Courtesy of ChatGPT - (adjusted to respect single source/sink feasibility)
            for (int i = 0; i < layers - 1; i++)
            {
                var A = mapLayers[i];
                var B = mapLayers[i + 1];

                // Track in/out degrees without changing your MapNode type
                Dictionary<MapNode,int> inDeg  = new Dictionary<MapNode,int>();
                Dictionary<MapNode,int> outDeg = new Dictionary<MapNode,int>();
                foreach (var n in A) outDeg[n] = n.children.Count; // probably 0 right now
                foreach (var n in B) inDeg[n]  = 0;

                // --- Per-band degree caps (lifted on edges touching single start/single end) ---
                int maxOutThisBand = (i == 0) 
                    ? Mathf.Max(maxOutPerNode, B.Count)          // single source must be able to parent everyone in B
                    : maxOutPerNode;

                int maxInNextBand = (i == layers - 2 && B.Count == 1)
                    ? Mathf.Max(maxInPerNode, A.Count)           // single sink must be able to accept all parents from A
                    : maxInPerNode;

                // Helper to add an edge respecting (possibly lifted) degree caps
                bool AddEdge(MapNode from, MapNode to)
                {
                    if (from.children.Contains(to)) return false;
                    if (outDeg[from] >= maxOutThisBand) return false;
                    if (inDeg[to]    >= maxInNextBand) return false;

                    from.children.Add(to);
                    outDeg[from]++; inDeg[to]++;
                    return true;
                }

                // A) Give every node in A at least one child (prefer closest-by-Y with free capacity)
                foreach (var from in A)
                {
                    MapNode best = null; float bestDist = float.MaxValue;
                    foreach (var to in B)
                    {
                        if (inDeg[to] >= maxInNextBand) continue;
                        float d = Mathf.Abs(from.transform.localPosition.y - to.transform.localPosition.y);
                        if (d < bestDist) { best = to; bestDist = d; }
                    }
                    if (best == null) best = B[mapRandom.Next(0, B.Count)]; // fallback
                    AddEdge(from, best);
                }

                // B) Ensure every node in B has at least one parent (choose closest A with spare out-degree)
                foreach (var to in B)
                {
                    if (inDeg[to] > 0) continue;

                    MapNode best = null; float bestDist = float.MaxValue;
                    foreach (var from in A)
                    {
                        if (outDeg[from] >= maxOutThisBand) continue;
                        float d = Mathf.Abs(from.transform.localPosition.y - to.transform.localPosition.y);
                        if (d < bestDist) { best = from; bestDist = d; }
                    }
                    if (best == null) best = A[mapRandom.Next(0, A.Count)]; // fallback
                    AddEdge(best, to);
                }

                // C) Optional extras: at most a couple per node, and only with some probability
                foreach (var from in A)
                {
                    if (outDeg[from] >= maxOutThisBand) continue;
                    if (mapRandom.NextDouble() > extraEdgeChance) continue;

                    MapNode best = null; float bestDist = float.MaxValue;
                    foreach (var to in B)
                    {
                        if (inDeg[to] >= maxInNextBand) continue;
                        if (from.children.Contains(to)) continue;
                        float d = Mathf.Abs(from.transform.localPosition.y - to.transform.localPosition.y);
                        if (d < bestDist) { best = to; bestDist = d; }
                    }
                    if (best != null) AddEdge(from, best);
                }
            }

            Debug.Log("Map generated with " + layers + " layers.");
        }
        
        private void DrawConnections()
        {
            linesList.ForEach(Destroy);
            
            foreach (var layer in mapLayers)
            {
                foreach (var node in layer)
                {
                    foreach (MapNode child in node.children)
                    {
                        GameObject lineObj = Instantiate(goList.GetValue("line"), goList.GetValue("anchor").transform);
                        lineObj.transform.localPosition = Vector2.zero;
                        lineObj.transform.SetSiblingIndex(0);
                        UILineRenderer lr = lineObj.GetComponent<UILineRenderer>();
                        linesList.Add(lr.gameObject);

                        Vector2[] points = new Vector2[2];
                        points[0] = node.transform.localPosition;
                        points[1] = child.transform.localPosition;
                        lr.Points = points;

                        // Highlight lines that connect from OR to the current node
                        if (node == currentNode)
                        {
                            lr.LineThickness = 4f; // thick connection (active)
                        }
                        else
                        {
                            lr.LineThickness = 2f; // thin connection (inactive)
                            lr.color = Color.darkGray;
                        }
                    }
                }
            }
        }



        private GameObject GetRandomTargetForLayer(int layerIndex)
        {
            if (layerIndex == 0)
            {
                return goList.GetValue("start");
            }
            
            if (layerIndex < layers - 1)
            {
                float random = (float) mapRandom.NextDouble();
                
                if (random >= 0 && random < 0.15f)
                    return goList.GetValue("event");
                
                if ((random >= 0.15f && random < 0.75f) || layerIndex <= 2)
                    return goList.GetValue("enemy");
                
                if (random >= 0.75f && random < 1f)
                    return goList.GetValue("hard");
            }
            
            return goList.GetValue("boss");
        }

        private void SetupMap()
        {
            GenerateMap();
            currentNode = mapLayers[0][0];
            UpdateCurrentNode();
            Debug.Log("Map generated with " + layers + " layers.");
            DrawConnections();
        }

        public void Update()
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll > 0f)
            {
                tempMapOffset += scrollDistance;
                MoveMap();
            } else if (scroll < 0f)
            {
                tempMapOffset -= scrollDistance;
                MoveMap();
            }
        }
        
        private void MoveMap()
        {
            tempMapOffset = (int) Mathf.Clamp(tempMapOffset, -((GetLayerIndex(currentNode)) * layerSpacing), (layers * layerSpacing) - (2 * mapOffset) - ((GetLayerIndex(currentNode)) * layerSpacing));
            goList.GetValue("anchor").GetComponent<LerpPosition>().targetLocation = new Vector3((-(GetLayerIndex(currentNode)) * layerSpacing) - mapOffset - tempMapOffset, 0, 0);
            goList.GetValue("player").transform.SetAsLastSibling();
        }

        private int GetLayerIndex(MapNode node)
        {
            for (int i = 0; i < mapLayers.Count; i++)
            {
                if (mapLayers[i].Contains(node))
                    return i;
            }

            return -1;
        }

        public override void Exit()
        {
            window.SendToLocation(new Vector2(0, 750));
        }
    }
}