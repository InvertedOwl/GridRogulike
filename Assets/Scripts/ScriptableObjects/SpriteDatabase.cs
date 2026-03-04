using System.Collections.Generic;
using UnityEngine;

namespace ScriptableObjects
{

    [CreateAssetMenu(fileName = "SpriteDatabase", menuName = "Game/Sprite Database")]
    public class SpriteDatabase : ScriptableObject
    {
        [System.Serializable]
        public struct SpriteInfo
        {
            public string key;
            public Sprite sprite;
        }

        [SerializeField] private List<SpriteInfo> sprites = new List<SpriteInfo>();

        private Dictionary<string, SpriteInfo> _lookup;

        private void BuildLookup()
        {
            _lookup = new Dictionary<string, SpriteInfo>();
            foreach (var entry in sprites)
                _lookup[entry.key] = entry;
        }

        public SpriteInfo? Get(string key)
        {
            if (_lookup == null) BuildLookup();
            return _lookup.TryGetValue(key, out var panel) ? panel : null;
        }
        
        public bool HasKey(string key)
        {
            if (_lookup == null) BuildLookup();
            return _lookup.ContainsKey(key);
        }
    }
}