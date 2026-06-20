using System;
using System.Collections;
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

        public RandomState campfireRandom = RunInfo.NewRandom("campfire");
        public Transform effectLocation;
        
        public override void Enter()
        {
            PlayWindowInSound();
            window.GetComponent<LerpPosition>().targetLocation = new Vector2(0, 0);
        }

        public override void Exit()
        {
            PlayWindowOutSound();
            window.GetComponent<LerpPosition>().targetLocation = new Vector2(0, 730);
        }

        public void Heal(int amount)
        {
            AreYouSure.Instance.AskConfirm((confirmed) =>
            {
                if (!confirmed)
                    return;
                Player.Instance.Health += amount;
                PlayHealthPickup();
                StartCoroutine(Leave());
            });
        }

        public IEnumerator Leave()
        {
            yield return new WaitForSeconds(1);
            GameStateManager.Instance.Change<MapState>();
        }
        
        public void MaxHealth(int amount)
        {
            AreYouSure.Instance.AskConfirm((confirmed) =>
            {
                if (!confirmed)
                    return;
                Player.Instance.initialHealth += amount;
                Player.Instance.Health += amount;
                PlayHealthPickup();
                StartCoroutine(Leave());
            });
        }

        public void PlayHealthPickup()
        {
            GameObject effect = FXManager.Instance.Play("PowerboxPickupHealth", effectLocation.position, effectLocation.rotation);
            effect.transform.localScale = new Vector3(20, 20, 20);
        }

        public void RemoveCard()
        {
            DeckView.Instance.GetCard((Card card) =>
            {
                if (!card.isReal)
                    return;
                
                AreYouSure.Instance.AskConfirm((confirmed) =>
                {
                    if (!confirmed)
                        return;

                    Deck.Instance.DestroyCard(card.UniqueId);
                    StartCoroutine(Leave());
                });
            });
        }
    }
}
