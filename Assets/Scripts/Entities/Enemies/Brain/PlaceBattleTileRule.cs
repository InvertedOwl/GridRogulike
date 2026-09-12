using Cards.Actions;
using Types.Tiles;
using UnityEngine;

namespace Entities.Enemies
{
    [CreateAssetMenu(fileName = "PlaceBattleTileRule", menuName = "Game/Enemy Brain/Rules/Utility/Place Battle Tile")]
    public class PlaceBattleTileRule : EnemyBrainUtilityRule
    {
        [SerializeField] private EnemyBrainTargetSelector targetSelector = EnemyBrainTargetSelector.Player;
        [SerializeField] private string battleTileId = BattleTileData.PrimedAttackId;
        [SerializeField, Tooltip("Use -1 to use the battle tile definition's default power.")]
        private int power = -1;
        [SerializeField] private bool replaceExisting = true;
        [SerializeField] private int baseCost = 1;
        [SerializeField] private string color = "basic";

        public override bool TryPlan(EnemyTurnContext context)
        {
            if (context == null ||
                !BattleTileData.TryGet(battleTileId, out _) ||
                !TrySelectUtilityTarget(context, targetSelector, out AbstractEntity target))
            {
                return false;
            }

            Vector2Int targetPosition = context.GetEntityPosition(target);
            if (!context.State.CanPlaceBattleTile(
                    battleTileId,
                    targetPosition,
                    context.Self,
                    replaceExisting))
            {
                return false;
            }

            return TryAddUtilityAction(
                context,
                new PlaceBattleTileAction(
                    baseCost,
                    color,
                    context.Self,
                    battleTileId,
                    targetPosition,
                    power,
                    replaceExisting));
        }
    }
}
