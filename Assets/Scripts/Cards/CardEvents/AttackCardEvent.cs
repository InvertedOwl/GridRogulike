using System.Collections.Generic;
using Entities;
using Grid;
using StateManager;
using Types.Statuses;
using UnityEngine;

namespace Cards.CardEvents
{
    public class AttackCardEvent: AbstractCardEvent
    {
        private const string AttackHitFxKey = "SmallExplosionFire";

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

        public override Dictionary<string, PreviewValue> GetPreviewValues()
        {
            Dictionary<string, PreviewValue> values = new Dictionary<string, PreviewValue>
            {
                [CardPreviewKeys.Damage] = PreviewValue.Int(amount),
                [CardPreviewKeys.Direction] = PreviewValue.Text(direction)
            };

            if (!usePosition)
                values[CardPreviewKeys.Distance] = PreviewValue.Int(distance);

            if (status != null)
            {
                values[CardPreviewKeys.StatusAmount] = PreviewValue.Int(status.Amount);
                values[CardPreviewKeys.StatusName] = PreviewValue.Text(status.GetType().Name);
            }

            return values;
        }


        public override void Activate(AbstractEntity entity)
        {
            if (GameStateManager.Instance.GetCurrent<PlayingState>() is { } playing)
            {
                Vector2Int targetPosition;
                if (usePosition)
                {
                    targetPosition = position;
                }
                else
                {
                    targetPosition = HexGridManager.MoveHex(entity.positionRowCol, direction, distance);
                }

                playing.DamageEntities(targetPosition, amount, status);
                PlayAttackHitFx(playing, targetPosition);
            }

        }

        public static void PlayAttackHitFx(PlayingState playing, Vector2Int targetPosition)
        {
            if (FXManager.Instance == null)
                return;

            Vector3 spawnPosition = HexGridManager.GetHexCenter(targetPosition.x, targetPosition.y);

            if (playing.EntitiesOnHex(targetPosition, out List<AbstractEntity> entities) && entities.Count > 0)
            {
                spawnPosition = entities[0].transform.position;
            }
            else if (HexGridManager.Instance != null &&
                     HexGridManager.Instance._hexObjects.TryGetValue(targetPosition, out GameObject hexObject) &&
                     hexObject != null)
            {
                spawnPosition = hexObject.transform.position;
            }

            FXManager.Instance.TryPlay(AttackHitFxKey, spawnPosition);
        }
    }
}
