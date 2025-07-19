using System;
using System.Collections;
using System.Collections.Generic;
using Types.Actions;

namespace Entities
{
    public abstract class Enemy: AbstractEntity
    {
        protected Random random = new Random();
        public List<AbstractAction> AvailableActions = new List<AbstractAction>();

        public override void Die()
        {
            Destroy(gameObject);
        }

        public override void Damage(int damage)
        {
            base.Damage(damage);
            if (damage == 0)
            {
                Die();
            }
        }
        
        public abstract IEnumerator MakeTurn();
        public abstract List<AbstractAction> NextTurn();
    }
}