using Cards;
using Cards.Actions;
using Entities;
using StateManager;

namespace Types.CardModifiers.Conditions
{
    public class AttackLeftCardCondition: AbstractCardCondition
    {
        public AttackLeftCardCondition()
        {
            this.ConditionText = "Attacking West: ";
        }
        
        public override bool Condition(Card card)
        {
            foreach (AbstractAction action in card.Actions)
            {
                if (action is AttackAction)
                {
                    if (((AttackAction)action).Direction.ToLower().Contains("w"))
                    {
                        return true;
                    }
                }
            }
            
            return false;
        }
    }
}