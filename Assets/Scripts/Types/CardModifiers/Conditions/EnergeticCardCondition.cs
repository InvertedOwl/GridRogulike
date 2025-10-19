using Cards;
using Entities;
using StateManager;

namespace Types.CardModifiers.Conditions
{
    public class EnergeticCardCondition: AbstractCardCondition
    {
        public EnergeticCardCondition()
        {
            this.ConditionText = "Max Energy: ";
        }
        
        public override bool Condition(Card card)
        {
            return RunInfo.Instance.CurrentEnergy >= RunInfo.Instance.MaxEnergy;
        }
    }
}