using System.Collections.Generic;
using Map;
using UnityEngine;
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


        private List<List<MapNode>> mapLayers = new List<List<MapNode>>();
        
        public override void Enter()
        {
            window.targetLocation = new Vector2(0, 0);
            if (!hasSetup)
            {
                hasSetup = true;
                SetupMap();
            }
        } 


    public void GenerateMap()
    {
        mapLayers.Clear();

        for (int i = 0; i < layers; i++)
        {
            int nodeCount = Random.Range(minNodesPerLayer, maxNodesPerLayer + 1);
            List<MapNode> layer = new List<MapNode>();

            float totalHeight = (nodeCount - 1) * nodeSpacing;
            float startY = -totalHeight / 2f;

            for (int j = 0; j < nodeCount; j++)
            {
                Vector3 pos = new Vector3(
                    i * layerSpacing, 
                    startY + j * nodeSpacing + Random.Range(-1, 1), 
                    0f
                );

                GameObject nodeObj = Instantiate(GetRandomTargetForLayer(i), pos, Quaternion.identity, transform);
                MapNode node = nodeObj.GetComponent<MapNode>();
                nodeObj.transform.SetParent(goList.GetValue("anchor").transform);
                layer.Add(node);
            }

            mapLayers.Add(layer);
        }

        for (int i = 0; i < layers - 1; i++)
        {
            List<MapNode> currentLayer = mapLayers[i];
            List<MapNode> nextLayer = mapLayers[i + 1];

            foreach (MapNode node in currentLayer)
            {
                MapNode guaranteed = nextLayer[Random.Range(0, nextLayer.Count)];
                node.children.Add(guaranteed);

                foreach (MapNode candidate in nextLayer)
                {
                    if (candidate != guaranteed && Random.value < 0.3f) // 30% chance
                    {
                        node.children.Add(candidate);
                    }
                }
            }
        }

        Debug.Log("Map generated with " + layers + " layers.");
    }

    private GameObject GetRandomTargetForLayer(int layerIndex)
    {
        if (layerIndex < layers - 1)
        {
            float random = Random.value;
            if (random < 0.3f)
                return goList.GetValue("event");
            if (random < 0.6f && random >= 0.3f)
                goList.GetValue("shop");
            if (random > 0.6f)
                goList.GetValue("enemy");
        }
        
        // TODO: replace with boss when I make said boss
        return goList.GetValue("enemy");
    }

        private void SetupMap()
        {
            GenerateMap();
            Debug.Log("Map generated with " + layers + " layers.");
        }

        public override void Exit()
        {
            window.targetLocation = new Vector2(0, 750);
        }
    }
}