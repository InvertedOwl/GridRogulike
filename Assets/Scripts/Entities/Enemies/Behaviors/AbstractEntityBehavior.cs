using System.Collections;
using System.Collections.Generic;
using Cards.Actions;
using Cards.CardEvents;
using Grid;
using StateManager;
using UnityEngine;

namespace Entities.Enemies
{
    public abstract class AbstractEntityBehavior : MonoBehaviour
    {
        public NonPlayerEntity self;
        protected static readonly string[] HexDirections = HexGridManager.HexDirections;

        public virtual IEnumerator MakeTurn()
        {
            if (self.plannedAction == null || self.plannedAction.Count == 0)
            {
                yield break;
            }

            foreach (AbstractAction action in self.plannedAction)
            {
                OnBeforeAction(action);

                CardEventContext context = new CardEventContext();
                foreach (AbstractCardEvent modifiedEvent in self.ModifyEvents(action.Activate(null)))
                {
                    OnBeforeEvent(modifiedEvent);
                    CardEventResult result = modifiedEvent.ActivateWithResult(self, context);
                    context.Record(result);

                    if (modifiedEvent is AttackCardEvent)
                        self.ClearNextTurnActionPreviewForAction(action);

                    yield return new WaitForSeconds(GetActionDelay(action));
                }

                OnAfterAction(action);
            }

            self.plannedAction.Clear();
        }

        protected virtual float GetActionDelay(AbstractAction action)
        {
            return 0.5f * (1 / GameplayNavSettings.speed);
        }

        protected virtual void OnBeforeAction(AbstractAction action) { }

        protected virtual void OnBeforeEvent(AbstractCardEvent cardEvent)
        {
            if (cardEvent is not AttackCardEvent attackCardEvent)
            {
                return;
            }

            Vector2Int targetPos = attackCardEvent.usePosition
                ? attackCardEvent.position
                : HexGridManager.MoveHex(
                    self.positionRowCol,
                    attackCardEvent.direction,
                    attackCardEvent.distance
                );

            if (!HexGridManager.Instance.BoardDictionary.ContainsKey(targetPos))
                return;

            Vector3 targetWorldPosition = HexGridManager.Instance.GetWorldHexObject(targetPos).transform.position;
            Vector3 worldOffset = (targetWorldPosition - transform.position).normalized * 0.5f;
            transform.localPosition += transform.parent != null
                ? transform.parent.InverseTransformVector(worldOffset)
                : worldOffset;
        }

        protected virtual void OnAfterAction(AbstractAction action) { }

        protected int CountLivingEnemies()
        {
            PlayingState state = GameStateManager.Instance.GetCurrent<PlayingState>();
            int count = 0;

            foreach (AbstractEntity entity in state.GetEntities())
            {
                if (entity != null && entity.entityType == EntityType.Enemy && entity.Health > 0)
                {
                    count++;
                }
            }

            return count;
        }

        protected bool IsOnlyLivingEnemy()
        {
            return CountLivingEnemies() <= 1;
        }

        public abstract List<AbstractAction> NextTurn();

        public virtual List<AbstractAction> NextTurn(
            IReadOnlyDictionary<AbstractEntity, Vector2Int> plannedEntityPositions)
        {
            return NextTurn();
        }
    }
}
