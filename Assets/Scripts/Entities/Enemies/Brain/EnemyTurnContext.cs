using System.Collections.Generic;
using Cards.Actions;
using Grid;
using StateManager;
using UnityEngine;

namespace Entities.Enemies
{
    public class EnemyTurnContext
    {
        public NonPlayerEntity Self { get; }
        public PlayingState State { get; }
        public Vector2Int SimulatedPosition { get; private set; }
        public HashSet<Vector2Int> OccupiedPositions { get; }
        public List<AbstractAction> PlannedActions { get; }
        public Dictionary<AbstractAction, Vector2Int> ActionSourcePositions { get; }
        public RandomState PlanningRandom { get; }
        public int PlannedActionRevision { get; private set; }
        public int MoveBudget { get; private set; }
        private readonly IReadOnlyDictionary<AbstractEntity, Vector2Int> _plannedEntityPositions;

        public EnemyTurnContext(
            NonPlayerEntity self,
            PlayingState state,
            int moveBudget,
            IReadOnlyDictionary<AbstractEntity, Vector2Int> plannedEntityPositions = null)
        {
            Self = self;
            State = state;
            SimulatedPosition = self != null ? self.positionRowCol : Vector2Int.zero;
            MoveBudget = Mathf.Max(0, moveBudget);
            OccupiedPositions = new HashSet<Vector2Int>();
            PlannedActions = new List<AbstractAction>();
            ActionSourcePositions = new Dictionary<AbstractAction, Vector2Int>();
            PlanningRandom = self != null ? self.EntityRandom.Clone() : RunInfo.NewRandom("enemy-brain");
            _plannedEntityPositions = plannedEntityPositions;

            if (state == null)
                return;

            foreach (AbstractEntity entity in state.GetEntities())
            {
                if (entity == null || entity == self || entity.Health <= 0)
                    continue;

                OccupiedPositions.Add(GetEntityPosition(entity));
            }
        }

        public bool AddAction(AbstractAction action)
        {
            if (action == null || Self == null)
                return false;

            if (action is AttackAction attackAction &&
                TryResolveAttackTarget(attackAction, SimulatedPosition, out Vector2Int attackTarget) &&
                TryHandleDuplicateAttackOnTarget(attackAction, attackTarget, out bool plannedAttack))
            {
                return plannedAttack;
            }

            action.entity = Self;
            PlannedActions.Add(action);
            ActionSourcePositions[action] = SimulatedPosition;
            PlannedActionRevision++;
            return true;
        }

        private bool TryHandleDuplicateAttackOnTarget(
            AttackAction newAttack,
            Vector2Int newTarget,
            out bool plannedAttack)
        {
            plannedAttack = false;

            for (int i = 0; i < PlannedActions.Count; i++)
            {
                if (PlannedActions[i] is not AttackAction existingAttack)
                    continue;

                if (!ActionSourcePositions.TryGetValue(existingAttack, out Vector2Int existingSource))
                    existingSource = SimulatedPosition;

                if (!TryResolveAttackTarget(existingAttack, existingSource, out Vector2Int existingTarget) ||
                    existingTarget != newTarget)
                {
                    continue;
                }

                if (existingAttack.Amount >= newAttack.Amount)
                    return true;

                newAttack.entity = Self;
                ActionSourcePositions.Remove(existingAttack);
                PlannedActions[i] = newAttack;
                ActionSourcePositions[newAttack] = SimulatedPosition;
                PlannedActionRevision++;
                plannedAttack = true;
                return true;
            }

            return false;
        }

        private bool TryResolveAttackTarget(
            AttackAction attack,
            Vector2Int source,
            out Vector2Int target)
        {
            target = Vector2Int.zero;
            if (attack == null || string.IsNullOrEmpty(attack.Direction) || attack.Distance <= 0)
                return false;

            target = HexGridManager.MoveHex(source, attack.Direction, attack.Distance);
            return true;
        }

        public bool TryAddMove(string direction, int distance = 1)
        {
            distance = Mathf.Max(1, distance);
            if (MoveBudget < distance)
                return false;

            Vector2Int destination = HexGridManager.MoveHex(SimulatedPosition, direction, distance);
            if (!IsOpen(destination))
                return false;

            MoveAction action = new MoveAction(1, "basic", Self, direction, distance);
            if (!AddAction(action))
                return false;

            SimulatedPosition = destination;
            MoveBudget -= distance;
            return true;
        }

        public bool IsBoardPosition(Vector2Int position)
        {
            return HexGridManager.Instance != null &&
                   HexGridManager.Instance.BoardDictionary.ContainsKey(position);
        }

        public bool IsOccupied(Vector2Int position)
        {
            return OccupiedPositions.Contains(position);
        }

        public bool IsOpen(Vector2Int position)
        {
            return IsBoardPosition(position) && !IsOccupied(position);
        }

        public bool IsAttackLineBlocked(Vector2Int position)
        {
            if (State == null)
                return IsOccupied(position);

            foreach (AbstractEntity entity in State.GetEntities())
            {
                if (entity == null || entity == Self || entity.Health <= 0)
                    continue;

                if (entity.entityType == EntityType.Player)
                    continue;

                if (GetEntityPosition(entity) == position)
                    return true;
            }

            return false;
        }

        public Vector2Int GetEntityPosition(AbstractEntity entity)
        {
            if (entity == null)
                return Vector2Int.zero;

            if (entity == Self)
                return SimulatedPosition;

            if (_plannedEntityPositions != null &&
                _plannedEntityPositions.TryGetValue(entity, out Vector2Int plannedPosition))
            {
                return plannedPosition;
            }

            return entity.positionRowCol;
        }

        public List<Vector2Int> GetBlockers(AbstractEntity excludedTarget = null)
        {
            List<Vector2Int> blockers = new List<Vector2Int>();

            if (State == null)
                return blockers;

            foreach (AbstractEntity entity in State.GetEntities())
            {
                if (entity == null || entity == Self || entity == excludedTarget || entity.Health <= 0)
                    continue;

                blockers.Add(GetEntityPosition(entity));
            }

            return blockers;
        }

        public bool TryGetDistanceTo(AbstractEntity target, out int distance)
        {
            distance = -1;
            if (target == null || HexGridManager.Instance == null)
                return false;

            Dictionary<Vector2Int, int> distanceMap =
                HexGridManager.Instance.CalculateDistanceMap(GetEntityPosition(target), GetBlockers(target));

            return distanceMap.TryGetValue(SimulatedPosition, out distance) && distance >= 0;
        }

        public void CommitPlanningRandom()
        {
            if (Self?.EntityRandom == null || PlanningRandom == null)
                return;

            Self.EntityRandom.CopyFrom(PlanningRandom);
        }
    }
}
