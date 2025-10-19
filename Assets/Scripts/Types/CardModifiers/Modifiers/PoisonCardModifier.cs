using System;
using System.Collections.Generic;
using Cards.CardEvents;
using Types.Statuses;

namespace Types.CardModifiers.Modifiers
{
    public class PoisonCardModifier: AbstractCardModifier
    {
        public PoisonCardModifier()
        {
            this.ModifierText = "All attacks apply 1 Poison";
        }
        
        public override List<AbstractCardEvent> Modify(List<AbstractCardEvent> eventList)
        {
            foreach (AbstractCardEvent cardEvent in eventList)
            {
                if (cardEvent is AttackCardEvent)
                {
                    AttackCardEvent attackCardEvent = (AttackCardEvent)cardEvent;
                    attackCardEvent.status = new PoisonStatus(1);
                }
            }

            return eventList;
        }
    }
}