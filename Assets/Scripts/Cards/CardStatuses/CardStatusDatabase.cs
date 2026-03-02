using System;
using System.Collections.Generic;
using System.Linq;
using Cards.CardEvents;
using StateManager;
using UnityEngine;

namespace Cards.CardStatuses
{
    [CreateAssetMenu(fileName = "CardStatusDatabase", menuName = "Game/Card Status Database")]
    public class CardStatusDatabase : ScriptableObject, ISerializationCallbackReceiver
    {
        [System.Serializable]
        public class CardStatus
        {
            public string key;
            public Sprite sprite;
            public Color color;
            [NonSerialized] public Func<List<AbstractCardEvent>, Card, List<AbstractCardEvent>> ModifyPlay;
            [NonSerialized] public Func<Card, bool> OnDiscard;
            [NonSerialized] public Action<Card> NotPlayed;

            public CardStatus() { }

            public CardStatus(
                string key,
                Sprite sprite,
                Color color,
                Func<List<AbstractCardEvent>, Card, List<AbstractCardEvent>> modifyPlay = null,
                Func<Card, bool> onDiscard = null,
                Action<Card> notPlayed = null)
            {
                this.key = key;
                this.sprite = sprite;
                this.color = color;
                this.ModifyPlay = modifyPlay;
                this.OnDiscard = onDiscard;
                this.NotPlayed = notPlayed;
            }
        }

        private static readonly Dictionary<string, Action<CardStatus>> StatusLogic = new()
        {
            ["sticky"] = s => { s.OnDiscard = card => false; },
            ["burning"] = s =>
            {
                s.NotPlayed = card =>
                {
                    GameStateManager.Instance.GetCurrent<PlayingState>().player.Damage(5, null);
                };
            },
            ["bramble"] = s =>
            {
                s.ModifyPlay = (List<AbstractCardEvent> events, Card card) =>
                {
                    GameStateManager.Instance.GetCurrent<PlayingState>().player.Damage(5, null);
                    return events;
                };
            }
        };

        [SerializeField] private List<CardStatus> cardStatuses = new();

        private Dictionary<string, CardStatus> _lookup;

        public void OnAfterDeserialize()
        {
            _lookup = null;
        }

        public void OnBeforeSerialize() { }

        private void SyncKeys()
        {
            var existing = cardStatuses.ToDictionary(s => s.key, s => s);
            cardStatuses.Clear();

            foreach (var key in StatusLogic.Keys)
            {
                if (existing.TryGetValue(key, out var entry))
                    entry.key = key;
                else
                    entry = new CardStatus(key, null, Color.white);

                cardStatuses.Add(entry);
            }
        }

        private void BuildLookup()
        {
            SyncKeys();
            _lookup = new Dictionary<string, CardStatus>();
            foreach (var data in cardStatuses)
            {
                if (StatusLogic.TryGetValue(data.key, out var apply))
                    apply(data);
                _lookup[data.key] = data;
            }
        }

        public CardStatus? Get(string key)
        {
            if (_lookup == null) BuildLookup();
            return _lookup.TryGetValue(key, out var status) ? status : null;
        }
    }
}