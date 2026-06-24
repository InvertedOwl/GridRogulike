using System.Collections.Generic;
using Cards.CardEvents;
using Entities;
using UnityEngine;

namespace Cards.Actions
{
    public class ApplyStatusToFixedEntityAction : AbstractAction
    {
        public AbstractEntity target;
        public StatusApplicationType statusType;
        public int amount;

        public ApplyStatusToFixedEntityAction(
            int baseCost,
            string color,
            AbstractEntity entity,
            AbstractEntity target,
            StatusApplicationType statusType,
            int amount) : base(baseCost, color, entity)
        {
            this.target = target;
            this.statusType = statusType;
            this.amount = amount;
        }

        public override List<AbstractCardEvent> Activate(CardMonobehaviour cardMono)
        {
            return Activate(cardMono, previewMode: false);
        }

        public override List<AbstractCardEvent> Activate(CardMonobehaviour cardMono, bool previewMode)
        {
            if (target == null || target.Health <= 0)
                return new List<AbstractCardEvent>();

            return new List<AbstractCardEvent>
            {
                new ApplyStatusToEntityCardEvent(
                    target,
                    ApplyStatusToEntityAction.CreateStatus(
                        statusType,
                        amount,
                        GetStableActionRandom(cardMono, previewMode, "status")))
            };
        }

        public override string GetText()
        {
            return "Apply " + amount + " " + ApplyStatusToEntityAction.StatusIcon(statusType) + " to target";
        }

        public override List<RectTransform> UpdateGraphic(GameObject diagram, GameObject tilePrefab, GameObject arrowPrefab)
        {
            return new List<RectTransform>();
        }

        public override string ToString()
        {
            return "Apply " + statusType + " " + amount;
        }
    }
}
