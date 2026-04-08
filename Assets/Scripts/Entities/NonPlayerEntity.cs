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
using UnityEngine.UI;
using Util;

namespace Entities
{
    public class NonPlayerEntity: AbstractEntity
    {
        public Image plannedActionSprite;
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

        
        // public abstract IEnumerator MakeTurn();
        // public abstract List<AbstractAction> NextTurn();

        public void SetIntent()
        {
            // return;
            if (plannedAction.Count == 0)
                return;
            AbstractAction actionChosen = plannedAction[plannedAction.Count - 1];

            if (actionChosen is AttackAction)
                return;
            
            

            // plannedActionSprite.sprite = SpriteDatabase.Get(actionChosen.Icon).Value.sprite;
            plannedActionSprite.GetComponent<EaseScale>().SetScale(Vector3.one);
            plannedActionSprite.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = actionChosen.ToSimpleText();
            LayoutRebuilder.ForceRebuildLayoutImmediate(plannedActionSprite.GetComponent<RectTransform>());
        }

        public void RemoveIntent()
        {
            // return;
            plannedActionSprite.GetComponent<EaseScale>().SetScale(Vector3.zero);
        }
    }
}