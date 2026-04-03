using System;
using System.Collections.Generic;
using System.Linq;
using Cards;
using Entities;
using Grid;
using Types.Tiles;
using UnityEngine;
using Util;

namespace StateManager
{
    public class CampfireState : GameState
    {
        public GameObject window;

        public RandomState campfireRandom = RunInfo.NewRandom("campfire".GetHashCode());
        
        public override void Enter()
        {
            window.GetComponent<LerpPosition>().targetLocation = new Vector2(0, 0);
        }

        public override void Exit()
        {
            window.GetComponent<LerpPosition>().targetLocation = new Vector2(0, 730);
        }

        public void Heal(int amount)
        {
            AreYouSure.Instance.AskConfirm((confirmed) =>
            {
                if (!confirmed)
                    return;
                Player.Instance.Health += amount;
                GameStateManager.Instance.Change<MapState>();
            });
        }
        
        public void MaxHealth(int amount)
        {
            AreYouSure.Instance.AskConfirm((confirmed) =>
            {
                if (!confirmed)
                    return;
                Player.Instance.initialHealth += amount;
                GameStateManager.Instance.Change<MapState>();
            });
        }

        public void RemoveCard()
        {
            DeckView.Instance.GetCard((Card card) =>
            {
                
                AreYouSure.Instance.AskConfirm((confirmed) =>
                {
                    if (!confirmed)
                        return;
                    Deck.Instance.DestroyCard(card.UniqueId);
                    GameStateManager.Instance.Change<MapState>();
                });
            });
        }
    }
}