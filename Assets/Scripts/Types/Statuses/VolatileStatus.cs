using System.Collections.Generic;
using Cards.CardEvents;
using Entities;
using Grid;
using StateManager;
using UnityEngine;

namespace Types.Statuses
{
    public class VolatileStatus : AbstractStatus
    {
        public VolatileStatus(int amount)
        {
            Amount = amount;
        }

        public override List<AbstractCardEvent> Modify(List<AbstractCardEvent> cardEvent)
        {
            return cardEvent;
        }

        public override void OnDeath()
        {
            if (Entity == null || Amount <= 0)
                return;

            if (GameStateManager.Instance == null || !GameStateManager.Instance.IsCurrent<PlayingState>())
                return;

            PlayingState playingState = GameStateManager.Instance.GetCurrent<PlayingState>();
            foreach (Vector2Int adjacentHex in HexGridManager.AdjacentHexes(Entity.positionRowCol))
            {
                if (!playingState.EntitiesOnHex(adjacentHex, out List<AbstractEntity> entities))
                    continue;

                foreach (AbstractEntity target in entities)
                {
                    if (target != null && target != Entity)
                        target.Damage(Amount);
                }
            }
        }

        public override void OnEndTurn()
        {
        }
    }
}
