using Entities;
using Grid;
using StateManager;

namespace Cards.CardEvents
{
    public class GainMoneyCardEvent: AbstractCardEvent
    {
        public int amount;
        

        public GainMoneyCardEvent(int amount)
        {
            this.amount = amount;
        }

        
        public override void Activate(AbstractEntity entity)
        {
            RunInfo.Instance.Money += amount;
            
        }
    }
}