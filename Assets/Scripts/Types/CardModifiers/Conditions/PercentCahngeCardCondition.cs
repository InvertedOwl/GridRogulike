using Cards;
using Cards.Actions;
using Entities;
using StateManager;

namespace Types.CardModifiers.Conditions
{
    public class PercentChanceCardCondition: AbstractCardCondition
    {
        
        private int _percentChance;
        public PercentChanceCardCondition(int percentChance)
        {
            this._percentChance = percentChance;
            this.ConditionText = percentChance + "% Chance: ";
        }
        
        public override bool Condition(Card card)
        {
            if (card.cardRandom.Next(100) < _percentChance)
            {
                return true;
            }
            
            return false;
        }
    }
}