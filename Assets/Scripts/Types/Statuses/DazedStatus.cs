using System;
using System.Collections.Generic;
using System.Linq;
using Cards.Actions;
using Cards.CardEvents;
using Grid;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Types.Statuses
{
    public class DazedStatus : AbstractStatus
    {
        private RandomState random;
        public DazedStatus(int amount, RandomState random)
        {
            this.Amount = amount;
            this.random = random;
        }
        
        public override List<AbstractCardEvent> Modify(List<AbstractCardEvent> cardEvents)
        {
            List<AbstractCardEvent> gainStepsCardEvents = new List<AbstractCardEvent>();

            int amounttodaze = Amount;
            
            foreach (AbstractCardEvent cardEvent in cardEvents)
            {
                if (amounttodaze == 0)
                {
                    continue;
                }
                
                if (cardEvent is GainStepsCardEvent gainStepsCardEvent)
                {
                    gainStepsCardEvents.Add(gainStepsCardEvent);
                    amounttodaze--;

                }
                if (cardEvent is MoveCardEvent moveCardEvent)
                {
                    gainStepsCardEvents.Add(moveCardEvent);
                    amounttodaze--;
                }
            }

            foreach (AbstractCardEvent blockedEvent in gainStepsCardEvents)
            {
                cardEvents.Add(new MoveCardEvent(1, HexGridManager.HexDirections[random.Next(HexGridManager.HexDirections.Length)])
                {
                    PreviewSourceActionIndex = blockedEvent.PreviewSourceActionIndex
                });
            }

            if (gainStepsCardEvents.Count > 0)
                cardEvents.Add(new ModifyStatusAmountCardEvent(this, -gainStepsCardEvents.Count));
            
            cardEvents = cardEvents.Except(gainStepsCardEvents).ToList();
            
            return cardEvents;
        }

        public override void OnEndTurn()
        {
        }
    }
}
