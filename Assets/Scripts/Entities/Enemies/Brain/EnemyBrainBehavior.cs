using System.Collections;
using System.Collections.Generic;
using Cards.Actions;
using Cards.CardEvents;
using StateManager;
using UnityEngine;

namespace Entities.Enemies
{
    public class EnemyBrainBehavior : AbstractEntityBehavior
    {
        [SerializeField] private EnemyBrainData brainData;
        [SerializeField] private int moveBudget = 3;

        private readonly Dictionary<AbstractAction, Vector2Int> _plannedActionSources =
            new Dictionary<AbstractAction, Vector2Int>();

        private void Awake()
        {
            if (self == null)
                self = GetComponent<NonPlayerEntity>();
        }

        public override List<AbstractAction> NextTurn()
        {
            return PlanNextTurn(null);
        }

        public override List<AbstractAction> NextTurn(
            IReadOnlyDictionary<AbstractEntity, Vector2Int> plannedEntityPositions)
        {
            return PlanNextTurn(plannedEntityPositions);
        }

        private List<AbstractAction> PlanNextTurn(
            IReadOnlyDictionary<AbstractEntity, Vector2Int> plannedEntityPositions)
        {
            if (self.plannedAction == null)
                self.plannedAction = new List<AbstractAction>();

            self.plannedAction.Clear();
            _plannedActionSources.Clear();

            if (brainData == null || self == null || self.Health <= 0)
                return self.plannedAction;

            PlayingState state = GameStateManager.Instance.GetCurrent<PlayingState>();
            if (state == null)
                return self.plannedAction;

            EnemyTurnContext context = new EnemyTurnContext(
                self,
                state,
                moveBudget,
                plannedEntityPositions);

            if (!brainData.PlanAttack(context).StopBrain &&
                !brainData.PlanMove(context).StopBrain)
            {
                brainData.PlanUtility(context);
            }

            self.plannedAction.AddRange(context.PlannedActions);
            foreach (KeyValuePair<AbstractAction, Vector2Int> entry in context.ActionSourcePositions)
            {
                _plannedActionSources[entry.Key] = entry.Value;
            }

            return self.plannedAction;
        }

        public override IEnumerator MakeTurn()
        {
            if (self.plannedAction == null || self.plannedAction.Count == 0)
                yield break;

            PlayingState state = GameStateManager.Instance.GetCurrent<PlayingState>();
            List<AbstractAction> actions = new List<AbstractAction>(self.plannedAction);

            foreach (AbstractAction action in actions)
            {
                _plannedActionSources.TryGetValue(action, out Vector2Int plannedSource);
                Vector2Int? source = _plannedActionSources.ContainsKey(action) ? plannedSource : null;

                if (!EnemyActionValidator.CanExecuteAction(action, self, state, source))
                {
                    self.ClearNextTurnActionPreviewForAction(action);
                    continue;
                }

                OnBeforeAction(action);

                CardEventContext context = new CardEventContext();
                foreach (AbstractCardEvent modifiedEvent in self.ModifyEvents(action.Activate((global::CardMonobehaviour)null)))
                {
                    if (!EnemyActionValidator.CanExecuteEvent(modifiedEvent, self, state))
                    {
                        if (modifiedEvent is AttackCardEvent)
                            self.ClearNextTurnActionPreviewForAction(action);

                        continue;
                    }

                    OnBeforeEvent(modifiedEvent);
                    CardEventResult result = modifiedEvent.ActivateWithResult(self, context);
                    context.Record(result);

                    if (modifiedEvent is AttackCardEvent)
                        self.ClearNextTurnActionPreviewForAction(action);

                    yield return new WaitForSeconds(GetActionDelay(action));
                }

                OnAfterAction(action);
            }

            self.EntityRandom.Next();
            self.plannedAction.Clear();
            _plannedActionSources.Clear();
        }
    }
}
