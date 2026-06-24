using Cards.CardEvents;
using Entities;
using Types.Statuses;

namespace Cards.Actions
{
    public class DirectionalStatusAttackAction : DirectionalAttackAction
    {
        public StatusApplicationType statusType;
        public int statusAmount;

        public DirectionalStatusAttackAction(
            int baseCost,
            string color,
            AbstractEntity entity,
            string direction,
            int distance,
            int amount,
            StatusApplicationType statusType,
            int statusAmount)
            : base(baseCost, color, entity, direction, distance, amount)
        {
            this.statusType = statusType;
            this.statusAmount = statusAmount;
        }

        protected override AttackCardEvent CreateAttackEvent(bool previewMode)
        {
            return new AttackCardEvent(
                Distance,
                Direction,
                Amount,
                ApplyStatusToEntityAction.CreateStatus(
                    statusType,
                    statusAmount,
                    GetActionRandom(previewMode)));
        }

        public override string GetText()
        {
            return Amount + " <attack><pos=60%>" + statusAmount + " " + ApplyStatusToEntityAction.StatusIcon(statusType);
        }

        public override string GetText(CardActionPreview preview)
        {
            int finalAmount = preview.GetFirstFinalValue(CardPreviewKeys.Damage, Amount);
            int finalStatus = preview.GetFirstFinalValue(CardPreviewKeys.StatusAmount, statusAmount);
            return preview.FormatValue("<attack>", Amount, finalAmount) +
                   "<pos=60%>" +
                   preview.FormatValue(ApplyStatusToEntityAction.StatusIcon(statusType), statusAmount, finalStatus);
        }
    }
}
