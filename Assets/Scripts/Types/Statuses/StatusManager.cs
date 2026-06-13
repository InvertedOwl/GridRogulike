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
            CacheIcon<BuffedStatus>("Buffed");
            CacheIcon<DazedStatus>("Dazed");
            CacheIcon<RestlessStatus>("Restless");
        }

        private void CacheIcon<T>(string name) where T : AbstractStatus
        {
            if (goList == null || !goList.HasValue(name))
                return;

            var icon = goList.GetValue(name);
            if (icon == null)
                return;

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
                    if (_textCache.TryGetValue(type, out TextMeshPro text) && text != null)
                        text.text = (Math.Max(status.Amount, 1)).ToString();
                }
            }

            foreach (var status in toRemove)
            {
                statusList.Remove(status);
            }
             
        }

        public void ClearStatuses()
        {
            statusList.Clear();
            UpdateStatuses(statusList);
        }


        public void Update()
        {
            UpdateStatuses(statusList);
        }
    }
}
