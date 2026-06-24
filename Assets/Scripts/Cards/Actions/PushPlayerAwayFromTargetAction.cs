using System.Collections.Generic;
using Cards.CardEvents;
using Entities;
using UnityEngine;

namespace Cards.Actions
{
    public class PushPlayerAwayFromTargetAction : AbstractAction
    {
        public int amount;

        public PushPlayerAwayFromTargetAction(int baseCost, string color, AbstractEntity entity, int amount)
            : base(baseCost, color, entity)
        {
            this.amount = amount;
        }

        public override List<AbstractCardEvent> Activate(CardMonobehaviour cardMono)
        {
            return new List<AbstractCardEvent>();
        }

        public override List<AbstractCardEvent> Activate(CardPlayContext context)
        {
            if (context?.Targets == null ||
                !context.Targets.TryGetFirstEntity(out AbstractEntity target))
            {
                return new List<AbstractCardEvent>();
            }

            return new List<AbstractCardEvent>
            {
                new PushPlayerAwayFromTargetCardEvent(target, amount)
            };
        }

        public override List<AbstractCardEvent> Preview(CardPlayContext context)
        {
            return Activate(context);
        }

        public override List<RectTransform> UpdateGraphic(GameObject diagram, GameObject tilePrefab, GameObject arrowPrefab)
        {
            return new List<RectTransform>();
        }

        public override string GetText()
        {
            return "Push yourself away from target " + amount + " tile" + (amount == 1 ? "" : "s");
        }

        public override string GetText(CardActionPreview preview)
        {
            int finalAmount = preview.GetTotalFinalValue(CardPreviewKeys.Distance, amount);
            return "Push yourself away from target " + preview.FormatValue("", amount, finalAmount) + " tile" + (finalAmount == 1 ? "" : "s");
        }

        public override string ToString()
        {
            return "Push Player Away From Target " + amount;
        }
    }
}
