using System;

namespace Cards.CardList
{
    public class CardEntry
    {
        public Card LocalCard;
        public StartingDeckEntry[] StartingDecks;
        public bool ShowUpInShop;

        public CardEntry(Card card, StartingDeckEntry[] startingDecks = null, bool showUpInShop = true)
        {
            LocalCard = card;
            StartingDecks = startingDecks ?? Array.Empty<StartingDeckEntry>();
            ShowUpInShop = showUpInShop;
        }
    }
}