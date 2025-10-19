using Cards;
using Cards.Actions;
using Entities;
using StateManager;

namespace Types.CardModifiers.Conditions
{
    public class AttackVerticalCardCondition: AbstractCardCondition
    {
        public AttackVerticalCardCondition()
        {
            this.ConditionText = "Attacking Vertically: ";
        }
        
        public override bool Condition(Card card)
        {
            foreach (AbstractAction action in card.Actions)
            {
                if (action is AttackAction)
                {
                    if (((AttackAction)action).Direction.ToLower() == "n" ||
                        ((AttackAction)action).Direction.ToLower() == "s")
                    {
                        return true;
                    }
                }
            }
            
            return false;
        }
    }
}