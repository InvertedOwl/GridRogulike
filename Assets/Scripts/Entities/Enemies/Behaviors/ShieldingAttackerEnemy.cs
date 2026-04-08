using System.Collections.Generic;
using Cards.Actions;
using UnityEngine;

namespace Entities.Enemies
{
    public class ShieldingAttackerEnemy : MoveAttackEnemy
    {
        [Header("Shield")] public int shieldAmount;

        private bool shieldingThisTurn = true;

        public override List<AbstractAction> NextTurn()
        {
            if (self.plannedAction == null)
            {
                self.plannedAction = new List<AbstractAction>();
            }
            self.plannedAction.Clear();

            if (IsTargetNearby(attackRange))
            {
                shieldingThisTurn = !shieldingThisTurn;
                if (shieldingThisTurn)
                {
                    PlanShield();
                    return self.plannedAction;
                }
            }
            
            return base.NextTurn();
        }

        private void PlanShield()
        {
            self.plannedAction.Add(new ShieldAction(1, "basic", self, shieldAmount));
        }
    }
}