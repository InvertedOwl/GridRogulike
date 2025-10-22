using Cards;
using Entities;
using StateManager;

namespace Types.CardModifiers.Conditions
{
    public class HighHealthCardCondition: AbstractCardCondition
    {
        public HighHealthCardCondition()
        {
            this.ConditionText = "Above 80% Health: ";
        }
        
        public override bool Condition(Card card)
        {
            Player player = GameStateManager.Instance.GetCurrent<PlayingState>().player;
            return (player.Health/player.initialHealth)>.8f;
        }
    }
}