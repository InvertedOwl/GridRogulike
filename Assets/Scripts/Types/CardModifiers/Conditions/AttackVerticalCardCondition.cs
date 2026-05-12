using System.Collections.Generic;
using Cards;
using Cards.CardEvents;

namespace Types.CardModifiers.Conditions
{
    public class AttackVerticalCardCondition: AbstractCardCondition
    {
        public AttackVerticalCardCondition()
        {
            this.ConditionText = "Attacking Vertically: ";
        }
        
        public override bool Condition(Card card, List<AbstractCardEvent> eventQueue)
        {
            foreach (AbstractCardEvent cardEvent in eventQueue)
            {
                if (cardEvent is not AttackCardEvent attackCardEvent ||
                    attackCardEvent.usePosition ||
                    string.IsNullOrEmpty(attackCardEvent.direction))
                    continue;

                string direction = attackCardEvent.direction.ToLower();
                if (direction.Contains("n") || direction.Contains("s"))
                    return true;
            }
            
            return false;
        }
    }
}
