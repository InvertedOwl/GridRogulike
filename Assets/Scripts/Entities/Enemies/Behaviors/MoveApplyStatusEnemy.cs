using System.Collections.Generic;
using Cards.Actions;
using Grid;
using StateManager;
using UnityEngine;

namespace Entities.Enemies
{
    public class MoveApplyStatusEnemy : MoveAttackEnemy
    {
        [Header("Status")]
        public StatusApplicationType statusType = StatusApplicationType.Buffed;
        public int statusAmount = 3;
        public int statusRange = 1;

        private bool _isPlanningAloneTurn;

        protected override void Awake()
        {
            InitializeAttackActions();
        }

        private void Reset()
        {
            targetSearchMode = TargetSearchMode.MoveTowardsClosestEnemy;
            stopMovingWhenWithinDistance = statusRange;
        }

        public override List<AbstractAction> NextTurn()
        {
            if (self.plannedAction == null)
            {
                self.plannedAction = new List<AbstractAction>();
            }

            self.plannedAction.Clear();
            if (IsOnlyLivingEnemy())
            {
                PlanAloneTurn();
            }
            else
            {
                PlanStatusTurn();
            }

            return self.plannedAction;
        }

        protected override bool IsValidTarget(AbstractEntity entity)
        {
            if (_isPlanningAloneTurn)
            {
                return entity != null &&
                       entity != self &&
                       (entity.entityType == EntityType.Player || entity.entityType == EntityType.Friendly) &&
                       entity.Health > 0;
            }

            return entity != null &&
                   entity != self &&
                   entity.entityType == EntityType.Enemy &&
                   entity.Health > 0;
        }

        private void PlanAloneTurn()
        {
            _isPlanningAloneTurn = true;
            try
            {
                if (IsTargetNearby(attackRange) && TryPlanAttack())
                {
                    return;
                }
            }
            finally
            {
                _isPlanningAloneTurn = false;
            }

            PlanApplyStatus(self);
        }

        private void PlanStatusTurn()
        {
            PlayingState state = GameStateManager.Instance.GetCurrent<PlayingState>();
            AbstractEntity target = GetClosestTarget(state);

            if (target == null)
            {
                return;
            }

            Dictionary<Vector2Int, int> distanceMap =
                self.CalculateDistanceMap(target.positionRowCol, state, target);

            Vector2Int currentPosition = self.positionRowCol;

            if (IsWithinStatusRange(currentPosition, distanceMap))
            {
                PlanApplyStatus(target);
                return;
            }

            for (int step = 0; step < maxMovesPerTurn; step++)
            {
                MoveAction bestMove = GetBestMove(currentPosition, distanceMap);

                if (bestMove == null)
                {
                    break;
                }

                currentPosition = HexGridManager.MoveHex(currentPosition, bestMove.Direction, 1);
                self.plannedAction.Add(bestMove);
            }
        }

        private bool IsWithinStatusRange(Vector2Int position, Dictionary<Vector2Int, int> distanceMap)
        {
            return distanceMap.TryGetValue(position, out int distance) &&
                   distance >= 0 &&
                   distance <= statusRange;
        }

        private void PlanApplyStatus(AbstractEntity target)
        {
            self.plannedAction.Add(
                new ApplyStatusToEntityAction(1, "basic", self, target, statusType, statusAmount)
            );
        }
    }
}
