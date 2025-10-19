using Cards;
using Cards.Actions;
using Entities;
using StateManager;

namespace Types.CardModifiers.Conditions
{
    public class AttackRightCardCondition: AbstractCardCondition
    {
        public AttackRightCardCondition()
        {
            this.ConditionText = "Attacking East ";
        }
        
        public override bool Condition(Card card)
        {
            foreach (AbstractAction action in card.Actions)
            {
                if (action is AttackAction)
                {
                    if (((AttackAction)action).Direction.ToLower().Contains("e"))
                    {
                        return true;
                    }
                }
            }
            
            return false;
        }
    }
}