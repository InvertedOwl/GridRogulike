using System;
using StateManager;
using TMPro;
using UnityEngine;

namespace Entities
{
    public class Player : AbstractEntity
    {

        public static Player Instance;

        public void Awake()
        {
            if (Instance != null)
            {
                throw new Exception("Duplicate player instance");
            }
            
            Instance = this;
        }

        public override void Die()
        {
            
        }

        public override void StartTurn()
        {
            Deck.Instance.DrawHand();
            RunInfo.Instance.CurrentEnergy = RunInfo.Instance.MaxEnergy;
            RunInfo.Instance.Redraws = RunInfo.Instance.maxRedraws;
            GameStateManager.Instance.GetCurrent<PlayingState>().AllowUserInput = true;
        }

        public override void EndTurn()
        {
            Deck.Instance.DiscardHand();
            GameStateManager.Instance.GetCurrent<PlayingState>().AllowUserInput = false;
            RunInfo.Instance.CurrentSteps = 0;
        }
    }
}