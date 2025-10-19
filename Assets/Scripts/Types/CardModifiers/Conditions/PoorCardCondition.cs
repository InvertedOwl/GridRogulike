using Cards;
using Entities;
using StateManager;

namespace Types.CardModifiers.Conditions
{
    public class PoorCardCondition: AbstractCardCondition
    {
        public PoorCardCondition()
        {
            this.ConditionText = "Less Than $5: ";
        }
        
        public override bool Condition(Card card)
        {
            return RunInfo.Instance.Money < 5;
        }
    }
}