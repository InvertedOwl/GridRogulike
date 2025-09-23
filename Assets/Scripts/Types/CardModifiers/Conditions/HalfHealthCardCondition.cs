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
        
        public override bool Condition()
        {
            Player player = GameStateManager.Instance.GetCurrent<PlayingState>().player;
            return player.health < player.initialHealth/2;
        }
    }
}