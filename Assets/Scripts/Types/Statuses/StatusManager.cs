using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Types.Statuses
{
    public class StatusManager : MonoBehaviour
    {
        public Transform iconParent;
        public GOList goList;
        
        public List<AbstractStatus> statusList = new List<AbstractStatus>();

        private Dictionary<System.Type, GameObject> _iconCache = new();
        private Dictionary<System.Type, TextMeshPro> _textCache = new();

        private void Start()
        {
            CacheIcon<PoisonStatus>("Poison");
            CacheIcon<FrostStatus>("Frost");
        }

        private void CacheIcon<T>(string name) where T : AbstractStatus
        {
            var icon = goList.GetValue(name);
            _iconCache[typeof(T)] = icon;
            _textCache[typeof(T)] = icon.GetComponentInChildren<TextMeshPro>();
            icon.SetActive(false);
        }

        public void UpdateStatuses(List<AbstractStatus> statuses)
        {
            foreach (var kv in _iconCache)
                kv.Value.SetActive(false);

            List<AbstractStatus> toRemove = new List<AbstractStatus>();
            
            foreach (var status in statuses)
            {
                var type = status.GetType();
                if (_iconCache.TryGetValue(type, out var icon))
                {
                    if (status.Amount <= 0)
                    {
                        toRemove.Add(status);
                    }
                    icon.SetActive(true);
                    _textCache[type].text = (Math.Max(status.Amount, 1)).ToString();
                }
            }

            foreach (var status in toRemove)
            {
                statusList.Remove(status);
            }
             
        }


        public void Update()
        {
            UpdateStatuses(statusList);
        }
    }
}