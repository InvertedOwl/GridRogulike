using System.Collections.Generic;
using Cards.Actions;
using Cards.CardEvents;
using Grid;
using StateManager;
using UnityEngine;

namespace Entities.Enemies
{
    public static class EnemyActionValidator
    {
        public static bool CanExecuteAction(
            AbstractAction action,
            NonPlayerEntity self,
            PlayingState state,
            Vector2Int? plannedSource)
        {
            if (action == null || self == null || state == null || self.Health <= 0)
                return false;

            if (plannedSource.HasValue && self.positionRowCol != plannedSource.Value)
                return false;

            if (action is MoveAction moveAction)
                return CanExecuteMove(moveAction, self, state);

            if (action is AttackAction attackAction)
                return CanExecuteAttack(attackAction, self, state);

            if (action is ApplyStatusToEntityAction applyStatusAction)
                return applyStatusAction.target != null && applyStatusAction.target.Health > 0;

            return true;
        }

        public static bool CanExecuteEvent(
            AbstractCardEvent cardEvent,
            NonPlayerEntity self,
            PlayingState state)
        {
            if (cardEvent == null || self == null || state == null || self.Health <= 0)
                return false;

            if (cardEvent is AttackCardEvent attackEvent)
            {
                if (!attackEvent.usePosition &&
                    !HasClearLineToAttackDistance(self, state, attackEvent.direction, attackEvent.distance))
                {
                    return false;
                }

                Vector2Int targetPosition = attackEvent.usePosition
                    ? attackEvent.position
                    : HexGridManager.MoveHex(self.positionRowCol, attackEvent.direction, attackEvent.distance);
                return IsValidAttackTile(self, state, targetPosition);
            }

            return true;
        }

        private static bool CanExecuteMove(MoveAction action, NonPlayerEntity self, PlayingState state)
        {
            Vector2Int targetPosition = HexGridManager.MoveHex(
                self.positionRowCol,
                action.Direction,
                action.Distance);

            return state.IsValidHex(targetPosition);
        }

        private static bool CanExecuteAttack(AttackAction action, NonPlayerEntity self, PlayingState state)
        {
            if (!HasClearLineToAttackDistance(self, state, action.Direction, action.Distance))
                return false;

            Vector2Int targetPosition = HexGridManager.MoveHex(
                self.positionRowCol,
                action.Direction,
                action.Distance);

            return IsValidAttackTile(self, state, targetPosition);
        }

        private static bool HasClearLineToAttackDistance(
            NonPlayerEntity self,
            PlayingState state,
            string direction,
            int distance)
        {
            if (string.IsNullOrEmpty(direction) || distance <= 0)
                return false;

            for (int currentDistance = 1; currentDistance < distance; currentDistance++)
            {
                Vector2Int tile = HexGridManager.MoveHex(self.positionRowCol, direction, currentDistance);
                if (HexGridManager.Instance == null ||
                    !HexGridManager.Instance.BoardDictionary.ContainsKey(tile))
                {
                    return false;
                }

                state.EntitiesOnHex(tile, out List<AbstractEntity> entitiesOnHex);
                foreach (AbstractEntity entity in entitiesOnHex)
                {
                    if (IsAttackLineBlockedByEntity(entity, self))
                        return false;
                }
            }

            return true;
        }

        private static bool IsAttackLineBlockedByEntity(AbstractEntity entity, NonPlayerEntity self)
        {
            return entity != null &&
                   entity != self &&
                   entity.Health > 0 &&
                   entity.entityType != EntityType.Player;
        }

        private static bool IsValidAttackTile(
            NonPlayerEntity self,
            PlayingState state,
            Vector2Int targetPosition)
        {
            if (HexGridManager.Instance == null ||
                !HexGridManager.Instance.BoardDictionary.ContainsKey(targetPosition))
            {
                return false;
            }

            state.EntitiesOnHex(targetPosition, out List<AbstractEntity> entitiesOnHex);
            foreach (AbstractEntity entity in entitiesOnHex)
            {
                if (entity == null || entity == self || entity.Health <= 0)
                    continue;

                if (entity.entityType == self.entityType)
                    return false;
            }

            return true;
        }
    }
}
