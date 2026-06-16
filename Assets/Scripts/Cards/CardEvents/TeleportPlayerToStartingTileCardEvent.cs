using System.Collections.Generic;
using Entities;
using StateManager;
using UnityEngine;

namespace Cards.CardEvents
{
    public class TeleportPlayerToStartingTileCardEvent : AbstractCardEvent
    {
        private static readonly Vector2Int StartingTilePosition = Vector2Int.zero;

        public override Dictionary<string, PreviewValue> GetPreviewValues()
        {
            return new Dictionary<string, PreviewValue>();
        }

        public override void Activate(AbstractEntity entity)
        {
            if (entity == null || entity.entityType != EntityType.Player)
                return;

            if (GameStateManager.Instance.GetCurrent<PlayingState>() is not { } playingState)
                return;

            playingState.MoveEntity(entity, StartingTilePosition);
        }
    }
}
