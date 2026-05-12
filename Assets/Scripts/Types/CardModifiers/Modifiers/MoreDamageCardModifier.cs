using System;
using System.Collections.Generic;
using Cards.CardEvents;

namespace Types.CardModifiers.Modifiers
{
    public class MoreDamageCardModifier: AbstractCardModifier
    {
        public MoreDamageCardModifier()
        {
            this.ModifierText = "+3 Damage";
        }
        
        public override List<AbstractCardEvent> Modify(List<AbstractCardEvent> eventList)
        {
            foreach (AbstractCardEvent cardEvent in eventList)
            {
                if (cardEvent is AttackCardEvent)
                {
                    AttackCardEvent attackCardEvent = (AttackCardEvent)cardEvent;
                    attackCardEvent.amount += 3;
                }
            }

            return eventList;
        }
    }
}
