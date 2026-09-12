using Entities;
using StateManager;
using UnityEngine;

namespace Cards.CardEvents
{
    public class PlaceBattleTileCardEvent : AbstractCardEvent
    {
        public string DefinitionId { get; }
        public Vector2Int TargetPosition { get; }
        public int Power { get; }
        public bool ReplaceExisting { get; }

        public PlaceBattleTileCardEvent(
            string definitionId,
            Vector2Int targetPosition,
            int power = -1,
            bool replaceExisting = true)
        {
            DefinitionId = definitionId;
            TargetPosition = targetPosition;
            Power = power;
            ReplaceExisting = replaceExisting;
        }

        public override void Activate(AbstractEntity entity)
        {
            if (entity == null || GameStateManager.Instance == null)
                return;

            PlayingState state = GameStateManager.Instance.GetCurrent<PlayingState>();
            state?.TryPlaceBattleTile(
                DefinitionId,
                TargetPosition,
                entity,
                Power,
                ReplaceExisting);
        }
    }
}
