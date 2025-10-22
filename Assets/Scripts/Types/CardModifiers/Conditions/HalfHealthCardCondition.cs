using Cards;
using Entities;
using StateManager;

namespace Types.CardModifiers.Conditions
{
    public class HalfHealthCardCondition: AbstractCardCondition
    {
        public HalfHealthCardCondition()
        {
            this.ConditionText = "On Half Health: ";
        }
        
        public override bool Condition(Card card)
        {
            Player player = GameStateManager.Instance.GetCurrent<PlayingState>().player;
            return player.Health < player.initialHealth/2;
        }
    }
}