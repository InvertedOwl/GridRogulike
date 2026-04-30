using System;
using System.Collections.Generic;
using Cards.CardEvents;
using Grid;

namespace Types.CardModifiers.Modifiers
{
    public class RandomDamageCardModifier : AbstractCardModifier
    {
        public RandomDamageCardModifier()
        {
            this.ModifierText = "Attack for 15 Damage in a random direction.";
        }
        
        public override List<AbstractCardEvent> Modify(List<AbstractCardEvent> cardEvent)
        {
            string[] directions = HexGridManager.HexDirections;
            cardEvent.Add(new AttackCardEvent(1, directions[new Random().Next(0, directions.Length)], 15));
            return cardEvent;
        }
    }
}
