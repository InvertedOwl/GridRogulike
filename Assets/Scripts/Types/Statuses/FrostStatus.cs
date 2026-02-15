using System;
using System.Collections.Generic;
using Cards.Actions;
using Cards.CardEvents;

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
                if (cardEvent is GainStepsCardEvent || cardEvent is MoveCardEvent){
                    stepOrMove.Add(cardEvent);
                }
            }
            
            Amount -= stepOrMove.Count;
            Amount = Math.Max(0, Amount);
            newEvents.RemoveAll((item) => stepOrMove.Contains(item));
            return newEvents;
        }

        public override void OnEndTurn()
        {
        }
    }
}