using System;
using System.Collections;
using System.Collections.Generic;
using Cards;
using Cards.CardList;
using Entities;
using NUnit.Framework;
using StateManager;
using Unity.VisualScripting;
using UnityEngine;
using Util;
using Random = System.Random;

public class Deck : MonoBehaviour
{
    public List<Card> Cards = new List<Card>();
    
    
    private List<CardMonobehaviour> _draw = new List<CardMonobehaviour>();
    private List<CardMonobehaviour> _discard = new List<CardMonobehaviour>();
    private List<CardMonobehaviour> _hand = new List<CardMonobehaviour>();
    private Random _random = new Random();
    public static Deck Instance;

    public GameObject actionPrefab;
    public Transform drawTransform;
    public Transform discardTransform;
    public List<CardMonobehaviour> Draw { get { return _draw; } }
    public List<CardMonobehaviour> Discard { get { return _discard; } }
    public List<CardMonobehaviour> Hand { get { return _hand; } }



    public void Awake()
    {
        Instance = this;
    }

    public void Start()
    {
        // DEBUG PICK NORMAL DECK
    }



    public void StartGame()
    {
        foreach (Card startingCard in CardData.GetStarter(StartingDecks.basic))
        {
            _draw.Add(CreateCard(startingCard));
        }
    }

    public void Update()
    {
        List<CardMonobehaviour> toRemove = new List<CardMonobehaviour>();

        foreach (CardMonobehaviour card in _hand)
        {
            if (card.used)
            {
                card.GetComponent<LerpPosition>().targetLocation = discardTransform.localPosition;
                _discard.Add(card);
                toRemove.Add(card);
            }
        }
        foreach (CardMonobehaviour card in toRemove)
        {
            _hand.Remove(card);
        }
        PositionHandCards(0);
    }

    public CardMonobehaviour CreateCard(Card card)
    {

        Cards.Add(card);
        return CreateCardMono(card);
    }

    public CardMonobehaviour CreateCardMono(Card card)
    {
        GameObject cardObject = Instantiate(actionPrefab, transform);
        cardObject.transform.position = drawTransform.position;
        cardObject.GetComponentInChildren<CardMonobehaviour>().SetCard(card);
        cardObject.GetComponentInChildren<CardMonobehaviour>().CardClickedCallback = () =>
        {
            if (GameStateManager.Instance.IsCurrent<PlayingState>())
            {
                PlayingState playingState = GameStateManager.Instance.GetCurrent<PlayingState>();
                if (playingState.CheckForFinish() == "player")
                {
                    playingState.PlayerWon();
                }
            }
        };
        return cardObject.GetComponent<CardMonobehaviour>();
    }

    public void UpdateDeck()
    {
        ResetDeck();
        _draw.ForEach(card => {Destroy(card.gameObject);});
        _draw.Clear();
        Cards.ForEach(card =>
        {
            Debug.Log(card + " being added");
             _draw.Add(CreateCardMono(card));
        });
    }

    public void ResetDeck()
    {
        _draw.AddRange(_hand);
        _draw.AddRange(_discard);
        _hand.Clear();
        _discard.Clear();
        RunInfo.Instance.Redraws = RunInfo.Instance.maxRedraws;
    }

    public void DiscardButton()
    {
        if (RunInfo.Instance.Redraws > 0)
        {
            DiscardHand();
            RunInfo.Instance.Redraws--;
            DrawHand();
        }
    }
    
    public void DiscardHand()
    {
        foreach (CardMonobehaviour card in _hand)
        {
            card.GetComponent<LerpPosition>().targetLocation = discardTransform.localPosition;
        }
        _discard.AddRange(_hand);
        _hand.Clear();
        PositionHandCards(0);
        
    }

    public void DrawHand()
    {
        FullDrawHand(5);
        PositionHandCards(0);
        Debug.Log("Cards in hand: " + _hand.Count);
    }
    
    public void FullDrawHand(int numToDraw)
    {
        if (_draw.Count < numToDraw)
        {
            foreach (CardMonobehaviour card in _discard)
            {
                LerpPosition lerp = card.GetComponent<LerpPosition>();
                card.transform.localPosition = drawTransform.localPosition;
                lerp.targetLocation = drawTransform.localPosition;
            }

            Debug.Log("Reshuffling!");
            _draw.AddRange(_discard);
            _discard.Clear();
        }
        
        for (int i = 0; i < numToDraw; i++)
        {
            int index = _random.Next(0, _draw.Count);
            CardMonobehaviour drawnCard = _draw[index];
            _draw.RemoveAt(index);
            _hand.Add(drawnCard);
            
            LerpPosition drawnLerp = drawnCard.GetComponent<LerpPosition>();
            drawnCard.transform.position = drawTransform.position;
            drawnLerp.targetLocation = drawTransform.localPosition;
            drawnCard.used = false;
            drawnCard.transform.SetSiblingIndex(i);
            drawnCard.siblingIndex = i;
        }

        PositionHandCards(0);
    }


    public void PositionHandCards(float animationDelayFactor = 0.2f)
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        float width = rectTransform.rect.width;

        int cardCount = _hand.Count;
        float spacing = width / (cardCount + 1);

        for (float i = cardCount - 1; i >= 0; i--)
        {
            CardMonobehaviour card = _hand[(int)i];
            float xPos = -width / 2 + spacing * (i + 1);
            Vector2 targetPos = new Vector2(xPos, 0);
            float delay = i * animationDelayFactor;
            if (delay > 0)
            {
                StartCoroutine(DelayedAnimation(delay, card, targetPos));
            }
            else
            {
                card.GetComponent<LerpPosition>().targetLocation = targetPos;
            }
        }

        foreach (CardMonobehaviour card in _discard)
        {
            card.GetComponent<LerpPosition>().targetLocation = discardTransform.localPosition;
        }
        foreach (CardMonobehaviour card in _draw)
        {
            card.GetComponent<LerpPosition>().targetLocation = drawTransform.localPosition;
        }
    }


    IEnumerator DelayedAnimation(float delaySecs, CardMonobehaviour card, Vector2 vector)
    {
        yield return new WaitForSeconds(delaySecs);
        card.GetComponent<LerpPosition>().targetLocation = vector;
    }
}
