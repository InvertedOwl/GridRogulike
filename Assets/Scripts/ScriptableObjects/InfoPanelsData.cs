using System.Collections.Generic;
using UnityEngine;

namespace ScriptableObjects
{

    [CreateAssetMenu(fileName = "InfoPanelDatabase", menuName = "Game/Info Panel Database")]
    public class InfoPanelsData : ScriptableObject
    {
        [System.Serializable]
        public struct InfoPanel
        {
            public string key;
            public Sprite sprite;
            public Color color;
            public string title;
            public string description;
        }

        [SerializeField] private List<InfoPanel> infoPanels = new List<InfoPanel>();

        private Dictionary<string, InfoPanel> _lookup;

        private void BuildLookup()
        {
            _lookup = new Dictionary<string, InfoPanel>();
            foreach (var entry in infoPanels)
                _lookup[entry.key] = entry;
        }

        public InfoPanel? Get(string key)
        {
            if (_lookup == null) BuildLookup();
            return _lookup.TryGetValue(key, out var panel) ? panel : null;
        }
        
        public bool HasKey(string key)
        {
            if (_lookup == null) BuildLookup();
            return _lookup.ContainsKey(key);
        }
        
        public List<InfoPanel> GetAllInString(string text)
        {
            if (_lookup == null) BuildLookup();
            var results = new List<InfoPanel>();
            foreach (var kvp in _lookup)
            {
                if (text.IndexOf(kvp.Key, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    results.Add(kvp.Value);
            }
            return results;
        }
    }
}