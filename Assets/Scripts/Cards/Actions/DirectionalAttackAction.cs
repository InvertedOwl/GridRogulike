using System;
using System.Collections.Generic;
using Cards.CardEvents;
using Entities;
using Grid;
using StateManager;
using UnityEngine;

namespace Cards.Actions
{
    public class DirectionalAttackAction : AbstractAction
    {
        public override string Icon
        {
            get
            {
                return "Damage4";
            }
        }

        protected string _direction;
        public string Direction { get { return _direction; } set { _direction = value; } }
        public int _distance;
        public int Distance { get { return _distance; } set { _distance = value; } }
        public int _amount;
        public int Amount { get { return _amount; } set { _amount = value; } }

        public DirectionalAttackAction(
            int baseCost,
            string color,
            AbstractEntity entity,
            string direction,
            int distance,
            int amount) : base(baseCost, color, entity)
        {
            _direction = direction;
            _distance = distance;
            _amount = amount;
        }

        public override List<AbstractCardEvent> Activate(CardMonobehaviour cardMono)
        {
            if (!CanAttackTargetTile())
                return new List<AbstractCardEvent>();

            return new List<AbstractCardEvent> { CreateAttackEvent(previewMode: false) };
        }

        public override List<AbstractCardEvent> Activate(CardMonobehaviour cardMono, bool previewMode)
        {
            if (!CanAttackTargetTile())
                return new List<AbstractCardEvent>();

            return new List<AbstractCardEvent> { CreateAttackEvent(previewMode) };
        }

        protected virtual AttackCardEvent CreateAttackEvent(bool previewMode)
        {
            return new AttackCardEvent(_distance, _direction, _amount);
        }

        private bool CanAttackTargetTile()
        {
            if (entity == null)
                return false;

            if (GameStateManager.Instance.GetCurrent<PlayingState>() is not { } playingState)
                return false;

            if (String.IsNullOrEmpty(_direction) || _distance <= 0)
                return false;

            playingState.EntitiesOnHex(
                HexGridManager.MoveHex(entity.positionRowCol, _direction, _distance),
                out List<AbstractEntity> entities);

            foreach (AbstractEntity target in entities)
            {
                if (target != null && target.entityType == entity.entityType)
                    return false;
            }

            return true;
        }

        public override string GetText()
        {
            return Amount + " <attack><pos=60%>" + Distance + " <arrow>";
        }

        public override string GetText(CardActionPreview preview)
        {
            int finalAmount = preview.GetFirstFinalValue(CardPreviewKeys.Damage, Amount);
            int finalDistance = preview.GetFirstFinalValue(CardPreviewKeys.Distance, Distance);
            return preview.FormatValue("<attack>", Amount, finalAmount) +
                   "<pos=60%>" +
                   preview.FormatValue("<arrow>", Distance, finalDistance);
        }

        public override List<RectTransform> UpdateGraphic(GameObject diagram, GameObject tilePrefab, GameObject arrowPrefab)
        {
            GameObject basic = GameObject.Instantiate(tilePrefab, diagram.transform);
            return new List<RectTransform> { basic.GetComponent<RectTransform>() };
        }

        public override string ToString()
        {
            return "Attack " + _distance + " " + FixDirection(_direction) + " D:" + _amount;
        }
    }
}
