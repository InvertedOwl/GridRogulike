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

        public abstract void OnEndTurn();
    }
}
