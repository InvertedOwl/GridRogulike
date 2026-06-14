using System.Collections.Generic;
using Cards.CardEvents;
using Entities;
using StateManager;
using UnityEngine;

namespace Types.Statuses
{
    public class HasteStatus : AbstractStatus
    {
        public HasteStatus(int amount)
        {
            Amount = amount;
        }

        public override List<AbstractCardEvent> Modify(List<AbstractCardEvent> cardEvent)
        {
            return cardEvent;
        }

        public override void OnApply(AbstractEntity entity, int amountAdded)
        {
            if (ShouldApplyImmediately(entity))
                AddSteps(amountAdded);
        }

        public override void OnTurnResourcesReady()
        {
            AddSteps(Amount);
        }

        private void AddSteps(int amount)
        {
            if (RunInfo.Instance == null || amount <= 0)
                return;

            RunInfo.Instance.CurrentSteps += amount;
            Amount -= amount;
        }

        private bool ShouldApplyImmediately(AbstractEntity entity)
        {
            if (entity == null || entity.entityType != EntityType.Player)
                return false;

            if (GameStateManager.Instance == null || !GameStateManager.Instance.IsCurrent<PlayingState>())
                return false;

            PlayingState playingState = GameStateManager.Instance.GetCurrent<PlayingState>();
            try
            {
                return playingState.AllowUserInput && playingState.CurrentTurn == entity;
            }
            catch
            {
                return false;
            }
        }

        public override void OnEndTurn()
        {
        }
    }
}
