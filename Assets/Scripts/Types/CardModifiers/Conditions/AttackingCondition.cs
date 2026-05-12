using Cards;
using System.Collections.Generic;
using Cards.CardEvents;

namespace Types.CardModifiers.Conditions
{
    public class AttackingCondition: AbstractCardCondition
    {
        public AttackingCondition()
        {
            this.ConditionText = "Attacking: ";
        }
        
        public override bool Condition(Card card, List<AbstractCardEvent> eventQueue)
        {
            foreach (AbstractCardEvent cardEvent in eventQueue)
            {
                if (cardEvent is AttackCardEvent)
                    return true;
            }
            
            return false;
        }
    }
}
