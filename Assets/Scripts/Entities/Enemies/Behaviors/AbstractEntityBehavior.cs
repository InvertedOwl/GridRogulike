using System.Collections;
using System.Collections.Generic;
using Cards.Actions;
using Cards.CardEvents;
using Grid;
using UnityEngine;

namespace Entities.Enemies
{
    public abstract class AbstractEntityBehavior : MonoBehaviour
    {
        public NonPlayerEntity self;
        protected static readonly string[] HexDirections = { "n", "ne", "nw", "s", "se", "sw" };

        public virtual IEnumerator MakeTurn()
        {
            if (self.plannedAction == null || self.plannedAction.Count == 0)
            {
                yield break;
            }

            foreach (AbstractAction action in self.plannedAction)
            {
                OnBeforeAction(action);

                foreach (AbstractCardEvent cardEvent in action.Activate(null))
                {
                    OnBeforeEvent(cardEvent);

                    foreach (AbstractCardEvent modifiedEvent in self.ModifyEvents(
                        new List<AbstractCardEvent> { cardEvent }))
                    {
                        modifiedEvent.Activate(self);
                    }

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

            Vector2Int targetPos = HexGridManager.MoveHex(
                self.positionRowCol,
                attackCardEvent.direction,
                attackCardEvent.distance
            );

            transform.localPosition +=
                ((Vector3)HexGridManager.GetHexCenter(targetPos.x, targetPos.y) - transform.position)
                .normalized * 0.5f;
        }

        protected virtual void OnAfterAction(AbstractAction action) { }

        public abstract List<AbstractAction> NextTurn();
    }
}