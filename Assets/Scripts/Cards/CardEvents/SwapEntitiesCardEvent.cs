using System.Collections.Generic;
using Entities;
using StateManager;

namespace Cards.CardEvents
{
    public class SwapEntitiesCardEvent : AbstractCardEvent
    {
        public AbstractEntity first;
        public AbstractEntity second;
        public bool useSourceAsFirst;

        public SwapEntitiesCardEvent(AbstractEntity first, AbstractEntity second)
        {
            this.first = first;
            this.second = second;
        }

        public SwapEntitiesCardEvent(AbstractEntity second)
        {
            this.second = second;
            useSourceAsFirst = true;
        }

        public override Dictionary<string, PreviewValue> GetPreviewValues()
        {
            return new Dictionary<string, PreviewValue>();
        }

        public override void Activate(AbstractEntity entity)
        {
            AbstractEntity firstEntity = useSourceAsFirst ? entity : first;
            if (!CanSwap(firstEntity, second))
                return;

            if (GameStateManager.Instance.GetCurrent<PlayingState>() is not { } playingState)
                return;

            playingState.SwapEntities(firstEntity, second);
        }

        protected virtual bool CanSwap(AbstractEntity firstEntity, AbstractEntity secondEntity)
        {
            return firstEntity != null &&
                   secondEntity != null &&
                   firstEntity != secondEntity &&
                   firstEntity.Health > 0 &&
                   secondEntity.Health > 0;
        }
    }
}
