using System;
using System.Collections.Generic;
using Cards.CardEvents;

namespace Types.CardModifiers.Modifiers
{
    public class GainEnergyCardModifier : AbstractCardModifier
    {
        public GainEnergyCardModifier()
        {
            this.ModifierText = "Gain +1 Energy";
        }
        
        public override List<AbstractCardEvent> Modify(List<AbstractCardEvent> cardEvent)
        {
            cardEvent.Add(new GainEnergyCardEvent(1));
            return cardEvent;
        }
    }
}