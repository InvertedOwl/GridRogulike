using System;
using Serializer;
using StateManager;
using UnityEngine;

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
        }
        

        public override void StartTurn()
        {
            Shield = 0;
            Deck.Instance.DrawHand();
            RunInfo.Instance.CurrentEnergy = RunInfo.Instance.MaxEnergy;
            PlayingState playingState = GameStateManager.Instance.GetCurrent<PlayingState>();
            playingState.AllowUserInput = true;
            playingState.ResetTurnTileTriggers();
            playingState.TriggerPlayerTile(positionRowCol, this);
            base.StartTurn();
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
