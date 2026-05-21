using System.Collections.Generic;
using Cards.CardEvents;
using Entities;
using Types.Statuses;
using UnityEngine;

namespace Cards.Actions
{
    public enum StatusApplicationType
    {
        Buffed,
        Dazed,
        Frost,
        Poison
    }

    public class ApplyStatusToEntityAction : AbstractAction
    {
        public AbstractEntity target;
        public StatusApplicationType statusType;
        public int amount;

        public ApplyStatusToEntityAction(
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
            return new List<AbstractCardEvent>
            {
                new ApplyStatusToEntityCardEvent(target, CreateStatus())
            };
        }

        public override string GetText()
        {
            return "Apply " + amount + " " + StatusIcon() + " to target";
        }

        public override string GetText(CardActionPreview preview)
        {
            int finalAmount = preview.GetTotalFinalValue(CardPreviewKeys.StatusAmount, amount);
            return "Apply " + preview.FormatValue(StatusIcon(), amount, finalAmount) + " to target";
        }

        public override string ToSimpleText()
        {
            return amount + " " + StatusIcon();
        }

        public override List<RectTransform> UpdateGraphic(GameObject diagram, GameObject tilePrefab, GameObject arrowPrefab)
        {
            return new List<RectTransform>();
        }

        public override string ToString()
        {
            return "Apply " + statusType + " " + amount;
        }

        private AbstractStatus CreateStatus()
        {
            switch (statusType)
            {
                case StatusApplicationType.Dazed:
                    return new DazedStatus(amount, _actionRandom);
                case StatusApplicationType.Frost:
                    return new FrostStatus(amount);
                case StatusApplicationType.Poison:
                    return new PoisonStatus(amount);
                case StatusApplicationType.Buffed:
                default:
                    return new BuffedStatus(amount);
            }
        }

        private string StatusIcon()
        {
            switch (statusType)
            {
                case StatusApplicationType.Dazed:
                    return "<sprite name=\"dazed\">";
                case StatusApplicationType.Frost:
                    return "<sprite name=\"snowflake\">";
                case StatusApplicationType.Poison:
                    return "<sprite name=\"droplets\">";
                case StatusApplicationType.Buffed:
                default:
                    return "<sprite name=\"buffenemies\">";
            }
        }
    }
}
