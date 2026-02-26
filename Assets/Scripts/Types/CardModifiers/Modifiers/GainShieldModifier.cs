using System;
using System.Collections.Generic;
using Cards.CardEvents;
using StateManager;

namespace Types.CardModifiers.Modifiers
{
    public class GainShieldModifier : AbstractCardModifier
    {
        public GainShieldModifier()
        {
            this.ModifierText = "Gain 3 Shield";
        }
        
        public override List<AbstractCardEvent> Modify(List<AbstractCardEvent> cardEvent)
        {
            cardEvent.Add(new ShieldCardEvent(3));
            return cardEvent;
        }
    }
}