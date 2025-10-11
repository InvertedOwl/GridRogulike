using System;

namespace Cards.CardList
{
    public class CardEntry
    {
        public Card LocalCard;
        public StartingDecks[] StartingDecks;

        public CardEntry(Card card, StartingDecks[] startingDecks = null)
        {
            LocalCard = card;
            StartingDecks = startingDecks ?? Array.Empty<StartingDecks>();
        }
    }
}