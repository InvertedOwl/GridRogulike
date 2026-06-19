using System.Collections.Generic;
using Entities;
using Grid;
using StateManager;
using Cards.CardEvents;
using Types.Statuses;
using UnityEngine;
using UnityEngine.UI;
using Util;

namespace Cards.Actions
{
    public class PoisonAttackAction: AttackAction
    {
        public int poisonAmount;

        public PoisonAttackAction(int baseCost, string color, AbstractEntity entity, string direction, int distance, int _amount, int poisonAmount) : base(baseCost, color, entity, direction, distance, _amount)
        {
            this.poisonAmount = poisonAmount;
        }

        public override List<AbstractCardEvent> Activate(CardMonobehaviour cardMono)
        {
            return new List<AbstractCardEvent> { new AttackCardEvent(_distance, _direction, _amount, new PoisonStatus(poisonAmount)) };
        }

        public override List<AbstractCardEvent> Activate(CardPlayContext context)
        {
            PoisonStatus poison = new PoisonStatus(poisonAmount);
            if (context?.Targets == null)
                return new List<AbstractCardEvent> { new AttackCardEvent(_distance, _direction, _amount, poison) };

            if (context.Targets.TryGetFirstEntity(out AbstractEntity target))
                return new List<AbstractCardEvent> { new AttackCardEvent(target.positionRowCol, _amount, poison, manual: false) };

            if (context.Targets.TryGetFirstPosition(out Vector2Int targetPosition))
                return new List<AbstractCardEvent> { new AttackCardEvent(targetPosition, _amount, poison, manual: false) };

            return new List<AbstractCardEvent>();
        }

        public override List<RectTransform> UpdateGraphic(GameObject diagram, GameObject tilePrefab, GameObject arrowPrefab)
        {
            GameObject basic = GameObject.Instantiate(tilePrefab, diagram.transform);
            Vector2Int newPos =
                HexGridManager.MoveHex(new Vector2Int(0, 0), this.Direction, this.Distance);
            Vector2 newPosWorld = HexGridManager.GetHexCenter(newPos.x, newPos.y) * 46.2222f;

            basic.GetComponent<RectTransform>().localPosition = newPosWorld;
            basic.GetComponent<Image>().color = new Color(192/255.0f, 52/255.0f, 235/255.0f);
            basic.GetComponent<RectTransform>();
            return new List<RectTransform> { basic.GetComponent<RectTransform>() };
        }

        public override string GetText()
        {
            return Amount + " <attack>     " + poisonAmount + " <poison>     " + Distance + " <arrow>";
        }

        public override string GetText(CardActionPreview preview)
        {
            int finalAmount = preview.GetFirstFinalValue(CardPreviewKeys.Damage, Amount);
            int finalPoison = preview.GetFirstFinalValue(CardPreviewKeys.StatusAmount, poisonAmount);
            int finalDistance = preview.GetFirstFinalValue(CardPreviewKeys.Distance, Distance);
            return preview.FormatValue("<attack>", Amount, finalAmount) + "     " +
                   preview.FormatValue("<poison>", poisonAmount, finalPoison) + "     " +
                   preview.FormatValue("<arrow>", Distance, finalDistance);
        }

        public override string ToString()
        {
            return "Attack " + this._distance + " " + FixDirection(this._direction) + " D:" + this._amount;
        }
    }
}
