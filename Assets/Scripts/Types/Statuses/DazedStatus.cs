using System.Collections.Generic;
using System.Linq;
using Cards.CardEvents;

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
            return Modify(cardEvents, false);
        }

        public override List<AbstractCardEvent> Modify(List<AbstractCardEvent> cardEvents, bool previewMode)
        {
            List<AbstractCardEvent> gainStepsCardEvents = new List<AbstractCardEvent>();
            RandomState modifyRandom = GetModifyRandom(previewMode);

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
                if (cardEvent is MoveCardEvent || cardEvent is RandomMoveCardEvent)
                {
                    gainStepsCardEvents.Add(cardEvent);
                    amounttodaze--;
                }
            }

            foreach (AbstractCardEvent blockedEvent in gainStepsCardEvents)
            {
                cardEvents.Add(new RandomMoveCardEvent(1, modifyRandom)
                {
                    PreviewSourceActionIndex = blockedEvent.PreviewSourceActionIndex
                });
            }

            if (gainStepsCardEvents.Count > 0)
                cardEvents.Add(new ModifyStatusAmountCardEvent(this, -gainStepsCardEvents.Count));
            
            cardEvents = cardEvents.Except(gainStepsCardEvents).ToList();
            
            return cardEvents;
        }

        private RandomState GetModifyRandom(bool previewMode)
        {
            if (random == null)
            {
                random = RunInfo.NewRandom("dazed");
            }

            return previewMode ? random.Clone() : random;
        }

        public override void OnEndTurn()
        {
        }
    }
}
