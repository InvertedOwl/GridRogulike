using System;
using System.Collections.Generic;
using Cards.CardEvents;

namespace Types.CardModifiers.Modifiers
{
    public class DrawCardModifier : AbstractCardModifier
    {
        public DrawCardModifier()
        {
            this.ModifierText = "Draw a card";
        }
        
        public override List<AbstractCardEvent> Modify(List<AbstractCardEvent> cardEvent)
        {
            Deck.Instance.FullDrawHand(1);
            return cardEvent;
        }
    }
}