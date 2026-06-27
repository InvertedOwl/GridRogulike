using UnityEngine;

namespace Entities.Enemies
{
    public abstract class EnemyBrainMoveRule : EnemyBrainRule
    {
        protected bool TrySelectMoveTarget(
            EnemyTurnContext context,
            EnemyBrainTargetSelector selector,
            out AbstractEntity target)
        {
            return TrySelectTarget(context, selector, out target);
        }

        protected bool TryMoveTowardTarget(
            EnemyTurnContext context,
            EnemyBrainTargetSelector selector,
            int stopDistance,
            int maxMoves)
        {
            int clampedStopDistance = ClampDistance(stopDistance);
            if (!TrySelectMoveTarget(context, selector, out AbstractEntity target))
                return false;

            if (EnemyBrainMovement.TryMoveTowardTarget(
                    context,
                    target,
                    clampedStopDistance,
                    maxMoves))
            {
                return true;
            }

            return IsAllySelector(selector) &&
                   IsTargetWithinDistance(context, target, clampedStopDistance);
        }

        protected bool TryMoveAwayFromTarget(
            EnemyTurnContext context,
            EnemyBrainTargetSelector selector,
            int minimumDistance,
            int maxMoves)
        {
            return TrySelectMoveTarget(context, selector, out AbstractEntity target) &&
                   EnemyBrainMovement.TryMoveAwayFromTarget(
                       context,
                       target,
                       ClampDistance(minimumDistance),
                       maxMoves);
        }

        protected bool TryMoveToPreferredDistance(
            EnemyTurnContext context,
            EnemyBrainTargetSelector selector,
            int preferredDistance,
            int maxMoves)
        {
            return TrySelectMoveTarget(context, selector, out AbstractEntity target) &&
                   EnemyBrainMovement.TryMoveToPreferredDistance(
                       context,
                       target,
                       ClampDistance(preferredDistance),
                       maxMoves);
        }

        protected bool TryMoveIntoDirectLine(
            EnemyTurnContext context,
            EnemyBrainTargetSelector selector,
            int attackRange,
            int preferredDistance,
            int maxMoves)
        {
            return TrySelectMoveTarget(context, selector, out AbstractEntity target) &&
                   EnemyBrainMovement.TryMoveIntoDirectLine(
                       context,
                       target,
                       Mathf.Max(1, attackRange),
                       ClampDistance(preferredDistance),
                       maxMoves);
        }

        protected bool TryMoveAlongCurrentDistance(
            EnemyTurnContext context,
            EnemyBrainTargetSelector selector,
            int maxMoves)
        {
            return TrySelectMoveTarget(context, selector, out AbstractEntity target) &&
                   EnemyBrainMovement.TryMoveAlongCurrentDistance(context, target, maxMoves);
        }

        protected bool TryAddMove(EnemyTurnContext context, string direction, int distance = 1)
        {
            return context != null && context.TryAddMove(direction, distance);
        }

        protected int ClampDistance(int distance)
        {
            return Mathf.Max(0, distance);
        }

        private bool IsAllySelector(EnemyBrainTargetSelector selector)
        {
            return selector == EnemyBrainTargetSelector.ClosestAlly ||
                   selector == EnemyBrainTargetSelector.LowestHealthAlly;
        }

        private bool IsTargetWithinDistance(
            EnemyTurnContext context,
            AbstractEntity target,
            int distance)
        {
            return context != null &&
                   target != null &&
                   context.TryGetDistanceTo(target, out int currentDistance) &&
                   currentDistance <= distance;
        }
    }
}
