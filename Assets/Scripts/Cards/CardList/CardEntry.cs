namespace Cards.CardList
{
    public class CardEntry
    {
        public Card LocalCard;
        public StartingDecks[] StartingDecks;

        public CardEntry(Card card, StartingDecks[] startingDecks)
        {
            LocalCard = card;
            StartingDecks = startingDecks;
        }
    }
}