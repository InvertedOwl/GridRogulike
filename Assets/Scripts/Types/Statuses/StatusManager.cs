using System;
using System.Collections.Generic;
using System.Linq;
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
            CacheIcon<FallStatus>("Fall");
            CacheIcon<ShockedStatus>("Shocked");
            CacheIcon<EnergeticStatus>("Energetic");
            CacheIcon<ExcitedStatus>("Excited");
            CacheIcon<FrozenStatus>("Frost");
            CacheIcon<SleepyStatus>("Sleepy");
            CacheIcon<RangedStatus>("Ranged");
            CacheIcon<HasteStatus>("Haste");
            CacheIcon<BlindStatus>("Blind");
            CacheIcon<VolatileStatus>("Volatile");
            CacheIcon<DizzyStatus>("Dizzy");
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
            
            foreach (var status in statuses.ToList())
            {
                if (status.Amount <= 0)
                {
                    toRemove.Add(status);
                    continue;
                }

                var type = status.GetType();
                if (_iconCache.TryGetValue(type, out var icon))
                {
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
