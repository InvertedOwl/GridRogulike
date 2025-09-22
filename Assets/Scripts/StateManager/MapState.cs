using System.Collections.Generic;
using Map;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using Util;

namespace StateManager
{
    public class MapState: GameState
    {
        public LerpPosition window;
        public bool hasSetup;

        public GOList goList;
        
        public int layers = 6; 
        public int minNodesPerLayer = 2;
        public int maxNodesPerLayer = 4;
        
        public float layerSpacing = 5f; 
        public float nodeSpacing = 3f;
        public float yjitter = 50f;
        
        public float extraEdgeChance = 0.15f;
        public int   maxOutPerNode   = 2;
        public int   maxInPerNode    = 2;
        
        private MapNode currentNode;

        private List<List<MapNode>> mapLayers = new List<List<MapNode>>();
        
        public override void Enter()
        {
            if (!hasSetup)
            {
                hasSetup = true;
                SetupMap();
            }
            
            window.targetLocation = new Vector2(0, 0);
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
            
            goList.GetValue("player").transform.position = currentNode.transform.position;
        }

        private void ClickNode(MapNode node)
        {
            currentNode = node;
            UpdateCurrentNode();

            if (node.target == MapTarget.Enemy)
                GameStateManager.Instance.Change<PlayingState>();
            if (node.target == MapTarget.Shop)
                GameStateManager.Instance.Change<ShopState>();
            if (node.target == MapTarget.Mystery)
                GameStateManager.Instance.Change<PlayingState>();
        }

        public void GenerateMap()
        {
            mapLayers.Clear();

            // Ensure we have at least 2 layers to have a start and an end
            layers = Mathf.Max(2, layers);

            for (int i = 0; i < layers; i++)
            {
                // --- Force single start and single end ---
                int nodeCount;
                if (i == 0) nodeCount = 1;                         // single start
                else if (i == layers - 1) nodeCount = 1;           // single end
                else nodeCount = Random.Range(minNodesPerLayer, maxNodesPerLayer + 1);

                List<MapNode> layer = new List<MapNode>();

                float totalHeight = (nodeCount - 1) * nodeSpacing;
                float startY = -totalHeight / 2f;

                int shopIndex = 0;
                // Layer 5 and 3
                if (i == 4 || i == 2)
                {
                    shopIndex = Random.Range(0, nodeCount);
                }
                
                for (int j = 0; j < nodeCount; j++)
                {
                    Vector3 pos = new Vector3(
                        i * layerSpacing,
                        startY + j * nodeSpacing + Random.Range(-yjitter, yjitter),
                        0f
                    );

                    GameObject target = GetRandomTargetForLayer(i);

                    if ((i == 4 || i == 2) && j == shopIndex)
                    {
                        target = goList.GetValue("shop");
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
                    if (best == null) best = B[Random.Range(0, B.Count)]; // fallback
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
                    if (best == null) best = A[Random.Range(0, A.Count)]; // fallback
                    AddEdge(best, to);
                }

                // C) Optional extras: at most a couple per node, and only with some probability
                foreach (var from in A)
                {
                    if (outDeg[from] >= maxOutThisBand) continue;
                    if (Random.value > extraEdgeChance) continue;

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
            DrawConnections();
        }
        
        private void DrawConnections()
        {
            foreach (var layer in mapLayers)
            {
                foreach (var node in layer)
                {
                    foreach (MapNode child in node.children)
                    {
                        GameObject lineObj = Instantiate(goList.GetValue("line"), goList.GetValue("anchor").transform);
                        lineObj.transform.localPosition = new Vector2(0, 0);
                        lineObj.transform.SetSiblingIndex(0);
                        UILineRenderer lr = lineObj.GetComponent<UILineRenderer>();

                        Vector2[] points = new Vector2[2];
                        points[0] = node.transform.localPosition;
                        points[1] = child.transform.localPosition;

                        lr.Points = points;
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
                float random = Random.value;
                if (random < 0.1f)
                    return goList.GetValue("event");
                if (random > 0.1f)
                    return goList.GetValue("enemy");
            }
            
            // TODO: replace with boss when I make said boss
            return goList.GetValue("enemy");
        }

        private void SetupMap()
        {
            GenerateMap();
            currentNode = mapLayers[0][0];
            UpdateCurrentNode();
            Debug.Log("Map generated with " + layers + " layers.");
        }

        public override void Exit()
        {
            window.targetLocation = new Vector2(0, 750);
        }
    }
}