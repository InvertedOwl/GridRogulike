using System;
using System.Collections.Generic;
using Cards.CardEvents;
using StateManager;

namespace Types.CardModifiers.Modifiers
{
    public class AgainCardModifier : AbstractCardModifier
    {
        public AgainCardModifier()
        {
            this.ModifierText = "Retrigger this card.";
        }
        
        public override List<AbstractCardEvent> Modify(List<AbstractCardEvent> cardEvent)
        {
            cardEvent.AddRange(cardEvent);
            return cardEvent;
        }
    }
}