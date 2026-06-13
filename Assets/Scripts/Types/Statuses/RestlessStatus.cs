using System.Collections.Generic;
using Cards;
using Cards.CardEvents;
using StateManager;
using Types.CardRestrictions;

namespace Types.Statuses
{
    public class RestlessStatus : AbstractStatus, ICardPlayRestriction
    {
        public RestlessStatus(int amount)
        {
            Amount = amount;
        }

        public string Reason => "Must move before attacking.";

        public bool Blocks(Card card, List<AbstractCardEvent> previewEvents, PlayingState playingState)
        {
            if (Amount <= 0)
                return false;

            return new MoveBeforeCardsRestriction(0, true, Reason)
                .Blocks(card, previewEvents, playingState);
        }

        public override List<AbstractCardEvent> Modify(List<AbstractCardEvent> cardEvent)
        {
            return cardEvent;
        }

        public override void OnEndTurn()
        {
        }
    }
}
