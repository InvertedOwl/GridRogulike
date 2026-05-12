using System.Collections.Generic;
using Cards;
using Cards.CardEvents;

namespace Types.CardModifiers.Conditions
{
    public class AttackLeftCardCondition: AbstractCardCondition
    {
        public AttackLeftCardCondition()
        {
            this.ConditionText = "Attacking West: ";
        }
        
        public override bool Condition(Card card, List<AbstractCardEvent> eventQueue)
        {
            foreach (AbstractCardEvent cardEvent in eventQueue)
            {
                if (cardEvent is AttackCardEvent attackCardEvent &&
                    !attackCardEvent.usePosition &&
                    !string.IsNullOrEmpty(attackCardEvent.direction) &&
                    attackCardEvent.direction.ToLower().Contains("w"))
                    return true;
            }
            
            return false;
        }
    }
}
