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
        public const string DefaultAttackHitFxKey = "SmallExplosionFire";

        public Vector2Int position;
        public int distance;
        public string direction;
        public int amount;
        public AbstractStatus status;
        public bool manual = true;
        public string hitFxKey;

        public bool usePosition = false;

        public AttackCardEvent(
            int distance,
            string direction,
            int amount,
            AbstractStatus status = null,
            bool manual = true,
            string hitFxKey = DefaultAttackHitFxKey)
        {
            this.distance = distance;
            this.direction = direction;
            this.amount = amount;
            this.status = status;
            this.manual = manual;
            this.hitFxKey = hitFxKey;
        }

        public AttackCardEvent(
            Vector2Int position,
            int amount,
            AbstractStatus status = null,
            bool manual = true,
            string hitFxKey = DefaultAttackHitFxKey)
        {
            this.amount = amount;
            this.position = position;
            this.status = status;
            this.usePosition = true;
            this.manual = manual;
            this.hitFxKey = hitFxKey;
        }

        public AttackCardEvent Copy()
        {
            AttackCardEvent copy = usePosition
                ? new AttackCardEvent(position, amount, status, manual, hitFxKey)
                : new AttackCardEvent(distance, direction, amount, status, manual, hitFxKey);

            copy.PreviewSourceActionIndex = PreviewSourceActionIndex;
            return copy;
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
            ActivateWithResult(entity, new CardEventContext());
        }

        public override CardEventResult ActivateWithResult(AbstractEntity entity, CardEventContext context)
        {
            if (entity == null)
                return new CardEventResult(this);

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

                CardEventResult result = playing.DamageEntities(targetPosition, amount, status, this);
                PlayAttackHitFx(playing, targetPosition, hitFxKey);
                RangedStatus.ConsumeAfterAttack(entity);
                return result;
            }

            return new CardEventResult(this);
        }

        public static void PlayAttackHitFx(
            PlayingState playing,
            Vector2Int targetPosition,
            string hitFxKey = DefaultAttackHitFxKey)
        {
            if (FXManager.Instance == null || string.IsNullOrWhiteSpace(hitFxKey))
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

            FXManager.Instance.TryPlay(hitFxKey, spawnPosition);
        }
    }
}
