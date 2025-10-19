using System;
using System.Collections.Generic;
using Cards.CardEvents;
using StateManager;

namespace Types.CardModifiers.Modifiers
{
    public class HealCardModifier : AbstractCardModifier
    {
        public HealCardModifier()
        {
            this.ModifierText = "Heal 5.";
        }
        
        public override List<AbstractCardEvent> Modify(List<AbstractCardEvent> cardEvent)
        {
            GameStateManager.Instance.GetCurrent<PlayingState>().player.health += 5;
            return cardEvent;
        }
    }
}