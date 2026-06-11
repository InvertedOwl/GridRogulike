using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cards;
using Entities;
using StateManager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Util;

public class DeckView : MonoBehaviour
{
    public static DeckView Instance;
    private static readonly Vector3 OpenLocation = new Vector3(0, 0, 0);
    private static readonly Vector3 ClosedLocation = new Vector3(0, 550, 0);

    public GameObject cardPrefab;
    public Transform cards;
    public Action<Card> Callback;
    private bool _isOpen;
    public TextMeshProUGUI Title;

    public void GetCard(Action<Card> callback, Card[] cardBlacklist)
    {
        Callback = callback;
        ViewDeck(cardBlacklist);
    }
    
    public void GetCard(Action<Card> callback)
    {
        Callback = callback;
        ViewDeck();
    }
    
    
    public void Start()
    {
        Instance = this;
    }

    public void ViewDeckButton()
    {
        if (_isOpen)
        {
            Exit();
            return;
        }

        ViewDeck();
    }

    public void ViewDrawPileButton()
    {
        List<Card> cardsLoad = new List<Card>();
        foreach (CardMonobehaviour card in Deck.Instance.Draw)
        {
            cardsLoad.Add(card.Card);
        }
        ViewDeck(cardsToLoad:cardsLoad, title:"Draw Pile");
    }
    public void ViewDiscardPileButton()
    {
        List<Card> cardsLoad = new List<Card>();
        foreach (CardMonobehaviour card in Deck.Instance.Discard)
        {
            cardsLoad.Add(card.Card);
        }
        ViewDeck(cardsToLoad:cardsLoad, title:"Discard Pile");
    }

    public void ViewDeck(Card[] cardBlacklist = null, List<Card> cardsToLoad = null, string title = "Deck")
    {
        Enter();
        ClearCards();

        if (cardsToLoad == null)
            SpawnCards(Deck.Instance.Cards, cardBlacklist);
        else
            SpawnCards(cardsToLoad, cardBlacklist);

        SetTitle(title);
    }

    private void SetTitle(string title)
    {
        Title.text = title;
    }

    private void SpawnCards(List<Card> cardsToLoad, Card[] cardBlacklist = null)
    {

        foreach (Card card in cardsToLoad)
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
            cardMono.onlyDisplay = true;
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
        _isOpen = true;
        GameStateManager.Instance?.PlayWindowInSound();
        GetComponent<EasePosition>().targetLocation = OpenLocation;
    }

    private void Exit()
    {
        if (Callback != null)
        {
            Callback.Invoke(new Card(false));
            Callback = null;
        }
        
        _isOpen = false;
        GameStateManager.Instance?.PlayWindowOutSound();
        GetComponent<EasePosition>().targetLocation = ClosedLocation;

        ClearCards();
    }

    private void ClearCards()
    {
        for (int i = cards.childCount - 1; i >= 0; i--)
        {
            Transform card = cards.GetChild(i);
            card.SetParent(null);
            Destroy(card.gameObject);
        }
    }
}
