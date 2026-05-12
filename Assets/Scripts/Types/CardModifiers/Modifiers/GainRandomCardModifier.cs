using System;
using System.Collections.Generic;
using Cards.CardEvents;
using Types;
namespace Types.CardModifiers.Modifiers
{
    public class GainRandomCardModifier : AbstractCardModifier
    {
        public GainRandomCardModifier()
        {
            this.ModifierText = "Gain a random uncommon card for the rest of the battle.";
        }
        
        public override List<AbstractCardEvent> Modify(List<AbstractCardEvent> cardEvent)
        {
            cardEvent.Add(new GainRandomCardEvent(Rarity.Uncommon, cardModifierRandom));
            return cardEvent;
        }
    }
}
