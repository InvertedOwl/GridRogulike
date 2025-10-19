using System;
using System.Collections.Generic;
using Cards.CardEvents;
using StateManager;

namespace Types.CardModifiers.Modifiers
{
    public class ShieldCardModifier : AbstractCardModifier
    {
        public ShieldCardModifier()
        {
            this.ModifierText = "Gain 15 shield.";
        }
        
        public override List<AbstractCardEvent> Modify(List<AbstractCardEvent> cardEvent)
        {
            cardEvent.Add(new ShieldCardEvent(15));
            return cardEvent;
        }
    }
}