using System;
using Types.CardEvents;

namespace Types.CardModifiers
{
    public class DoNothingCardModifier : AbstractCardModifier
    {
        public DoNothingCardModifier()
        {
            this.ModifierText = "No Effect";
            this.Rarity = Rarity.Common;
        }
        
        public override AbstractCardEvent Modify(AbstractCardEvent cardEvent)
        {
            return cardEvent;
        }
    }
}