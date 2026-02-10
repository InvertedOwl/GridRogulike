using System;

namespace Cards.CardList
{
    public class CardEntry
    {
        public Card LocalCard;
        public StartingDeckEntry[] StartingDecks;

        public CardEntry(Card card, StartingDeckEntry[] startingDecks = null)
        {
            LocalCard = card;
            StartingDecks = startingDecks ?? Array.Empty<StartingDeckEntry>();
        }
    }
}