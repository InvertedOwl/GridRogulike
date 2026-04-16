using System.Collections.Generic;
using Entities.Enemies;
using UnityEngine;

namespace ScriptableObjects
{

    [CreateAssetMenu(fileName = "EnemiesDatabase", menuName = "Game/Enemies Database")]
    public class EnemiesData : ScriptableObject
    {
        [System.Serializable]
        public struct EnemyEntry
        {
            public string enemyName;
            public GameObject enemyPrefab;
            public EnemyType enemyType;
            public Sprite enemyIconPrefab;
        }

        [SerializeField] private List<EnemyEntry> enemies = new List<EnemyEntry>();

        private Dictionary<string, EnemyEntry> _lookup;

        private void BuildLookup()
        {
            _lookup = new Dictionary<string, EnemyEntry>();
            foreach (var entry in enemies)
                _lookup[entry.enemyName] = entry;
        }

        public EnemyEntry? Get(string key)
        {
            if (_lookup == null) BuildLookup();
            return _lookup.TryGetValue(key, out var panel) ? panel : null;
        }
        
        public bool HasKey(string key)
        {
            if (_lookup == null) BuildLookup();
            return _lookup.ContainsKey(key);
        }
        
        public List<EnemyEntry> GetAllInString(string text)
        {
            if (_lookup == null) BuildLookup();
            var results = new List<EnemyEntry>();
            foreach (var kvp in _lookup)
            {
                if (text.IndexOf(kvp.Key, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    results.Add(kvp.Value);
            }
            return results;
        }
    }
}