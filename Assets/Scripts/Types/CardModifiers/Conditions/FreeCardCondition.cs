using Cards;
using Entities;
using StateManager;

namespace Types.CardModifiers.Conditions
{
    public class FreeCardCondition: AbstractCardCondition
    {
        public FreeCardCondition()
        {
            this.ConditionText = "Card Has 0 Cost: ";
        }
        
        public override bool Condition(Card card)
        {
            if (!card.isReal)
                return false;

            return card.Cost == 0;
        }
    }
}
