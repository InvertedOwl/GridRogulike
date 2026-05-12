using System.Collections.Generic;
using Cards;
using Cards.CardEvents;

namespace Types.CardModifiers.Conditions
{
    public class AttackRightCardCondition: AbstractCardCondition
    {
        public AttackRightCardCondition()
        {
            this.ConditionText = "Attacking East ";
        }
        
        public override bool Condition(Card card, List<AbstractCardEvent> eventQueue)
        {
            foreach (AbstractCardEvent cardEvent in eventQueue)
            {
                if (cardEvent is AttackCardEvent attackCardEvent &&
                    !attackCardEvent.usePosition &&
                    !string.IsNullOrEmpty(attackCardEvent.direction) &&
                    attackCardEvent.direction.ToLower().Contains("e"))
                    return true;
            }
            
            return false;
        }
    }
}
