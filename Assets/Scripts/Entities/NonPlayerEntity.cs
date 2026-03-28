using System;
using System.Collections;
using System.Collections.Generic;
using Cards.Actions;
using Cards.CardEvents;
using Grid;
using ScriptableObjects;
using StateManager;
using TMPro;
using Types.Statuses;
using UnityEngine;
using Util;

namespace Entities
{
    public abstract class NonPlayerEntity: AbstractEntity
    {
        public SpriteRenderer plannedActionSprite;
        public SpriteDatabase SpriteDatabase;
        
        public List<AbstractAction> AvailableActions = new List<AbstractAction>();
        

        public override void Damage(int damage, AbstractStatus status)
        {
            BattleStats.DamageDoneThisBattle += damage;
            BattleStats.DamageDoneThisTurn += damage;
            base.Damage(damage, status);
        }


        public List<AbstractCardEvent> ModifyEvents(List<AbstractCardEvent> events)
        {
            List<AbstractCardEvent> modifiedEvents = new List<AbstractCardEvent>(events);

            foreach (AbstractStatus status in statusManager.statusList)
            {
                modifiedEvents = status.Modify(modifiedEvents);
            }
            
            return modifiedEvents;
        }

        public override void Die()
        {
            base.Die();
            RemoveIntent();
            
        }

        
        public abstract IEnumerator MakeTurn();
        public abstract List<AbstractAction> NextTurn();

        public void SetIntent()
        {
            AbstractAction actionChosen = _plannedAction[_plannedAction.Count - 1];


            plannedActionSprite.sprite = SpriteDatabase.Get(actionChosen.Icon).Value.sprite;
            plannedActionSprite.transform.parent.GetComponent<EaseScale>().SetScale(Vector3.one);
        }

        public void RemoveIntent()
        {
            plannedActionSprite.transform.parent.GetComponent<EaseScale>().SetScale(Vector3.zero);
        }
    }
}