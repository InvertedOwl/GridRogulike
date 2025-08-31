using System.Collections.Generic;
using UnityEngine;

namespace Map
{
    public class MapNode: MonoBehaviour
    {
        public List<MapNode> children = new List<MapNode>();
        public MapTarget target;
        public bool isOn;
    }
}