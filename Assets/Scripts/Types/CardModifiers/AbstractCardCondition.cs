using Cards;
using Cards.CardEvents;

namespace Types.CardModifiers
{
    public abstract class AbstractCardCondition
    {
        public string ConditionText;
        public abstract bool Condition(Card card);
    }
}