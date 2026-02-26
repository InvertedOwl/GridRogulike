using Cards;
using Cards.Actions;
using Entities;
using StateManager;

namespace Types.CardModifiers.Conditions
{
    public class AttackingCondition: AbstractCardCondition
    {
        public AttackingCondition()
        {
            this.ConditionText = "Attacking West: ";
        }
        
        public override bool Condition(Card card)
        {
            foreach (AbstractAction action in card.Actions)
            {
                if (action is AttackAction)
                {
                        return true;
                }
            }
            
            return false;
        }
    }
}