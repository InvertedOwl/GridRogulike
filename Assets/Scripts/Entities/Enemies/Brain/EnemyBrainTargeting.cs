using System.Collections.Generic;
using Grid;
using UnityEngine;

namespace Entities.Enemies
{
    public enum EnemyBrainTargetSelector
    {
        Player,
        Self,
        ClosestOpponent,
        ClosestAlly,
        LowestHealthAlly,
        ClosestNeutral,
        RandomOpponent
    }

    public static class EnemyBrainTargeting
    {
        public static bool TrySelectTarget(
            EnemyTurnContext context,
            EnemyBrainTargetSelector selector,
            out AbstractEntity target)
        {
            target = null;
            if (context == null || context.Self == null || context.State == null)
                return false;

            switch (selector)
            {
                case EnemyBrainTargetSelector.Player:
                    target = IsLiving(context.State.player) ? context.State.player : null;
                    return target != null;

                case EnemyBrainTargetSelector.Self:
                    target = context.Self;
                    return IsLiving(target);

                case EnemyBrainTargetSelector.ClosestOpponent:
                    return TryGetClosest(context, IsOpponent, out target);

                case EnemyBrainTargetSelector.ClosestAlly:
                    return TryGetClosest(context, IsAlly, out target);

                case EnemyBrainTargetSelector.LowestHealthAlly:
                    return TryGetLowestHealth(context, IsAlly, out target);

                case EnemyBrainTargetSelector.ClosestNeutral:
                    return TryGetClosest(context, IsNeutral, out target);

                case EnemyBrainTargetSelector.RandomOpponent:
                    return TryGetRandom(context, IsOpponent, out target);

                default:
                    return false;
            }
        }

        public static int CountLivingEnemies(EnemyTurnContext context)
        {
            if (context?.State == null)
                return 0;

            int count = 0;
            foreach (AbstractEntity entity in context.State.GetEntities())
            {
                if (IsLiving(entity) && entity.entityType == EntityType.Enemy)
                    count++;
            }

            return count;
        }

        private delegate bool TargetFilter(EnemyTurnContext context, AbstractEntity entity);

        private static bool TryGetClosest(EnemyTurnContext context, TargetFilter filter, out AbstractEntity target)
        {
            target = null;
            int bestDistance = int.MaxValue;

            foreach (AbstractEntity entity in context.State.GetEntities())
            {
                if (!filter(context, entity))
                    continue;

                if (!TryGetPathDistance(context, entity, out int distance))
                    continue;

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    target = entity;
                }
            }

            return target != null;
        }

        private static bool TryGetLowestHealth(EnemyTurnContext context, TargetFilter filter, out AbstractEntity target)
        {
            target = null;
            float lowestHealth = float.MaxValue;

            foreach (AbstractEntity entity in context.State.GetEntities())
            {
                if (!filter(context, entity))
                    continue;

                if (entity.Health < lowestHealth)
                {
                    lowestHealth = entity.Health;
                    target = entity;
                }
            }

            return target != null;
        }

        private static bool TryGetRandom(EnemyTurnContext context, TargetFilter filter, out AbstractEntity target)
        {
            target = null;
            List<AbstractEntity> candidates = new List<AbstractEntity>();

            foreach (AbstractEntity entity in context.State.GetEntities())
            {
                if (filter(context, entity))
                    candidates.Add(entity);
            }

            if (candidates.Count == 0)
                return false;

            int index = context.PlanningRandom.Next(0, candidates.Count);
            target = candidates[index];
            return true;
        }

        private static bool TryGetPathDistance(
            EnemyTurnContext context,
            AbstractEntity target,
            out int distance)
        {
            distance = -1;
            if (target == null || HexGridManager.Instance == null)
                return false;

            Dictionary<Vector2Int, int> distanceMap =
                HexGridManager.Instance.CalculateDistanceMap(
                    context.GetEntityPosition(target),
                    context.GetBlockers(target));

            return distanceMap.TryGetValue(context.SimulatedPosition, out distance) && distance >= 0;
        }

        private static bool IsLiving(AbstractEntity entity)
        {
            return entity != null && entity.Health > 0;
        }

        private static bool IsOpponent(EnemyTurnContext context, AbstractEntity entity)
        {
            if (!IsLiving(entity) || entity == context.Self)
                return false;

            if (context.Self.entityType == EntityType.Enemy)
                return entity.entityType == EntityType.Player || entity.entityType == EntityType.Friendly;

            return entity.entityType == EntityType.Enemy;
        }

        private static bool IsAlly(EnemyTurnContext context, AbstractEntity entity)
        {
            return IsLiving(entity) &&
                   entity != context.Self &&
                   entity.entityType == context.Self.entityType;
        }

        private static bool IsNeutral(EnemyTurnContext context, AbstractEntity entity)
        {
            return IsLiving(entity) && entity.entityType == EntityType.Neutral;
        }
    }
}
