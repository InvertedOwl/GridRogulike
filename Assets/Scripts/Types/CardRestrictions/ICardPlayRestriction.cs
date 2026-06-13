using System.Collections.Generic;
using Cards;
using Cards.CardEvents;
using StateManager;

namespace Types.CardRestrictions
{
    public interface ICardPlayRestriction
    {
        string Reason { get; }
        bool Blocks(Card card, List<AbstractCardEvent> previewEvents, PlayingState playingState);
    }
}
