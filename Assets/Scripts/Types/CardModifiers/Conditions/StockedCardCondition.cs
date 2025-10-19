using Cards;
using Entities;
using StateManager;

namespace Types.CardModifiers.Conditions
{
    public class StockedCardCondition: AbstractCardCondition
    {
        public StockedCardCondition()
        {
            this.ConditionText = "6+ Cards In Hand: ";
        }
        
        public override bool Condition(Card card)
        {
            return Deck.Instance.Hand.Count >= 6;
        }
    }
}