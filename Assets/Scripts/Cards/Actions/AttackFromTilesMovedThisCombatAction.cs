using System.Collections.Generic;
using Cards.CardEvents;
using Entities;
using UnityEngine;
using Util;

namespace Cards.Actions
{
    public class AttackFromTilesMovedThisCombatAction : AttackAction
    {
        private const int DamagePerTileMoved = 2;

        public override string Icon => "Damage4";

        public AttackFromTilesMovedThisCombatAction(int baseCost, string color, AbstractEntity entity)
            : base(baseCost, color, entity, 0)
        {
        }

        public override List<AbstractCardEvent> Activate(CardMonobehaviour cardMono)
        {
            return new List<AbstractCardEvent>();
        }

        public override List<AbstractCardEvent> Activate(CardPlayContext context)
        {
            int amount = BattleStats.TilesMovedThisBattle * DamagePerTileMoved;
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
            return "Deal " + DamagePerTileMoved + " <attack> for each tile moved this combat (0)";
        }

        public override string GetText(CardActionPreview preview)
        {
            int amount = BattleStats.TilesMovedThisBattle * DamagePerTileMoved;
            int finalAmount = preview.GetTotalFinalValue(CardPreviewKeys.Damage, amount);
            return "Deal " + DamagePerTileMoved + " <attack> for each tile moved this combat (" +
                   preview.FormatValue("", amount, finalAmount) +
                   ")";
        }

        public override string ToString()
        {
            return "Attack From Tiles Moved This Combat";
        }
    }
}
