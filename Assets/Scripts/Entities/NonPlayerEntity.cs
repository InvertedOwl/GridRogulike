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
            {
                HideIntentIndicator();
                return;
            }

            AbstractAction actionChosen = plannedAction[plannedAction.Count - 1];

            if (actionChosen is AttackAction)
            {
                HideIntentIndicator();
                return;
            }
            
            

            // plannedActionSprite.sprite = SpriteDatabase.Get(actionChosen.Icon).Value.sprite;
            plannedActionSprite.GetComponent<EaseScale>().SetScale(Vector3.one);
            plannedActionSprite.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = actionChosen.ToSimpleText();
            LayoutRebuilder.ForceRebuildLayoutImmediate(plannedActionSprite.GetComponent<RectTransform>());
        }

        private void HideIntentIndicator()
        {
            if (plannedActionSprite == null)
                return;

            plannedActionSprite.GetComponent<EaseScale>().SetScale(Vector3.zero);
        }

        public void RemoveIntent()
        {
            StartCoroutine(RemoveIntentLate());
        }

        public IEnumerator RemoveIntentLate()
        {
            yield return new WaitForSeconds(0.1f);
            HideIntentIndicator();
        }
    }
}
