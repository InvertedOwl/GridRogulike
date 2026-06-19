using System.Collections.Generic;
using Cards.Actions;
using Grid;
using UnityEngine;

namespace Entities.Enemies
{
    [CreateAssetMenu(fileName = "WideMeleeAttackRule", menuName = "Game/Enemy Brain/Rules/Attack/Wide Melee Attack")]
    public class WideMeleeAttackRule : EnemyBrainRule
    {
        [SerializeField] private EnemyBrainTargetSelector targetSelector = EnemyBrainTargetSelector.Player;
        [SerializeField] private int damage = 10;
        [SerializeField] private int range = 1;
        [SerializeField, Min(1)] private int width = 3;
        [SerializeField, Min(1)] private int height = 1;
        [SerializeField] private int baseCost = 1;
        [SerializeField] private string color = "basic";

        public override bool TryPlan(EnemyTurnContext context)
        {
            if (!TrySelectTarget(context, targetSelector, out AbstractEntity target))
                return false;

            if (!EnemyBrainLine.TryFindDirectLine(
                    context,
                    context.SimulatedPosition,
                    context.GetEntityPosition(target),
                    Mathf.Max(1, range),
                    out string direction,
                    out int distance))
            {
                return false;
            }

            bool added = false;
            foreach (int attackDistance in GetAttackDistances(distance))
            {
                foreach (string attackDirection in GetAttackDirections(direction))
                {
                    if (!CanPlanAttack(context, attackDirection, attackDistance))
                        continue;

                    added |= context.AddAction(
                        new AttackAction(baseCost, color, context.Self, attackDirection, attackDistance, damage)
                    );
                }
            }

            return added;
        }

        private List<string> GetAttackDirections(string centerDirection)
        {
            List<string> attackDirections = new List<string>();
            int clampedWidth = Mathf.Clamp(width, 1, HexGridManager.HexDirections.Length);
            int centerIndex = GetDirectionIndex(centerDirection);
            if (centerIndex < 0)
                return attackDirections;

            for (int i = 0; i < clampedWidth; i++)
            {
                int offset = GetCenteredOffset(i);
                int directionIndex = PositiveModulo(centerIndex + offset, HexGridManager.HexDirections.Length);
                attackDirections.Add(HexGridManager.HexDirections[directionIndex]);
            }

            return attackDirections;
        }

        private List<int> GetAttackDistances(int centerDistance)
        {
            List<int> attackDistances = new List<int>();
            int clampedHeight = Mathf.Max(1, height);
            int distancesBeforeCenter = (clampedHeight - 1) / 2;
            int startDistance = Mathf.Max(1, centerDistance - distancesBeforeCenter);

            for (int i = 0; i < clampedHeight; i++)
            {
                attackDistances.Add(startDistance + i);
            }

            return attackDistances;
        }

        private bool CanPlanAttack(EnemyTurnContext context, string direction, int distance)
        {
            if (context == null || string.IsNullOrEmpty(direction) || distance <= 0)
                return false;

            Vector2Int targetPosition = HexGridManager.MoveHex(context.SimulatedPosition, direction, distance);
            if (!context.IsBoardPosition(targetPosition))
                return false;

            context.State.EntitiesOnHex(targetPosition, out List<AbstractEntity> entitiesOnHex);
            foreach (AbstractEntity entity in entitiesOnHex)
            {
                if (entity == null || entity == context.Self || entity.Health <= 0)
                    continue;

                if (entity.entityType == context.Self.entityType)
                    return false;
            }

            return true;
        }

        private int GetDirectionIndex(string direction)
        {
            for (int i = 0; i < HexGridManager.HexDirections.Length; i++)
            {
                if (HexGridManager.HexDirections[i] == direction)
                    return i;
            }

            return -1;
        }

        private int GetCenteredOffset(int index)
        {
            if (index == 0)
                return 0;

            int magnitude = (index + 1) / 2;
            return index % 2 == 1 ? magnitude : -magnitude;
        }

        private int PositiveModulo(int value, int modulo)
        {
            return (value % modulo + modulo) % modulo;
        }
    }
}
