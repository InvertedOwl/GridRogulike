using Cards;
using Entities;
using StateManager;

namespace Types.CardModifiers.Conditions
{
    public class ShieldedCardCondition: AbstractCardCondition
    {
        public ShieldedCardCondition()
        {
            this.ConditionText = "Above 30 Shield: ";
        }
        
        public override bool Condition(Card card)
        {
            Player player = GameStateManager.Instance.GetCurrent<PlayingState>().player;
            return player.Shield > 30;
        }
    }
}