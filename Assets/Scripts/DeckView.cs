using System;
using System.Collections;
using System.Collections.Generic;
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

    public void Start()
    {
        Instance = this;
    }

    public void ViewDeck()
    {
        Enter();
        SpawnCards();
    }

    private void SpawnCards()
    {

        foreach (Card card in Deck.Instance.Cards)
        {
            GameObject cardObject = Instantiate(cardPrefab, cards);
            var cardMono = cardObject.GetComponent<CardMonobehaviour>();
            cardMono.SetCard(card);
            cardMono.hoverScale = 1.3f;
            cardMono.used = true;
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
        GetComponent<LerpPosition>().targetLocation = new Vector3(0, 0, 0);
    }

    private void Exit()
    {
        if (GameStateManager.Instance.GetCurrent<PlayingState>() is { } playing)
            GameStateManager.Instance.GetCurrent<PlayingState>().MoveEntitiesIn();
        GetComponent<LerpPosition>().targetLocation = new Vector3(0, 750, 0);

        foreach (Transform card in cards)
        {
            Destroy(card.gameObject);
        }
    }
}
