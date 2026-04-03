using System.Collections;
using System.Collections.Generic;
using Cards.Actions;
using Cards.CardEvents;
using Grid;
using StateManager;
using UnityEngine;
using Util;

namespace Entities.Enemies
{
    public class MoveAttackWideEnemy : MoveTowardsPlayerEnemy
    {
        [Header("Attack")]
        public int defaultDamage = 10;

        private void Awake()
        {

            self.AvailableActions.Clear();
            self.AvailableActions.Add(new AttackAction(1, "basic", self, "n", 1, defaultDamage));
            self.AvailableActions.Add(new AttackAction(1, "basic", self, "s", 1, defaultDamage));
            self.AvailableActions.Add(new AttackAction(1, "basic", self, "ne", 1, defaultDamage));
            self.AvailableActions.Add(new AttackAction(1, "basic", self, "nw", 1, defaultDamage));
            self.AvailableActions.Add(new AttackAction(1, "basic", self, "se", 1, defaultDamage));
            self.AvailableActions.Add(new AttackAction(1, "basic", self, "sw", 1, defaultDamage));
        }

        public override IEnumerator MakeTurn()
        {
            if (self.plannedAction == null || self.plannedAction.Count == 0)
            {
                yield break;
            }

            foreach (AbstractAction action in self.plannedAction)
            {
                foreach (AbstractCardEvent cardEvent in action.Activate(null))
                {
                    if (cardEvent is AttackCardEvent attackCardEvent)
                    {
                        Vector2Int targetPos = HexGridManager.MoveHex(
                            self.positionRowCol,
                            attackCardEvent.direction,
                            attackCardEvent.distance
                        );

                        transform.localPosition +=
                            ((Vector3)HexGridManager.GetHexCenter(targetPos.x, targetPos.y) - transform.position)
                            .normalized * 0.5f;
                    }

                    foreach (AbstractCardEvent modifiedEvent in self.ModifyEvents(new List<AbstractCardEvent> { cardEvent }))
                    {
                        modifiedEvent.Activate(self);
                    }

                    yield return new WaitForSeconds(0.5f * (1 / GameplayNavSettings.speed));
                }
            }

            self.plannedAction.Clear();
        }

        private void PlanTurn()
        {
            self.plannedAction.Clear();

            foreach (AttackAction action in self.AvailableActions)
            {
                string dir = action.Direction;

                List<AbstractEntity> entitiesOnHex = new List<AbstractEntity>();
                GameStateManager.Instance.GetCurrent<PlayingState>().EntitiesOnHex(
                    HexGridManager.MoveHex(self.positionRowCol, dir, 1),
                    out entitiesOnHex
                );

                foreach (AbstractEntity e in entitiesOnHex)
                {
                    if (e.entityType == EntityType.Player)
                    {
                        self.plannedAction.Add(action);
                    }
                }
            }

            if (self.plannedAction.Count > 0)
            {
                AttackAction attack = (AttackAction)self.plannedAction[0];

                foreach (string dir in HexGridManager.neighborDirections[attack.Direction])
                {
                    self.plannedAction.Add(new AttackAction(1, "basic", self, dir, 1, defaultDamage));
                }

                return;
            }
            
            PlanMovementTowardsPlayer();
        }

        public override List<AbstractAction> NextTurn()
        {
            if (self.plannedAction == null)
            {
                self.plannedAction = new List<AbstractAction>();
            }

            if (self.plannedAction.Count == 0)
            {
                PlanTurn();
            }

            return self.plannedAction;
        }
    }
}