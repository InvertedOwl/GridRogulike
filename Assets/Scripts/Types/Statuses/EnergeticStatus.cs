using System.Collections.Generic;
using Cards.CardEvents;
using Entities;
using StateManager;
using UnityEngine;

namespace Types.Statuses
{
    public class EnergeticStatus : AbstractStatus
    {
        public EnergeticStatus(int amount)
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
                AddEnergy(amountAdded);
        }

        public override void OnTurnResourcesReady()
        {
            AddEnergy(Amount);
        }

        private void AddEnergy(int amount)
        {
            if (RunInfo.Instance == null || amount <= 0)
                return;

            RunInfo.Instance.CurrentEnergy += amount;
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
