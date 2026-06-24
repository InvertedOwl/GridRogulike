using System.Collections.Generic;
using Cards.CardEvents;
using Entities;
using Types.Statuses;
using UnityEngine;

namespace Cards.Actions
{
    public class StatusAttackAction : AttackAction
    {
        public StatusApplicationType statusType;
        public int statusAmount;

        public StatusAttackAction(
            int baseCost,
            string color,
            AbstractEntity entity,
            int amount,
            StatusApplicationType statusType,
            int statusAmount) : base(baseCost, color, entity, amount)
        {
            this.statusType = statusType;
            this.statusAmount = statusAmount;
        }

        public override List<AbstractCardEvent> Activate(CardMonobehaviour cardMono)
        {
            return new List<AbstractCardEvent>();
        }

        public override List<AbstractCardEvent> Activate(CardPlayContext context)
        {
            AbstractStatus status = CreateStatus(context);
            if (context?.Targets == null)
                return new List<AbstractCardEvent>();

            if (context.Targets.TryGetFirstEntity(out AbstractEntity target))
                return new List<AbstractCardEvent> { new AttackCardEvent(target.positionRowCol, _amount, status, manual: false) };

            if (context.Targets.TryGetFirstPosition(out Vector2Int targetPosition))
                return new List<AbstractCardEvent> { new AttackCardEvent(targetPosition, _amount, status, manual: false) };

            return new List<AbstractCardEvent>();
        }

        public override string GetText()
        {
            return Amount + " <attack><pos=60%>" + statusAmount + " " + StatusIcon();
        }

        public override string GetText(CardActionPreview preview)
        {
            int finalAmount = preview.GetFirstFinalValue(CardPreviewKeys.Damage, Amount);
            int finalStatus = preview.GetFirstFinalValue(CardPreviewKeys.StatusAmount, statusAmount);
            return preview.FormatValue("<attack>", Amount, finalAmount) +
                   "<pos=60%>" +
                   preview.FormatValue(StatusIcon(), statusAmount, finalStatus);
        }

        private AbstractStatus CreateStatus(CardPlayContext context)
        {
            return ApplyStatusToEntityAction.CreateStatus(
                statusType,
                statusAmount,
                GetStableActionRandom(context, "status"));
        }

        private string StatusIcon()
        {
            return ApplyStatusToEntityAction.StatusIcon(statusType);
        }
    }
}
