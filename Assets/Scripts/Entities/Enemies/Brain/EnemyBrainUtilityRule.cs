using Cards.Actions;
using UnityEngine;

namespace Entities.Enemies
{
    public abstract class EnemyBrainUtilityRule : EnemyBrainRule
    {
        protected bool TrySelectUtilityTarget(
            EnemyTurnContext context,
            EnemyBrainTargetSelector selector,
            out AbstractEntity target)
        {
            return TrySelectTarget(context, selector, out target);
        }

        protected bool TryAddUtilityAction(EnemyTurnContext context, AbstractAction action)
        {
            return context != null && context.AddAction(action);
        }

        protected bool TryAddShield(
            EnemyTurnContext context,
            int amount,
            int baseCost,
            string color)
        {
            return context != null &&
                   TryAddUtilityAction(context, new ShieldAction(baseCost, color, context.Self, amount));
        }

        protected bool TryAddBuffSelf(
            EnemyTurnContext context,
            int amount,
            int baseCost,
            string color)
        {
            return context != null &&
                   TryAddUtilityAction(context, new BuffSelfAction(baseCost, color, context.Self, amount));
        }

        protected bool TryAddStatusToSelf(
            EnemyTurnContext context,
            StatusApplicationType statusType,
            int statusAmount,
            int baseCost,
            string color)
        {
            return context != null &&
                   TryAddStatusToTarget(context, context.Self, statusType, statusAmount, baseCost, color);
        }

        protected bool TryAddStatusToTarget(
            EnemyTurnContext context,
            AbstractEntity target,
            StatusApplicationType statusType,
            int statusAmount,
            int baseCost,
            string color)
        {
            return context != null &&
                   target != null &&
                   TryAddUtilityAction(
                       context,
                       new ApplyStatusToFixedEntityAction(
                           baseCost,
                           color,
                           context.Self,
                           target,
                           statusType,
                           statusAmount)
                   );
        }

        protected bool IsTargetInRange(
            EnemyTurnContext context,
            AbstractEntity target,
            int range,
            out int distance)
        {
            distance = -1;
            return context != null &&
                   target != null &&
                   context.TryGetDistanceTo(target, out distance) &&
                   distance <= Mathf.Max(0, range);
        }
    }
}
