using System.Collections.Generic;
using Entities;

namespace Cards.CardEvents
{
    public enum PreviewValueType
    {
        Int,
        Float,
        Text,
        Bool
    }

    public readonly struct PreviewValue
    {
        public readonly PreviewValueType Type;
        public readonly int IntValue;
        public readonly float FloatValue;
        public readonly string TextValue;
        public readonly bool BoolValue;

        private PreviewValue(PreviewValueType type, int intValue, float floatValue, string textValue, bool boolValue)
        {
            Type = type;
            IntValue = intValue;
            FloatValue = floatValue;
            TextValue = textValue;
            BoolValue = boolValue;
        }

        public static PreviewValue Int(int value)
        {
            return new PreviewValue(PreviewValueType.Int, value, value, "", false);
        }

        public static PreviewValue Float(float value)
        {
            return new PreviewValue(PreviewValueType.Float, 0, value, "", false);
        }

        public static PreviewValue Text(string value)
        {
            return new PreviewValue(PreviewValueType.Text, 0, 0f, value ?? "", false);
        }

        public static PreviewValue Bool(bool value)
        {
            return new PreviewValue(PreviewValueType.Bool, 0, 0f, "", value);
        }

        public bool TryGetInt(out int value)
        {
            if (Type == PreviewValueType.Int)
            {
                value = IntValue;
                return true;
            }

            value = 0;
            return false;
        }

        public bool TryGetFloat(out float value)
        {
            if (Type == PreviewValueType.Float)
            {
                value = FloatValue;
                return true;
            }

            if (Type == PreviewValueType.Int)
            {
                value = IntValue;
                return true;
            }

            value = 0f;
            return false;
        }

        public bool TryGetText(out string value)
        {
            if (Type == PreviewValueType.Text)
            {
                value = TextValue;
                return true;
            }

            value = "";
            return false;
        }

        public bool TryGetBool(out bool value)
        {
            if (Type == PreviewValueType.Bool)
            {
                value = BoolValue;
                return true;
            }

            value = false;
            return false;
        }
    }

    public static class CardPreviewKeys
    {
        public const string Damage = "damage";
        public const string Distance = "distance";
        public const string Direction = "direction";
        public const string Shield = "shield";
        public const string Energy = "energy";
        public const string Steps = "steps";
        public const string Draw = "draw";
        public const string Heal = "heal";
        public const string Money = "money";
        public const string SelfDamage = "selfDamage";
        public const string StatusAmount = "statusAmount";
        public const string StatusDelta = "statusDelta";
        public const string StatusName = "statusName";
        public const string Discard = "discard";
        public const string DiscardHand = "discardHand";
        public const string Destroy = "destroy";
        public const string Scrap = "scrap";
        public const string Cost = "cost";
        public const string CardCount = "cardCount";
        public const string CardId = "cardId";
        public const string PassiveName = "passiveName";
    }

    public abstract class AbstractCardEvent
    {
        public int PreviewSourceActionIndex = -1;

        public virtual Dictionary<string, PreviewValue> GetPreviewValues()
        {
            return new Dictionary<string, PreviewValue>();
        }

        public virtual CardEventResult ActivateWithResult(AbstractEntity entity, CardEventContext context)
        {
            Activate(entity);
            return new CardEventResult(this);
        }

        public abstract void Activate(AbstractEntity entity);
    }

    public class CardEventContext
    {
        private readonly List<CardEventResult> _results = new List<CardEventResult>();

        public IReadOnlyList<CardEventResult> Results => _results;
        public CardEventResult PreviousResult => _results.Count > 0 ? _results[_results.Count - 1] : null;

        public void Record(CardEventResult result)
        {
            _results.Add(result ?? new CardEventResult(null));
        }
    }

    public class CardEventResult
    {
        public AbstractCardEvent SourceEvent { get; }
        public List<AbstractEntity> DamagedEntities { get; } = new List<AbstractEntity>();
        public List<AbstractEntity> DefeatedEntities { get; } = new List<AbstractEntity>();

        public bool DefeatedEnemy
        {
            get
            {
                foreach (AbstractEntity entity in DefeatedEntities)
                {
                    if (entity != null && entity.entityType == EntityType.Enemy)
                        return true;
                }

                return false;
            }
        }

        public CardEventResult(AbstractCardEvent sourceEvent)
        {
            SourceEvent = sourceEvent;
        }
    }
}
