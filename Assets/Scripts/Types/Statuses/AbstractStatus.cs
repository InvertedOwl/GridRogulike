using System.Collections.Generic;
using Cards.CardEvents;
using Entities;
using UnityEngine;

namespace Types.Statuses
{
    public abstract class AbstractStatus
    {
        public AbstractEntity Entity;
        public int Amount;
        public abstract List<AbstractCardEvent> Modify(List<AbstractCardEvent> cardEvent);
        public virtual List<AbstractCardEvent> Modify(List<AbstractCardEvent> cardEvent, bool previewMode)
        {
            return Modify(cardEvent);
        }

        public virtual int ModifyDrawCount(int drawCount)
        {
            return drawCount;
        }

        public virtual void OnApply(AbstractEntity entity, int amountAdded)
        {
        }

        public virtual void OnStartTurn()
        {
        }

        public virtual void OnTurnResourcesReady()
        {
        }

        public virtual bool PreservesShieldOnStartTurn(AbstractEntity entity)
        {
            return false;
        }

        public virtual void OnShieldPreservedStartTurn()
        {
        }

        public virtual bool BlocksMovement(AbstractEntity entity, int distance)
        {
            return false;
        }

        public virtual void OnDamageReceived(int damage)
        {
        }

        public virtual void OnDeath()
        {
        }

        public abstract void OnEndTurn();
    }
}
