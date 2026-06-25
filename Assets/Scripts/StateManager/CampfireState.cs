using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cards;
using Entities;
using Grid;
using Types.Tiles;
using UnityEngine;
using UnityEngine.UI;
using Util;

namespace StateManager
{
    public class CampfireState : GameState
    {
        public GameObject window;

        public RandomState campfireRandom = RunInfo.NewRandom("campfire");
        public Transform effectLocation;

        public Button Button1;
        public Button Button2;
        public Button Button3;
        
        public override void Enter()
        {
            Button1.enabled = true;
            Button2.enabled = true;
            Button3.enabled = true;
            
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
            Button1.enabled = false;
            Button2.enabled = false;
            Button3.enabled = false;
            
            yield return new WaitForSeconds(.6f);
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
