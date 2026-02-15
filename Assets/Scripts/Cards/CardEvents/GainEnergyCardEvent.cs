using Entities;
using StateManager;

namespace Cards.CardEvents
{
    public class GainEnergyCardEvent : AbstractCardEvent
    {
        private int _amount;
        
        public GainEnergyCardEvent(int amount)
        {
            this._amount = amount;
        }

        
        public override void Activate(AbstractEntity entity)
        {
            if (entity.entityType == EntityType.Player)
                RunInfo.Instance.CurrentEnergy += this._amount;
        }
    }
}