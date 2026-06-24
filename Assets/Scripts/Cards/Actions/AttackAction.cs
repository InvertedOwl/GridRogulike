using System.Collections.Generic;
using Entities;
using Cards.CardEvents;
using UnityEngine;

namespace Cards.Actions
{
    public class AttackAction: AbstractAction
    {
        
        public override string Icon
        {
            get
            {
                return "Damage4";
            }
        }

        public int _amount;
        public int Amount { get { return _amount; } set { _amount = value; } }

        public AttackAction(int baseCost, string color, AbstractEntity entity, int amount) : base(baseCost, color, entity)
        {
            _amount = amount;
        }

        public override List<AbstractCardEvent> Activate(CardMonobehaviour cardMono)
        {
            return new List<AbstractCardEvent>();
        }

        public override List<AbstractCardEvent> Activate(CardPlayContext context)
        {
            if (context?.Targets == null)
                return new List<AbstractCardEvent>();

            if (context.Targets.TryGetFirstEntity(out AbstractEntity target))
                return new List<AbstractCardEvent> { new AttackCardEvent(target.positionRowCol, _amount, manual: false) };

            if (context.Targets.TryGetFirstPosition(out Vector2Int targetPosition))
                return new List<AbstractCardEvent> { new AttackCardEvent(targetPosition, _amount, manual: false) };

            return new List<AbstractCardEvent>();
        }

        public string arrowUUID;
        
        public override void HoverOn()
        {
            Debug.Log("Hovering on attack actions");
        }

        public override void HoverOff()
        {
            Debug.Log("Hovering off attack actions");
        }

        public override List<RectTransform> UpdateGraphic(GameObject diagram, GameObject tilePrefab, GameObject arrowPrefab)
        {
            GameObject basic = GameObject.Instantiate(tilePrefab, diagram.transform);
            return new List<RectTransform> { basic.GetComponent<RectTransform>() };
        }

        public override string GetText()
        {
            return Amount + " <attack>";
        }

        public override string GetText(CardActionPreview preview)
        {
            int finalAmount = preview.GetFirstFinalValue(CardPreviewKeys.Damage, Amount);
            return preview.FormatValue("<attack>", Amount, finalAmount);
        }

        public override string ToString()
        {
            return "Attack D:" + this._amount;
        }
    }
}
