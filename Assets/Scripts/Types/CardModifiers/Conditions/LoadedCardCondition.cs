using Cards;
using Entities;
using StateManager;

namespace Types.CardModifiers.Conditions
{
    public class LoadedCardCondition: AbstractCardCondition
    {
        public LoadedCardCondition()
        {
            this.ConditionText = "Card Has 3 Actions: ";
        }
        
        public override bool Condition(Card card)
        {
            return card.Actions.Count == 3;
        }
    }
}