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
        private const float IntentDefaultScale = 0.5f;
        private const float IntentHoveredScale = 1f;
        private static readonly Vector3 IntentDefaultLocalPosition = new Vector3(0f, -31f, 0f);
        private static readonly Vector3 IntentHoveredLocalPosition = Vector3.zero;
        private static readonly Vector3 IntentDefaultLocalScale = Vector3.one * IntentDefaultScale;
        private static readonly Vector3 IntentHoveredLocalScale = Vector3.one * IntentHoveredScale;

        public Image plannedActionSprite;
        public SpriteDatabase SpriteDatabase;
        private bool _intentVisible;
        private bool? _intentHovered;
        
        public List<AbstractAction> AvailableActions = new List<AbstractAction>();
        

        public override void Damage(int damage, bool triggerDamageReceivedStatuses = true)
        {
            BattleStats.DamageDoneThisBattle += damage;
            BattleStats.DamageDoneThisTurn += damage;
            base.Damage(damage, triggerDamageReceivedStatuses);
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
            if (plannedAction == null || plannedAction.Count == 0)
            {
                HideIntentIndicator();
                return;
            }

            if (TryGetLargestPlannedAttackAmount(out int attackAmount))
            {
                ShowIntentText(attackAmount + " <sprite name=Damage4>");
                return;
            }

            AbstractAction actionChosen = plannedAction[plannedAction.Count - 1];
            ShowIntentText(actionChosen.ToSimpleText());
        }

        private bool TryGetLargestPlannedAttackAmount(out int amount)
        {
            amount = 0;

            if (plannedAction == null || plannedAction.Count == 0)
                return false;

            bool foundAttack = false;
            List<AbstractStatus> plannedSelfStatuses = new List<AbstractStatus>();

            foreach (AbstractAction action in plannedAction)
            {
                List<AbstractCardEvent> modifiedEvents = ModifyEvents(
                    action.Activate(null, previewMode: true),
                    previewMode: true
                );

                foreach (AbstractStatus plannedSelfStatus in plannedSelfStatuses)
                {
                    modifiedEvents = plannedSelfStatus.Modify(modifiedEvents, previewMode: true);
                }

                foreach (AbstractCardEvent cardEvent in modifiedEvents)
                {
                    if (cardEvent is ApplyStatusToEntityCardEvent applyStatusEvent &&
                        applyStatusEvent.target == this &&
                        applyStatusEvent.status != null)
                    {
                        plannedSelfStatuses.Add(applyStatusEvent.status);
                    }

                    if (cardEvent is not AttackCardEvent attackCardEvent)
                        continue;

                    amount = Mathf.Max(amount, attackCardEvent.amount);
                    foundAttack = true;
                }
            }

            return foundAttack;
        }

        private void ShowIntentText(string text)
        {
            if (plannedActionSprite == null)
                return;

            plannedActionSprite.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = text;
            LayoutRebuilder.ForceRebuildLayoutImmediate(plannedActionSprite.GetComponent<RectTransform>());
            _intentVisible = true;
            ApplyIntentHoverState(IsMouseHoveringThisEntity(), force: true);
        }

        private void HideIntentIndicator()
        {
            if (plannedActionSprite == null)
                return;

            EaseScale easeScale = plannedActionSprite.GetComponent<EaseScale>();
            if (easeScale != null)
            {
                easeScale.SetScale(Vector3.zero);
            }
            else
            {
                plannedActionSprite.transform.localScale = Vector3.zero;
            }

            _intentVisible = false;
            _intentHovered = null;
        }

        protected override void UpdateEntityHoverState()
        {
            if (!_intentVisible || plannedActionSprite == null)
                return;

            ApplyIntentHoverState(IsMouseHoveringThisEntity());
        }

        private void ApplyIntentHoverState(bool isHovered, bool force = false)
        {
            if (!force && _intentHovered.HasValue && _intentHovered.Value == isHovered)
                return;

            _intentHovered = isHovered;

            Vector3 targetPosition = isHovered
                ? IntentHoveredLocalPosition
                : IntentDefaultLocalPosition;
            SendIntentToPosition(targetPosition);

            Vector3 targetScale = isHovered
                ? IntentHoveredLocalScale
                : IntentDefaultLocalScale;

            EaseScale easeScale = plannedActionSprite.GetComponent<EaseScale>();
            if (easeScale != null)
            {
                easeScale.SetScale(targetScale);
            }
            else
            {
                plannedActionSprite.transform.localScale = targetScale;
            }
        }

        private void SendIntentToPosition(Vector3 targetPosition)
        {
            EasePosition easePosition = plannedActionSprite.GetComponent<EasePosition>();
            if (easePosition == null)
            {
                easePosition = plannedActionSprite.gameObject.AddComponent<EasePosition>();
                easePosition.isLocal = true;
            }

            easePosition.SendToLocation(targetPosition);
        }

        public void ClearIntentVisuals()
        {
            ClearNextTurnActionPreviews();
            HideIntentIndicator();
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
