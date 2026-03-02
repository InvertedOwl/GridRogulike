using System;
using Cards.CardEvents;
using Grid;
using StateManager;
using TMPro;
using Types.Tiles;
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
        

        public override void StartTurn()
        {
            Shield = 0;
            Deck.Instance.DrawHand();
            RunInfo.Instance.CurrentEnergy = RunInfo.Instance.MaxEnergy;
            RunInfo.Instance.Redraws = RunInfo.Instance.maxRedraws;
            GameStateManager.Instance.GetCurrent<PlayingState>().AllowUserInput = true;
            
            TileEntry tile = TileData.tiles[HexGridManager.Instance.HexType(positionRowCol)];
            foreach (AbstractCardEvent cardEvent in tile.landEvent.Invoke())
            {
                cardEvent.Activate(this);
            }
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
    }
}