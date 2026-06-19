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

        public struct InfoPanelMatch
        {
            public InfoPanel infoPanel;
            public int index;
            public int order;

            public InfoPanelMatch(InfoPanel infoPanel, int index, int order)
            {
                this.infoPanel = infoPanel;
                this.index = index;
                this.order = order;
            }
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
            var results = new List<InfoPanel>();
            foreach (InfoPanelMatch match in GetAllMatchesInString(text))
            {
                results.Add(match.infoPanel);
            }
            return results;
        }

        public List<InfoPanelMatch> GetAllMatchesInString(string text)
        {
            if (_lookup == null) BuildLookup();

            List<InfoPanelMatch> results = new List<InfoPanelMatch>();
            if (string.IsNullOrEmpty(text))
                return results;

            for (int i = 0; i < infoPanels.Count; i++)
            {
                InfoPanel infoPanel = infoPanels[i];
                if (string.IsNullOrWhiteSpace(infoPanel.key))
                    continue;

                int index = text.IndexOf(infoPanel.key, System.StringComparison.OrdinalIgnoreCase);
                if (index >= 0)
                    results.Add(new InfoPanelMatch(infoPanel, index, i));
            }

            results.Sort((a, b) =>
            {
                int indexCompare = a.index.CompareTo(b.index);
                return indexCompare != 0 ? indexCompare : a.order.CompareTo(b.order);
            });

            return results;
        }
    }
}
