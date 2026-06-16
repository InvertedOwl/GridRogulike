using System.Collections.Generic;
using Entities;
using StateManager;

namespace Cards.CardEvents
{
    public class BlockPlayerMovementThisCombatCardEvent : AbstractCardEvent
    {
        public override Dictionary<string, PreviewValue> GetPreviewValues()
        {
            return new Dictionary<string, PreviewValue>();
        }

        public override void Activate(AbstractEntity entity)
        {
            if (entity == null || entity.entityType != EntityType.Player)
                return;

            if (GameStateManager.Instance.GetCurrent<PlayingState>() is { } playingState)
                playingState.BlockPlayerMovementForCombat();
        }
    }
}
