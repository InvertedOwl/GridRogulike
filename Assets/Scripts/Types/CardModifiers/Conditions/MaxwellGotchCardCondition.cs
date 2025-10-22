using Cards;
using Entities;
using StateManager;
using Util;

namespace Types.CardModifiers.Conditions
{
    public class MaxwellGotchCardCondition: AbstractCardCondition
    {
        public MaxwellGotchCardCondition()
        {
            this.ConditionText = "3 Cards Played: ";
        }
        
        public override bool Condition(Card card)
        {
            return BattleStats.CardsPlayedThisBattle >= 3;
        }
    }
}