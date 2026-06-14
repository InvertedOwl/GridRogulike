using System.Collections.Generic;
using Entities;
using Grid;
using StateManager;
using UnityEngine;
using Util;

namespace Cards.CardEvents
{
    public class ShieldCardEvent: AbstractCardEvent
    {
        private const string ShieldFxKey = "MagicBuffBlue";

        public int amount;


        public ShieldCardEvent(int amount)
        {
            this.amount = amount;
        }

        public override Dictionary<string, PreviewValue> GetPreviewValues()
        {
            return new Dictionary<string, PreviewValue>
            {
                [CardPreviewKeys.Shield] = PreviewValue.Int(amount)
            };
        }


        public override void Activate(AbstractEntity entity)
        {
            if (entity == null)
                return;

            if (GameStateManager.Instance.GetCurrent<PlayingState>() is { } playing)
            {
                entity.Shield += amount;
                PlayShieldFx(entity);
            }

        }

        private void PlayShieldFx(AbstractEntity entity)
        {
            if (amount <= 0 || FXManager.Instance == null)
                return;

            if (FXManager.Instance.TryPlay(ShieldFxKey, GetShieldFxPosition(entity), out GameObject fxInstance) &&
                fxInstance != null)
            {
                fxInstance.transform.rotation = Quaternion.LookRotation(-Vector3.forward, Vector3.up);
                fxInstance.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
            }
        }

        private Vector3 GetShieldFxPosition(AbstractEntity entity)
        {
            if (HexGridManager.Instance != null &&
                HexGridManager.Instance._hexObjects.TryGetValue(entity.positionRowCol, out GameObject hexObject) &&
                hexObject != null)
            {
                return hexObject.transform.position;
            }

            return entity.transform.position;
        }
    }
}
