namespace Cards.CardList
{
    public class StartingDeckEntry
    {
        public StartingDecks startingDeck;
        public int numberOfCards;

        public StartingDeckEntry(StartingDecks startingDeck, int numberOfCards)
        {
            this.startingDeck = startingDeck;
            this.numberOfCards = numberOfCards;
        }
    }
}