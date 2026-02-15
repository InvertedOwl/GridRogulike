using System;
using System.Collections.Generic;
using UnityEngine;


namespace Grid
{
    [Serializable]
    public class MapEntry
    {
        public Vector2Int key;
        public string value;
    }

    [Serializable]
    public class MapData
    {
        public List<MapEntry> entries = new List<MapEntry>();

        public Dictionary<Vector2Int, string> ToDictionary()
        {
            Dictionary<Vector2Int, string> dict = new Dictionary<Vector2Int, string>();

            foreach (var entry in entries)
            {
                dict[entry.key] = entry.value;
            }

            return dict;
        }
    }

}