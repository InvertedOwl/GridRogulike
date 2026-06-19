using System.Collections.Generic;
using Cards.CardEvents;
using Entities;
using UnityEngine;

namespace Cards.Actions
{
    public class PushEnemyAwayAction : AbstractAction
    {
        private const int DefaultPushDistance = 1;

        public int amount;

        public PushEnemyAwayAction(int baseCost, string color, AbstractEntity entity)
            : this(baseCost, color, entity, DefaultPushDistance)
        {
        }

        public PushEnemyAwayAction(int baseCost, string color, AbstractEntity entity, int amount)
            : base(baseCost, color, entity)
        {
            this.amount = amount;
        }

        public override List<AbstractCardEvent> Activate(CardMonobehaviour cardMono)
        {
            return new List<AbstractCardEvent>
            {
                new PushEntityAwayCardEvent(null, amount)
            };
        }

        public override List<AbstractCardEvent> Activate(CardPlayContext context)
        {
            if (context?.Targets != null && context.Targets.TryGetFirstEntity(out AbstractEntity target))
                return new List<AbstractCardEvent> { new PushEntityAwayCardEvent(target, amount) };

            if (context?.Targets != null && context.Targets.TryGetFirstPosition(out Vector2Int targetPosition))
                return new List<AbstractCardEvent> { new PushEntityAwayCardEvent(targetPosition, amount) };

            return Activate(context?.CardMono);
        }

        public override List<RectTransform> UpdateGraphic(GameObject diagram, GameObject tilePrefab, GameObject arrowPrefab)
        {
            return new List<RectTransform>();
        }

        public override string GetText()
        {
            return "Push target away " + amount + " tile" + (amount == 1 ? "" : "s");
        }

        public override string GetText(CardActionPreview preview)
        {
            int finalAmount = preview.GetTotalFinalValue(CardPreviewKeys.Distance, amount);
            return "Push target away " + preview.FormatValue("", amount, finalAmount) + " tile" + (finalAmount == 1 ? "" : "s");
        }

        public override string ToString()
        {
            return "Push Target Away " + amount;
        }
    }
}
