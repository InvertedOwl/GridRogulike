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
            return Modify(cardEvent, previewMode: false);
        }

        public override List<AbstractCardEvent> Modify(List<AbstractCardEvent> cardEvent, bool previewMode)
        {
            cardEvent.Add(new RandomMoveCardEvent(1, GetModifierRandom(previewMode)));
            return cardEvent;
        }
    }
}
