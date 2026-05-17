using System.Collections.Generic;
using Cards.CardEvents;
using Grid;

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
            string[] directions = HexGridManager.HexDirections;
            RandomState modifierRandom = GetModifierRandom(previewMode);
            cardEvent.Add(new MoveCardEvent(1, directions[modifierRandom.Next(0, directions.Length)]));
            return cardEvent;
        }
    }
}
