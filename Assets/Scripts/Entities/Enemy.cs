using System;
using System.Collections;
using System.Collections.Generic;
using Cards.Actions;

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
        }
        
        public abstract IEnumerator MakeTurn();
        public abstract List<AbstractAction> NextTurn();
    }
}