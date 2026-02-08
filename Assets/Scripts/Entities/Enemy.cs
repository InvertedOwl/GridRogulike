using System;
using System.Collections;
using System.Collections.Generic;
using Cards.Actions;
using Grid;
using StateManager;
using TMPro;
using Types.Statuses;
using UnityEngine;
using Util;

namespace Entities
{
    public abstract class Enemy: AbstractEntity
    {
        public List<AbstractAction> AvailableActions = new List<AbstractAction>();

        public override void Die()
        {
            Destroy(gameObject);
        }

        public override void Damage(int damage, AbstractStatus status)
        {
            BattleStats.DamageDoneThisBattle += damage;
            BattleStats.DamageDoneThisTurn += damage;
            base.Damage(damage, status);
        }
        
        

        
        public abstract IEnumerator MakeTurn();
        public abstract List<AbstractAction> NextTurn();
    }
}