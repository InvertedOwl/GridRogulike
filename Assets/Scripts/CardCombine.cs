using System;
using System.Collections.Generic;
using System.Linq;
using Cards;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CardCombine : MonoBehaviour
{

    private Card[] toCombine = new Card[2];
    
    public DeckView deckView;

    public Button confirmButton;
    public CardMonobehaviour card1;
    public CardMonobehaviour card2;
    public CardMonobehaviour resultCard;
    
    void Start()
    {
        toCombine[0] = new Card(isReal:false);
        toCombine[1] = new Card(isReal:false);
    }

    void Update()
    {

    }

    public void Choose1()
    {
        List<Card> cardToRemove = new List<Card>();
        cardToRemove.AddRange(toCombine);
        foreach (Card card in Deck.Instance.Cards)
        {
            if (card.Actions.Count + toCombine[1].Actions.Count > 3)
            {
                cardToRemove.Add(card);
            }
        }
        
        deckView.GetCard(card =>
        {
            if (card.isReal)
            {

                toCombine[0] = card;
                card2.SetCard(card);
                UpdateResult();
            }
        }, cardToRemove.ToArray());
        
    }
    public void Choose2()
    {
        List<Card> cardToRemove = new List<Card>();
        cardToRemove.AddRange(toCombine);
        foreach (Card card in Deck.Instance.Cards)
        {
            if (card.Actions.Count + toCombine[0].Actions.Count > 3)
            {
                cardToRemove.Add(card);
            }
        }
        
        deckView.GetCard((card) =>
        {
            if (card.isReal)
            {
                toCombine[1] = card;
                card1.SetCard(card);
                UpdateResult();
            }
        }, cardToRemove.ToArray());
    }

    private void UpdateResult()
    {
        
        if (!toCombine[0].isReal || !toCombine[1].isReal)
        {
            confirmButton.interactable = false;
            resultCard.gameObject.SetActive(false);
        }
        else
        {
            confirmButton.interactable = true;
            resultCard.gameObject.SetActive(true);
            
            Card newCard = new Card(toCombine[0]);
            newCard.Actions.AddRange(toCombine[1].Actions);
            
            resultCard.SetCard(newCard);
        }
        
        card2.gameObject.SetActive(toCombine[0].isReal);
        card1.gameObject.SetActive(toCombine[1].isReal);
        card1.CardClickedCallback = () => { Choose2(); };
        card2.CardClickedCallback = () => { Choose1(); };
    }
    
}
