using Entities;
using Grid;
using StateManager;
using Types.Statuses;
using UnityEngine;

namespace Cards.CardEvents
{
    public class AttackCardEvent: AbstractCardEvent
    {
        public Vector2Int position;
        public int distance;
        public string direction;
        public int amount;
        public AbstractStatus status;
        public bool manual = true;

        public bool usePosition = false;

        public AttackCardEvent(int distance, string direction, int amount, AbstractStatus status = null, bool manual = true)
        {
            this.distance = distance;
            this.direction = direction;
            this.amount = amount;
            this.status = status;
            this.manual = manual;
        }

        public AttackCardEvent(Vector2Int position, int amount, AbstractStatus status = null, bool manual = true)
        {
            this.amount = amount;
            this.position = position;
            this.status = status;
            this.usePosition = true;
            this.manual = manual;
        }

        
        public override void Activate(AbstractEntity entity)
        {
            if (GameStateManager.Instance.GetCurrent<PlayingState>() is { } playing)
            {
                if (usePosition)
                {
                    playing.DamageEntities(position, amount, status);
                }
                else
                {
                    playing.DamageEntities(HexGridManager.MoveHex(entity.positionRowCol, direction, distance), amount, status);
                }
                
            }
            
        }
    }
}