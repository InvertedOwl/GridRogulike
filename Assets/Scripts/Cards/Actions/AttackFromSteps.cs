using System.Collections.Generic;
using Cards.CardEvents;
using Entities;
using UnityEngine;
using Util;

namespace Cards.Actions
{
    public class AttackFromSteps: AttackAction
    {
        public override string Icon
        {
            get
            {
                return "Damage4";
            }
        }

        public AttackFromSteps(int baseCost, string color, AbstractEntity entity) : base(baseCost, color, entity, 0)
        {
        }

        public override List<AbstractCardEvent> Activate(CardMonobehaviour cardMono)
        {
            return new List<AbstractCardEvent>();
        }

        public override List<AbstractCardEvent> Activate(CardPlayContext context)
        {
            int amount = BattleStats.TilesMovedThisTurn * 5;
            if (context?.Targets == null)
                return new List<AbstractCardEvent>();

            if (context.Targets.TryGetFirstEntity(out AbstractEntity target))
                return new List<AbstractCardEvent> { new AttackCardEvent(target.positionRowCol, amount, manual: false) };

            if (context.Targets.TryGetFirstPosition(out Vector2Int targetPosition))
                return new List<AbstractCardEvent> { new AttackCardEvent(targetPosition, amount, manual: false) };

            return new List<AbstractCardEvent>();
        }

        public override string GetText()
        {
            return "Deal damage equal to number of tiles moved this turn (0)";
        }

        public override string GetText(CardActionPreview preview)
        {
            int amount = BattleStats.TilesMovedThisTurn * 5;
            int finalAmount = preview.GetTotalFinalValue(CardPreviewKeys.Damage, amount);
            return "Deal <sprite name=\"Damage4\"> equal to number of tiles moved this turn (" +
                   preview.FormatValue("", amount, finalAmount) +
                   ")";
        }

        public override string ToString()
        {
            return "Attack From Steps";
        }
    }
}
