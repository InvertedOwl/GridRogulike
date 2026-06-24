using System;
using System.Collections.Generic;
using Cards.CardEvents;
using Grid;
using Serializer;
using StateManager;
using UnityEngine;
using Util;

namespace Entities
{
    public class Player : AbstractEntity
    {

        public static Player Instance;

        public static void ResetStatics()
        {
            Instance = null;
        }

        public void Awake ()
        {
            Instance = this;
            if (DebugStats.Enabled)
            {
                initialHealth = DebugStats.StartingPlayerHealth;
                _health = DebugStats.StartingPlayerHealth;
            }
        }
        

        public override void StartTurn()
        {
            base.StartTurn();
            Deck.Instance.DrawHand();
            RunInfo.Instance.CurrentEnergy = RunInfo.Instance.MaxEnergy;
            TriggerTurnResourcesReadyStatuses();
            PlayingState playingState = GameStateManager.Instance.GetCurrent<PlayingState>();
            playingState.AllowUserInput = true;
            
            playingState.ResetTurnTileTriggers();
            playingState.TriggerPlayerTileStart(positionRowCol, this);

            // Gain movement at start of turn
            int stepsToGain = 1;
            if (DebugStats.Enabled)
            {
                stepsToGain = 1000;
            }
            foreach (AbstractCardEvent cardEvent in ModifyEvents(new List<AbstractCardEvent> { new GainStepsCardEvent(stepsToGain) }, false))
            {
                cardEvent.Activate(this);
            }

            HexClickPlayerController.instance?.UpdateMovableParticles(playingState);
        }

        public override void EndTurn()
        {
            foreach (CardMonobehaviour card in Deck.Instance.Hand)
            {
                if (card.CardStatus?.NotPlayed != null)
                    card.CardStatus?.NotPlayed(card.Card);
            }
            
            Deck.Instance.DiscardHand();
            if (GameStateManager.Instance.IsCurrent<PlayingState>())
                GameStateManager.Instance.GetCurrent<PlayingState>().AllowUserInput = false;
            RunInfo.Instance.CurrentSteps = 0;
            PlayingState playingState = GameStateManager.Instance.GetCurrent<PlayingState>();
            playingState.TriggerPlayerTileEnd(positionRowCol, this);

        }
        
        
        public PlayerSaveData CaptureSaveData()
        {
            return new PlayerSaveData()
            {
                maxHealth = initialHealth,
                health = _health
            };
        }
        
        public void RestoreFromSaveData(PlayerSaveData data)
        {
            if (data == null) return;
            initialHealth = data.maxHealth;
            _health = data.health;
        }
    }
}
