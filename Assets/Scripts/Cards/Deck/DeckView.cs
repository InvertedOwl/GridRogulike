using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cards;
using Entities;
using StateManager;
using UnityEngine;
using UnityEngine.UI;
using Util;

public class DeckView : MonoBehaviour
{
    public static DeckView Instance;
    public GameObject cardPrefab;
    public Transform cards;
    public Action<Card> Callback;
    public void GetCard(Action<Card> callback, Card[] cardBlacklist)
    {
        Callback = callback;
        ViewDeck(cardBlacklist);
    }
    
    
    public void Start()
    {
        Instance = this;
    }

    public void ViewDeckButton()
    {
        Enter();
        SpawnCards();
    }

    public void ViewDeck(Card[] cardBlacklist = null)
    {
        Enter();
        SpawnCards(cardBlacklist);
    }

    private void SpawnCards(Card[] cardBlacklist = null)
    {

        foreach (Card card in Deck.Instance.Cards)
        {
  
            
            GameObject cardObject = Instantiate(cardPrefab, cards);
            var cardMono = cardObject.GetComponent<CardMonobehaviour>();
            bool isInactive = cardBlacklist != null && cardBlacklist.Contains(card);
            cardMono.SetCard(card, active:!isInactive);
            cardMono.hoverScale = 1.4f;
            cardMono.used = true;
            cardMono.GetComponent<Canvas>().overrideSorting = true;
            cardMono.GetComponent<Canvas>().sortingOrder = 202;
            cardMono.sortingLayer = 202;
            cardMono.CardClickedCallback = () =>
            {
                if (Callback == null) return;
                Callback.Invoke(card);
                Callback = null;
                Exit();
            };
            cardObject.transform.localScale = new Vector3(0.1239199f, 0.1239199f, 0.1239199f);
            Destroy(cardObject.GetComponent<LerpPosition>());
            LayoutRebuilder.ForceRebuildLayoutImmediate(cards.GetComponent<RectTransform>());

        }
    }


    public void Done()
    {
        Exit();
    }
    
    private void Enter()
    {
        if (GameStateManager.Instance.GetCurrent<PlayingState>() is { } playing)
            GameStateManager.Instance.GetCurrent<PlayingState>().MoveEntitiesOut();
        GetComponent<EasePosition>().targetLocation = new Vector3(0, 0, 0);
    }

    private void Exit()
    {
        if (Callback != null)
        {
            Callback.Invoke(new Card(false));
        }
        
        if (GameStateManager.Instance.GetCurrent<PlayingState>() is { } playing)
            GameStateManager.Instance.GetCurrent<PlayingState>().MoveEntitiesIn();
        GetComponent<EasePosition>().targetLocation = new Vector3(0, 750, 0);

        foreach (Transform card in cards)
        {
            Destroy(card.gameObject);
        }
    }
}
