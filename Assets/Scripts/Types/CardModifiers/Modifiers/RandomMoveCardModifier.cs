using System;
using System.Collections.Generic;
using Cards.CardEvents;

namespace Types.CardModifiers.Modifiers
{
    public class RandomMoveCardModifier : AbstractCardModifier
    {
        public RandomMoveCardModifier()
        {
            this.ModifierText = "Move in a random direction.";
        }
        
        public override List<AbstractCardEvent> Modify(List<AbstractCardEvent> cardEvent)
        {
            string[] directions = new[] { "n", "s", "ne", "nw", "se", "sw" };
            cardEvent.Add(new MoveCardEvent(1, directions[new Random().Next(0, directions.Length)]));
            return cardEvent;
        }
    }
}