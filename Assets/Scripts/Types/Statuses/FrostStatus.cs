using System.Collections.Generic;
using Cards.CardEvents;
using Entities;
using UnityEngine;

namespace Types.Statuses
{
    public class FrostStatus : AbstractStatus
    {
        public FrostStatus(int amount)
        {
            this.Amount = amount;
        }
        
        public override List<AbstractCardEvent> Modify(List<AbstractCardEvent> cardEvents)
        {
            List<AbstractCardEvent> newEvents = new List<AbstractCardEvent>(cardEvents);
            List<AbstractCardEvent> stepOrMove = new List<AbstractCardEvent>();
            foreach (AbstractCardEvent cardEvent in cardEvents)
            {
                if (cardEvent is GainStepsCardEvent || cardEvent is MoveCardEvent || cardEvent is RandomMoveCardEvent){
                    stepOrMove.Add(cardEvent);
                }
            }
            
            if (stepOrMove.Count > 0)
                newEvents.Add(new ModifyStatusAmountCardEvent(this, -stepOrMove.Count));

            newEvents.RemoveAll((item) => stepOrMove.Contains(item));
            return newEvents;
        }

        public override bool BlocksMovement(AbstractEntity entity, int distance)
        {
            if (Amount <= 0)
                return false;

            Amount -= Mathf.Max(1, distance);
            return true;
        }

        public override void OnEndTurn()
        {
        }
    }
}
