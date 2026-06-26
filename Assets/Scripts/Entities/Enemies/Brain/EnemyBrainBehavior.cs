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

            brainData.Plan(context);

            context.CommitPlanningRandom();

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

            for (int i = 0; i < actions.Count;)
            {
                AbstractAction action = actions[i];
                if (action is DirectionalAttackAction)
                {
                    int runStartIndex = i;
                    while (i < actions.Count && actions[i] is DirectionalAttackAction)
                        i++;

                    foreach (DirectionalAttackActionGroup group in BuildDirectionalAttackGroups(actions, runStartIndex, i))
                    {
                        bool activatedAny = false;
                        bool attackWindupUsed = false;
                        float delay = 0f;

                        foreach (AbstractAction groupedAction in group.Actions)
                        {
                            if (!TryExecuteActionWithoutDelay(
                                    groupedAction,
                                    state,
                                    suppressRepeatedAttackWindup: true,
                                    ref attackWindupUsed))
                            {
                                continue;
                            }

                            activatedAny = true;
                            delay = Mathf.Max(delay, GetActionDelay(groupedAction));
                        }

                        if (activatedAny)
                            yield return new WaitForSeconds(delay);
                    }

                    continue;
                }

                i++;
                yield return ExecuteActionWithDefaultDelay(action, state);
            }

            self.plannedAction.Clear();
            _plannedActionSources.Clear();
        }

        private IEnumerator ExecuteActionWithDefaultDelay(AbstractAction action, PlayingState state)
        {
            _plannedActionSources.TryGetValue(action, out Vector2Int plannedSource);
            Vector2Int? source = _plannedActionSources.ContainsKey(action) ? plannedSource : null;

            if (!EnemyActionValidator.CanExecuteAction(action, self, state, source))
            {
                self.ClearNextTurnActionPreviewForAction(action);
                yield break;
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

        private bool TryExecuteActionWithoutDelay(
            AbstractAction action,
            PlayingState state,
            bool suppressRepeatedAttackWindup,
            ref bool attackWindupUsed)
        {
            _plannedActionSources.TryGetValue(action, out Vector2Int plannedSource);
            Vector2Int? source = _plannedActionSources.ContainsKey(action) ? plannedSource : null;

            if (!EnemyActionValidator.CanExecuteAction(action, self, state, source))
            {
                self.ClearNextTurnActionPreviewForAction(action);
                return false;
            }

            OnBeforeAction(action);

            bool activatedAny = false;
            CardEventContext context = new CardEventContext();
            foreach (AbstractCardEvent modifiedEvent in self.ModifyEvents(action.Activate((global::CardMonobehaviour)null)))
            {
                if (!EnemyActionValidator.CanExecuteEvent(modifiedEvent, self, state))
                {
                    if (modifiedEvent is AttackCardEvent)
                        self.ClearNextTurnActionPreviewForAction(action);

                    continue;
                }

                bool isAttackEvent = modifiedEvent is AttackCardEvent;
                if (!isAttackEvent || !suppressRepeatedAttackWindup || !attackWindupUsed)
                {
                    OnBeforeEvent(modifiedEvent);
                    if (isAttackEvent)
                        attackWindupUsed = true;
                }

                CardEventResult result = modifiedEvent.ActivateWithResult(self, context);
                context.Record(result);
                activatedAny = true;

                if (isAttackEvent)
                    self.ClearNextTurnActionPreviewForAction(action);
            }

            OnAfterAction(action);
            return activatedAny;
        }

        private static List<DirectionalAttackActionGroup> BuildDirectionalAttackGroups(
            List<AbstractAction> actions,
            int startIndex,
            int endIndex)
        {
            List<DirectionalAttackActionGroup> groups = new List<DirectionalAttackActionGroup>();
            for (int i = startIndex; i < endIndex; i++)
            {
                if (actions[i] is not DirectionalAttackAction attackAction)
                    continue;

                string direction = attackAction.Direction ?? "";
                DirectionalAttackActionGroup group = groups.Find(entry => entry.Direction == direction);
                if (group == null)
                {
                    group = new DirectionalAttackActionGroup(direction);
                    groups.Add(group);
                }

                group.Actions.Add(actions[i]);
            }

            return groups;
        }

        private class DirectionalAttackActionGroup
        {
            public string Direction { get; }
            public List<AbstractAction> Actions { get; } = new List<AbstractAction>();

            public DirectionalAttackActionGroup(string direction)
            {
                Direction = direction;
            }
        }
    }
}
