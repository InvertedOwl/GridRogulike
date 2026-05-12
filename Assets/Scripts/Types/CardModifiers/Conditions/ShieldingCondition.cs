using System.Collections.Generic;
using Cards;
using Cards.CardEvents;

namespace Types.CardModifiers.Conditions
{
    public class ShieldingCondition: AbstractCardCondition
    {
        public ShieldingCondition()
        {
            this.ConditionText = "Shielding";
        }
        
        public override bool Condition(Card card, List<AbstractCardEvent> eventQueue)
        {
            foreach (AbstractCardEvent cardEvent in eventQueue)
            {
                if (cardEvent is ShieldCardEvent)
                    return true;
            }
            
            return false;
        }
    }
}
