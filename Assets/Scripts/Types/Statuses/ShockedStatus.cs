using System.Collections.Generic;
using Cards.CardEvents;
using Entities;
using Grid;
using StateManager;
using UnityEngine;

namespace Types.Statuses
{
    public class ShockedStatus : AbstractStatus
    {
        public ShockedStatus(int amount)
        {
            Amount = amount;
        }

        public override List<AbstractCardEvent> Modify(List<AbstractCardEvent> cardEvent)
        {
            return cardEvent;
        }

        public override void OnDamageReceived(int damage)
        {
            if (Entity == null || Amount <= 0 || damage <= 0)
                return;

            if (GameStateManager.Instance == null || !GameStateManager.Instance.IsCurrent<PlayingState>())
                return;

            int spreadDamage = Mathf.CeilToInt(damage * Mathf.Clamp(Amount, 0, 100) / 100f);
            if (spreadDamage <= 0)
                return;

            PlayingState playingState = GameStateManager.Instance.GetCurrent<PlayingState>();
            foreach (Vector2Int adjacentHex in HexGridManager.AdjacentHexes(Entity.positionRowCol))
            {
                if (!playingState.EntitiesOnHex(adjacentHex, out List<AbstractEntity> entities))
                    continue;

                foreach (AbstractEntity target in entities)
                {
                    if (target != null && target != Entity)
                        target.Damage(spreadDamage, false);
                }
            }
        }

        public override void OnEndTurn()
        {
        }
    }
}
