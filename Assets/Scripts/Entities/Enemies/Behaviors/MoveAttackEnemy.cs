using System.Collections;
using System.Collections.Generic;
using Cards.Actions;
using Cards.CardEvents;
using Grid;
using StateManager;
using UnityEngine;

namespace Entities.Enemies
{
    public class MoveAttackEnemy : MoveTowardsPlayerEnemy
    {
        [Header("Attack")]
        public int attackRange = 1;
        public int defaultDamage = 10;
        public float actionDelay = 0.5f;

        private void Awake()
        {
            InitializeAttackActions();
        }

        private void InitializeAttackActions()
        {
            self.AvailableActions.Clear();

            foreach (string direction in HexDirections)
            {
                self.AvailableActions.Add(
                    new AttackAction(1, "basic", self, direction, attackRange, defaultDamage)
                );
            }
        }
        

        public override List<AbstractAction> NextTurn()
        {
            if (self.plannedAction == null)
            {
                self.plannedAction = new List<AbstractAction>();
            }

            self.plannedAction.Clear();

            if (IsTargetNearby(attackRange) && TryPlanAttack())
            {
                return self.plannedAction;
            }

            return base.NextTurn();
        }

        private bool TryPlanAttack()
        {
            foreach (AttackAction attackAction in self.AvailableActions)
            {
                if (CanAttackPlayer(attackAction))
                {
                    self.plannedAction.Add(attackAction);
                    return true;
                }
            }

            return false;
        }

        private bool CanAttackPlayer(AttackAction attackAction)
        {
            Vector2Int targetHex = HexGridManager.MoveHex(
                self.positionRowCol,
                attackAction.Direction,
                attackAction._distance
            );

            PlayingState state = GameStateManager.Instance.GetCurrent<PlayingState>();
            state.EntitiesOnHex(targetHex, out List<AbstractEntity> entitiesOnHex);

            foreach (AbstractEntity entity in entitiesOnHex)
            {
                if (entity.entityType == EntityType.Player)
                {
                    return true;
                }
            }

            return false;
        }

        private void HandlePreEventVisual(AbstractCardEvent cardEvent)
        {
            if (cardEvent is not AttackCardEvent attackCardEvent)
            {
                return;
            }

            Vector2Int targetPos = HexGridManager.MoveHex(
                self.positionRowCol,
                attackCardEvent.direction,
                attackCardEvent.distance
            );

            transform.localPosition +=
                ((Vector3)HexGridManager.GetHexCenter(targetPos.x, targetPos.y) - transform.position)
                .normalized * 0.5f;
        }
    }
}