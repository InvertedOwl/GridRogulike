using Cards;
using Cards.Actions;
using Entities;
using StateManager;

namespace Types.CardModifiers.Conditions
{
    public class ShieldingCondition: AbstractCardCondition
    {
        public ShieldingCondition()
        {
            this.ConditionText = "Shielding";
        }
        
        public override bool Condition(Card card)
        {
            foreach (AbstractAction action in card.Actions)
            {
                if (action is ShieldAction)
                {
                        return true;
                }
            }
            
            return false;
        }
    }
}