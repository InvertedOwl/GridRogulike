using System;
using System.Collections.Generic;
using System.Linq;
using Cards.CardEvents;

namespace Cards.Actions
{
    public readonly struct CardEventPreviewSnapshot
    {
        public readonly int SourceActionIndex;
        public readonly Dictionary<string, PreviewValue> Values;

        public CardEventPreviewSnapshot(AbstractCardEvent cardEvent)
        {
            SourceActionIndex = cardEvent.PreviewSourceActionIndex;
            Values = cardEvent.GetPreviewValues();
        }

        public bool TryGetInt(string key, out int value)
        {
            if (Values.TryGetValue(key, out PreviewValue previewValue))
                return previewValue.TryGetInt(out value);

            value = 0;
            return false;
        }
    }

    public class CardActionPreview
    {
        private readonly int _actionIndex;
        private readonly List<CardEventPreviewSnapshot> _baseEvents;
        private readonly List<CardEventPreviewSnapshot> _modifiedEvents;

        public CardActionPreview(
            int actionIndex,
            List<CardEventPreviewSnapshot> baseEvents,
            List<AbstractCardEvent> modifiedEvents)
        {
            _actionIndex = actionIndex;
            _baseEvents = baseEvents;
            _modifiedEvents = modifiedEvents
                .Select(cardEvent => new CardEventPreviewSnapshot(cardEvent))
                .ToList();
        }

        public int GetFirstDelta(string key)
        {
            List<CardEventPreviewSnapshot> baseEvents = BaseEvents(key).ToList();
            if (baseEvents.Count == 0 || !baseEvents[0].TryGetInt(key, out int baseAmount))
                return 0;

            int modifiedAmount = ModifiedEvents(key)
                .Select(snapshot => snapshot.TryGetInt(key, out int amount) ? amount : 0)
                .FirstOrDefault();

            return modifiedAmount - baseAmount;
        }

        public int GetTotalDelta(string key)
        {
            int baseAmount = BaseEvents(key)
                .Sum(snapshot => snapshot.TryGetInt(key, out int amount) ? amount : 0);
            int modifiedAmount = ModifiedEvents(key)
                .Sum(snapshot => snapshot.TryGetInt(key, out int amount) ? amount : 0);

            return modifiedAmount - baseAmount;
        }

        public int GetFirstFinalValue(string key, int fallbackBaseValue)
        {
            int delta = GetFirstDelta(key);
            return Math.Max(0, fallbackBaseValue + delta);
        }

        public int GetTotalFinalValue(string key, int fallbackBaseValue)
        {
            int delta = GetTotalDelta(key);
            return Math.Max(0, fallbackBaseValue + delta);
        }

        public bool TryGetFirstModifiedValue(string key, out PreviewValue value)
        {
            foreach (CardEventPreviewSnapshot snapshot in ModifiedEvents(key))
            {
                value = snapshot.Values[key];
                return true;
            }

            value = default(PreviewValue);
            return false;
        }

        public string FormatValue(string prefix, int baseValue, int finalValue)
        {
            int delta = finalValue - baseValue;
            if (delta == 0)
                return prefix + baseValue;

            string sign = delta > 0 ? "+" : "";
            return prefix + baseValue + " <color=green>" + sign + delta + "</color>";
        }

        private IEnumerable<CardEventPreviewSnapshot> BaseEvents(string key)
        {
            return _baseEvents.Where(e => e.SourceActionIndex == _actionIndex && e.Values.ContainsKey(key));
        }

        private IEnumerable<CardEventPreviewSnapshot> ModifiedEvents(string key)
        {
            return _modifiedEvents.Where(e => e.SourceActionIndex == _actionIndex && e.Values.ContainsKey(key));
        }
    }
}
