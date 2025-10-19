using System;
using System.Collections.Generic;
using Cards.CardEvents;

namespace Types.CardModifiers.Modifiers
{
    public class MoreDamageCardModifier: AbstractCardModifier
    {
        public MoreDamageCardModifier()
        {
            this.ModifierText = "x1.5 Damage";
        }
        
        public override List<AbstractCardEvent> Modify(List<AbstractCardEvent> eventList)
        {
            foreach (AbstractCardEvent cardEvent in eventList)
            {
                if (cardEvent is AttackCardEvent)
                {
                    AttackCardEvent attackCardEvent = (AttackCardEvent)cardEvent;
                    attackCardEvent.amount = (int) MathF.Floor(attackCardEvent.amount * 1.5f);
                }
            }

            return eventList;
        }
    }
}