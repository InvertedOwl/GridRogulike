using System.Collections.Generic;
using Cards.Actions;
using Grid;
using UnityEngine;

namespace Entities.Enemies
{
    public abstract class EnemyBrainAttackRule : EnemyBrainRule
    {
        [SerializeField]
        [Tooltip("When enabled, this rule still plans an attack if no direct hit exists by choosing the attack tile closest to the target.")]
        private bool alwaysPlan;

        protected bool TrySelectAttackTarget(
            EnemyTurnContext context,
            EnemyBrainTargetSelector selector,
            out AbstractEntity target)
        {
            return TrySelectTarget(context, selector, out target);
        }

        protected bool TryFindDirectAttackLine(
            EnemyTurnContext context,
            AbstractEntity target,
            int range,
            out string direction,
            out int distance)
        {
            direction = "";
            distance = 0;

            return context != null &&
                   target != null &&
                   (EnemyBrainLine.TryFindDirectLine(
                        context,
                        context.SimulatedPosition,
                        context.GetEntityPosition(target),
                        ClampRange(range),
                        out direction,
                        out distance) ||
                    (alwaysPlan &&
                     TryFindClosestAttackLine(
                         context,
                         target,
                         ClampRange(range),
                         out direction,
                         out distance)));
        }

        protected bool TryFindDirectAttackLine(
            EnemyTurnContext context,
            EnemyBrainTargetSelector selector,
            int range,
            out AbstractEntity target,
            out string direction,
            out int distance)
        {
            direction = "";
            distance = 0;

            return TrySelectAttackTarget(context, selector, out target) &&
                   TryFindDirectAttackLine(context, target, range, out direction, out distance);
        }

        protected bool TryAddAttack(
            EnemyTurnContext context,
            string direction,
            int distance,
            int damage,
            int baseCost,
            string color)
        {
            if (context == null || string.IsNullOrEmpty(direction) || distance <= 0)
                return false;

            return context.AddAction(
                new DirectionalAttackAction(baseCost, color, context.Self, direction, distance, damage)
            );
        }

        protected bool TryAddStatusAttack(
            EnemyTurnContext context,
            string direction,
            int distance,
            int damage,
            StatusApplicationType statusType,
            int statusAmount,
            int baseCost,
            string color)
        {
            if (context == null || string.IsNullOrEmpty(direction) || distance <= 0)
                return false;

            return context.AddAction(
                new DirectionalStatusAttackAction(
                    baseCost,
                    color,
                    context.Self,
                    direction,
                    distance,
                    damage,
                    statusType,
                    statusAmount)
            );
        }

        protected bool TryAddAttackIfValid(
            EnemyTurnContext context,
            string direction,
            int distance,
            int damage,
            int baseCost,
            string color)
        {
            return CanPlanAttack(context, direction, distance) &&
                   TryAddAttack(context, direction, distance, damage, baseCost, color);
        }

        protected int AddLineAttackActions(
            EnemyTurnContext context,
            string direction,
            int range,
            int damage,
            int baseCost,
            string color)
        {
            return EnemyBrainLine.AddLineAttackActions(
                context,
                direction,
                ClampRange(range),
                damage,
                baseCost,
                color);
        }

        protected bool CanPlanAttack(EnemyTurnContext context, string direction, int distance)
        {
            if (context == null || context.State == null || string.IsNullOrEmpty(direction) || distance <= 0)
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

        protected List<string> GetCenteredDirections(string centerDirection, int width)
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

        protected List<int> GetCenteredDistances(int centerDistance, int height)
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

        protected int ClampRange(int range)
        {
            return Mathf.Max(1, range);
        }

        protected bool TryFindClosestAttackLine(
            EnemyTurnContext context,
            AbstractEntity target,
            int range,
            out string direction,
            out int distance)
        {
            direction = "";
            distance = 0;

            if (context == null || target == null || HexGridManager.Instance == null)
                return false;

            Vector2Int targetPosition = context.GetEntityPosition(target);
            Dictionary<Vector2Int, int> targetDistanceMap =
                HexGridManager.Instance.CalculateDistanceMap(targetPosition, new List<Vector2Int>());

            int bestScore = int.MaxValue;
            List<AttackLineCandidate> bestCandidates = new List<AttackLineCandidate>();

            foreach (string possibleDirection in HexGridManager.HexDirections)
            {
                for (int currentDistance = 1; currentDistance <= range; currentDistance++)
                {
                    Vector2Int attackTile =
                        HexGridManager.MoveHex(context.SimulatedPosition, possibleDirection, currentDistance);

                    if (!context.IsBoardPosition(attackTile))
                        break;

                    if (CanPlanAttack(context, possibleDirection, currentDistance) &&
                        targetDistanceMap.TryGetValue(attackTile, out int score) &&
                        score >= 0)
                    {
                        if (score < bestScore)
                        {
                            bestScore = score;
                            bestCandidates.Clear();
                            bestCandidates.Add(new AttackLineCandidate(possibleDirection, currentDistance));
                        }
                        else if (score == bestScore)
                        {
                            bestCandidates.Add(new AttackLineCandidate(possibleDirection, currentDistance));
                        }
                    }

                    if (context.IsAttackLineBlocked(attackTile))
                        break;
                }
            }

            if (bestCandidates.Count == 0)
                return false;

            int index = context.PlanningRandom.Next(0, bestCandidates.Count);
            direction = bestCandidates[index].Direction;
            distance = bestCandidates[index].Distance;
            return true;
        }

        private readonly struct AttackLineCandidate
        {
            public string Direction { get; }
            public int Distance { get; }

            public AttackLineCandidate(string direction, int distance)
            {
                Direction = direction;
                Distance = distance;
            }
        }

        protected int GetDirectionIndex(string direction)
        {
            for (int i = 0; i < HexGridManager.HexDirections.Length; i++)
            {
                if (HexGridManager.HexDirections[i] == direction)
                    return i;
            }

            return -1;
        }

        protected int GetCenteredOffset(int index)
        {
            if (index == 0)
                return 0;

            int magnitude = (index + 1) / 2;
            return index % 2 == 1 ? magnitude : -magnitude;
        }

        protected int PositiveModulo(int value, int modulo)
        {
            return (value % modulo + modulo) % modulo;
        }
    }
}
