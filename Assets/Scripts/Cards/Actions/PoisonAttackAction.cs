using System.Collections.Generic;
using Cards.CardEvents;
using Entities;
using Types.Statuses;

namespace Cards.Actions
{
    public class PoisonAttackAction: AttackAction
    {
        public int poisonAmount;

        public PoisonAttackAction(int baseCost, string color, AbstractEntity entity, int amount, int poisonAmount)
            : base(baseCost, color, entity, amount)
        {
            this.poisonAmount = poisonAmount;
        }

        public override List<AbstractCardEvent> Activate(CardMonobehaviour cardMono)
        {
            return new List<AbstractCardEvent>();
        }

        public override List<AbstractCardEvent> Activate(CardPlayContext context)
        {
            PoisonStatus poison = new PoisonStatus(poisonAmount);
            if (context?.Targets == null)
                return new List<AbstractCardEvent>();

            if (context.Targets.TryGetFirstEntity(out AbstractEntity target))
                return new List<AbstractCardEvent> { new AttackCardEvent(target.positionRowCol, _amount, poison, manual: false) };

            if (context.Targets.TryGetFirstPosition(out UnityEngine.Vector2Int targetPosition))
                return new List<AbstractCardEvent> { new AttackCardEvent(targetPosition, _amount, poison, manual: false) };

            return new List<AbstractCardEvent>();
        }

        public override string GetText()
        {
            return Amount + " <attack>     " + poisonAmount + " <poison>";
        }

        public override string GetText(CardActionPreview preview)
        {
            int finalAmount = preview.GetFirstFinalValue(CardPreviewKeys.Damage, Amount);
            int finalPoison = preview.GetFirstFinalValue(CardPreviewKeys.StatusAmount, poisonAmount);
            return preview.FormatValue("<attack>", Amount, finalAmount) + "     " +
                   preview.FormatValue("<poison>", poisonAmount, finalPoison);
        }

        public override string ToString()
        {
            return "Poison Attack D:" + _amount + " P:" + poisonAmount;
        }
    }
}
