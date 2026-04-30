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
                    string direction = ((AttackAction)action).Direction.ToLower();
                    if (direction.Contains("n") || direction.Contains("s"))
                    {
                        return true;
                    }
                }
            }
            
            return false;
        }
    }
}
