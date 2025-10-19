using Cards;
using Entities;
using StateManager;

namespace Types.CardModifiers.Conditions
{
    public class ExpensiveCardCondition: AbstractCardCondition
    {
        public ExpensiveCardCondition()
        {
            this.ConditionText = "Card Has 3+ Cost: ";
        }
        
        public override bool Condition(Card card)
        {
            return card.Cost >= 3;
        }
    }
}