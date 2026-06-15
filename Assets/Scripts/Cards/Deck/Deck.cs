using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cards;
using Cards.CardList;
using Entities;
using Grid;
using StateManager;
using Types.Statuses;
using UnityEngine;
using Util;

public class Deck : MonoBehaviour
{
    public List<Card> Cards = new List<Card>();

    private List<CardMonobehaviour> _draw = new List<CardMonobehaviour>();
    private List<CardMonobehaviour> _discard = new List<CardMonobehaviour>();
    private List<CardMonobehaviour> _hand = new List<CardMonobehaviour>();
    private List<CardMonobehaviour> _scrap = new List<CardMonobehaviour>();

    public RandomState _randomDeck;
    public static Deck Instance;

    public GameObject actionPrefab;
    public Transform drawTransform;
    public Transform discardTransform;
    public Transform playTransform;

    public Transform scrapTransform;

    public List<CardMonobehaviour> Draw { get { return _draw; } }
    public List<CardMonobehaviour> Discard { get { return _discard; } }
    public List<CardMonobehaviour> Hand { get { return _hand; } }
    public List<CardMonobehaviour> Scrap { get { return _scrap; } }

    private RectTransform _handRect;
    private bool _layoutDirty = true;
    private bool _playabilityDirty = true;
    private bool _hasLayoutSignature;
    private bool _hasPlayabilitySignature;
    private int _lastLayoutSignature;
    private int _lastPlayabilitySignature;

    public static void ResetStatics()
    {
        Instance = null;
    }

    public void Awake ()
    {
        Instance = this;
    }

    public void Start()
    {
        _randomDeck = RunInfo.NewRandom("deck");
        
        
        Debug.Log("SEED: " + RunInfo.seed);
    }

    public void SetInactive(bool inactive)
    {
        SetInactive(inactive, true);
    }

    public void SetInactive(bool inactive, bool darken)
    {
        foreach (var card in _draw) card.SetInactive(inactive, darken);
        foreach (var card in _discard) card.SetInactive(inactive, darken);
        foreach (var card in _hand) card.SetInactive(inactive, darken);
        foreach (var card in _scrap) card.SetInactive(inactive, darken);
        MarkPlayabilityDirty();
    }

    // TODO: Parameterize this
    public void StartGame()
    {
        
        // Dont duplicate cards when loading saved
        if (Cards.Count != 0)
        {
            return;
        }
        
        StartingDecks startingDeck = DebugStats.Enabled ? StartingDecks.developer : StartingDecks.basic;
        foreach (Card startingCard in CardData.GetStarter(startingDeck))
        {
            _draw.Add(CreateCard(startingCard));
        }

        MarkHandLayoutDirty();
        MarkPlayabilityDirty();
    }

    public void UpdatePlayability()
    {
        if (GameStateManager.Instance == null)
        {
            RememberPlayabilitySignature();
            return;
        }
        if (!GameStateManager.Instance.IsCurrent<PlayingState>())
        {
            RememberPlayabilitySignature();
            return;
        }
        if (RunInfo.Instance == null)
        {
            RememberPlayabilitySignature();
            return;
        }
        if (!ShouldRefreshPlayability())
            return;

        var playingState = GameStateManager.Instance.GetCurrent<PlayingState>();
        foreach (CardMonobehaviour card in Hand)
        {
            bool canPlay = CanPlayCardNow(card, playingState);
            bool isResolvingManualAttack = IsCardResolvingManualAttack(card, playingState);
            card.SetInactive(!canPlay && !isResolvingManualAttack);
            SetGlowActive(card, canPlay || isResolvingManualAttack);
        }

        RememberPlayabilitySignature();
    }

    private bool CanPlayCardNow(CardMonobehaviour card, PlayingState playingState)
    {
        return card != null &&
               playingState != null &&
               playingState.AllowUserInput &&
               !card.played &&
               !card.IsResolvingManualAttack &&
               !IsCardTooExpensive(card) &&
               !IsCardBlockedByRestriction(card);
    }

    private bool IsCardResolvingManualAttack(CardMonobehaviour card, PlayingState playingState)
    {
        return card != null &&
               playingState != null &&
               playingState.AllowUserInput &&
               !card.played &&
               card.IsResolvingManualAttack;
    }

    private void SetGlowActive(CardMonobehaviour card, bool active)
    {
        if (card == null)
            return;

        GOList goList = card.GetComponent<GOList>();
        if (goList == null || !goList.HasValue("Glow"))
            return;

        GameObject glow = goList.GetValue("Glow");
        if (glow != null)
            glow.SetActive(active);
    }

    private bool IsCardTooExpensive(CardMonobehaviour card)
    {
        return RunInfo.Instance != null &&
               (int)(card.CostOverride > -1 ? card.CostOverride : card.Card.Cost) > RunInfo.Instance.CurrentEnergy;
    }

    private bool IsCardBlockedByRestriction(CardMonobehaviour card)
    {
        return card != null && !card.CanPlayByRestrictions(out _);
    }

    public void SetHandToUnused()
    {
        if (HexClickPlayerController.instance != null)
            HexClickPlayerController.instance.ClearPendingAttacks();

        foreach (CardMonobehaviour card in Hand)
        {
            card.used = false;
        }

        MarkHandLayoutDirty();
        MarkPlayabilityDirty();
    }

    public void MarkHandLayoutDirty()
    {
        _layoutDirty = true;
    }

    public void MarkPlayabilityDirty()
    {
        _playabilityDirty = true;
    }

    private bool ShouldRefreshHandLayout()
    {
        int signature = GetLayoutSignature();
        if (!_layoutDirty && _hasLayoutSignature && signature == _lastLayoutSignature)
            return false;

        _lastLayoutSignature = signature;
        _hasLayoutSignature = true;
        _layoutDirty = false;
        return true;
    }

    private bool ShouldRefreshPlayability()
    {
        int signature = GetPlayabilitySignature();
        if (!_playabilityDirty && _hasPlayabilitySignature && signature == _lastPlayabilitySignature)
            return false;

        _lastPlayabilitySignature = signature;
        _hasPlayabilitySignature = true;
        _playabilityDirty = false;
        return true;
    }

    private void RememberLayoutSignature()
    {
        _lastLayoutSignature = GetLayoutSignature();
        _hasLayoutSignature = true;
        _layoutDirty = false;
    }

    private void RememberPlayabilitySignature()
    {
        _lastPlayabilitySignature = GetPlayabilitySignature();
        _hasPlayabilitySignature = true;
        _playabilityDirty = false;
    }

    private RectTransform GetHandRect()
    {
        if (_handRect == null)
            _handRect = GetComponent<RectTransform>();

        return _handRect;
    }

    private int GetLayoutSignature()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + GetGameStateSignature();
            hash = hash * 31 + GetRoundedVectorHash(drawTransform != null ? drawTransform.localPosition : Vector3.zero);
            hash = hash * 31 + GetRoundedVectorHash(discardTransform != null ? discardTransform.localPosition : Vector3.zero);
            hash = hash * 31 + GetRoundedVectorHash(scrapTransform != null ? scrapTransform.localPosition : Vector3.zero);

            RectTransform handRect = GetHandRect();
            hash = hash * 31 + (handRect != null ? Mathf.RoundToInt(handRect.rect.width * 100f) : 0);
            hash = AppendPileSignature(hash, _draw, false);
            hash = AppendPileSignature(hash, _discard, true);
            hash = AppendPileSignature(hash, _hand, true);
            hash = AppendPileSignature(hash, _scrap, false);
            return hash;
        }
    }

    private int GetPlayabilitySignature()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + GetGameStateSignature();

            if (RunInfo.Instance != null)
                hash = hash * 31 + RunInfo.Instance.CurrentEnergy;

            if (GameStateManager.Instance != null && GameStateManager.Instance.IsCurrent<PlayingState>())
            {
                PlayingState playingState = GameStateManager.Instance.GetCurrent<PlayingState>();
                hash = hash * 31 + (playingState != null && playingState.AllowUserInput ? 1 : 0);
                hash = hash * 31 + (playingState != null ? playingState.GetCardPlayRestrictionSignature() : 0);
            }

            hash = AppendPileSignature(hash, _hand, true);
            return hash;
        }
    }

    private int GetGameStateSignature()
    {
        if (GameStateManager.Instance == null)
            return 0;

        return GameStateManager.Instance.IsCurrent<PlayingState>() ? 1 : 2;
    }

    private int AppendPileSignature(int hash, List<CardMonobehaviour> pile, bool includePlayState)
    {
        unchecked
        {
            hash = hash * 31 + pile.Count;
            foreach (CardMonobehaviour card in pile)
            {
                hash = hash * 31 + (card != null ? card.GetInstanceID() : 0);

                if (card == null || !includePlayState)
                    continue;

                hash = hash * 31 + (card.used ? 1 : 0);
                hash = hash * 31 + (card.played ? 1 : 0);
                hash = hash * 31 + (card.inactive ? 1 : 0);
                hash = hash * 31 + (card.IsResolvingManualAttack ? 1 : 0);
                hash = hash * 31 + (card.onlyDisplay ? 1 : 0);
                hash = hash * 31 + Mathf.RoundToInt(card.CostOverride * 100f);
                hash = hash * 31 + Mathf.RoundToInt(card.Card.Cost * 100f);
            }

            return hash;
        }
    }

    private int GetRoundedVectorHash(Vector3 vector)
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + Mathf.RoundToInt(vector.x * 100f);
            hash = hash * 31 + Mathf.RoundToInt(vector.y * 100f);
            hash = hash * 31 + Mathf.RoundToInt(vector.z * 100f);
            return hash;
        }
    }

    private HashSet<CardMonobehaviour> _removingPlayed = new HashSet<CardMonobehaviour>();

    public void Update()
    {
        for (int i = _hand.Count - 1; i >= 0; i--)
        {
            CardMonobehaviour card = _hand[i];
            if (card.played && !_removingPlayed.Contains(card))
            {
                _removingPlayed.Add(card);
                RemovePlayed(card);
                MarkHandLayoutDirty();
                MarkPlayabilityDirty();
            }
        }

        PositionHandCards(0);
        UpdatePlayability();
        
    }

    private void RemovePlayed(CardMonobehaviour cardMonobehaviour)
    {
        cardMonobehaviour.ResetPlayState();
        _discard.Add(cardMonobehaviour);
        _hand.Remove(cardMonobehaviour);
        _removingPlayed.Remove(cardMonobehaviour);
        MarkHandLayoutDirty();
        MarkPlayabilityDirty();
        PositionHandCards();
    }


    public void DiscardRandomFromHand()
    {
        if (Hand.Count == 0) return;

        CardMonobehaviour chosen = Hand[_randomDeck.Next(0, Hand.Count)];

        if (chosen.CardStatus?.OnDiscard != null && !chosen.CardStatus.OnDiscard(chosen.Card))
        {
            return;
        }

        DiscardCard(chosen.Card.UniqueId);
    }
    
    public void DiscardCard(String cardId)
    {
        CardMonobehaviour cardToDiscard = null;

        // Find the card in hand with matching ID
        foreach (CardMonobehaviour card in _hand)
        {
            if (card.Card.UniqueId == cardId)
            {
                cardToDiscard = card;
                break;
            }
        }

        if (cardToDiscard == null)
        {
            return;
        }
        
        if (cardToDiscard.CardStatus?.OnDiscard != null && !cardToDiscard.CardStatus.OnDiscard(cardToDiscard.Card))
        {
            return;
        }

        cardToDiscard.GetComponent<LerpPosition>().targetLocation = discardTransform.localPosition;

        _discard.Add(cardToDiscard);
        _hand.Remove(cardToDiscard);
        MarkHandLayoutDirty();
        MarkPlayabilityDirty();
        PositionHandCards(0);
    }

    public CardMonobehaviour CreateCard(Card card)
    {
        Cards.Add(card);
        MarkHandLayoutDirty();
        MarkPlayabilityDirty();
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
                    playingState.CaptureFinish();
                }
            }
        };
        MarkHandLayoutDirty();
        MarkPlayabilityDirty();
        return cardObject.GetComponent<CardMonobehaviour>();
    }

    // TODO: Add destroying animation
    public void DestroyCard(string cardId)
    {
        Card card = Deck.Instance.Cards.Find(c => c.UniqueId == cardId);
        Cards.RemoveAll(c => c.UniqueId == cardId);

        Purge(_hand, card);
        Purge(_draw, card);
        Purge(_discard, card);
        Purge(_scrap, card);

        PositionHandCards(0);
        MarkPlayabilityDirty();
    }

    public void Purge(List<CardMonobehaviour> pile, Card card)
    {
        bool removed = false;
        for (int i = pile.Count - 1; i >= 0; i--)
        {
            var cardMono = pile[i];
            var cardMonoCard = cardMono.Card;

            if (cardMonoCard.UniqueId == card.UniqueId)
            {
                pile.RemoveAt(i);
                Destroy(cardMono.gameObject);
                removed = true;
            }
        }

        if (removed)
        {
            MarkHandLayoutDirty();
            MarkPlayabilityDirty();
        }
    }

    public void UpdateDeck()
    {
        ResetDeck();
        _draw.ForEach(card => { Destroy(card.gameObject); });
        _draw.Clear();
        Cards.ForEach(card =>
        {
            _draw.Add(CreateCardMono(card));
        });
        MarkHandLayoutDirty();
        MarkPlayabilityDirty();
    }

    public void ResetDeck()
    {
        // IMPORTANT: Scrap does NOT get returned to draw.
        _draw.AddRange(_hand);
        _draw.AddRange(_discard);
        _hand.Clear();
        _discard.Clear();
        MarkHandLayoutDirty();
        MarkPlayabilityDirty();
    }

    public void DiscardButton()
    {
        StartCoroutine(DiscardButtonPatient());
    }

    private IEnumerator DiscardButtonPatient()
    {
        DiscardHand();
        yield return new WaitForSeconds(0.25f * (1/GameplayNavSettings.speed));
        DrawHand();
    }

    public void DiscardHand()
    {
        if (HexClickPlayerController.instance != null)
            HexClickPlayerController.instance.ClearPendingAttacks();

        List<CardMonobehaviour> toKeep = new List<CardMonobehaviour>();

        foreach (CardMonobehaviour card in _hand)
        {
            if (card.CardStatus?.OnDiscard != null && !card.CardStatus.OnDiscard(card.Card))
            {
                toKeep.Add(card);
                continue;
            }
            card.GetComponent<LerpPosition>().targetLocation = discardTransform.localPosition;
            _discard.Add(card);
        }

        _hand.Clear();
        _hand.AddRange(toKeep);
        PositionHandCards(0);
    }

    public void ScrapHand()
    {
        foreach (CardMonobehaviour card in _hand)
        {
            ScrapCard(card);
        }
        _hand.Clear();
        PositionHandCards(0);
    }

    public void DrawHand()
    {
        _removingPlayed.Clear();
        FullDrawHand(GetModifiedHandDrawCount(4));
        PositionHandCards(0);
        Debug.Log("Cards in hand: " + _hand.Count);
    }

    private int GetModifiedHandDrawCount(int baseDrawCount)
    {
        int drawCount = baseDrawCount;
        if (Player.Instance?.statusManager == null)
            return drawCount;

        foreach (AbstractStatus status in Player.Instance.statusManager.statusList.ToList())
        {
            drawCount = status.ModifyDrawCount(drawCount);
        }

        return Mathf.Max(0, drawCount);
    }

    public void DrawCard(Card card)
    {
        for (int i = 0; i < _draw.Count; i++)
        {
            CardMonobehaviour drawnCard = _draw[i];
            if (!drawnCard.Card.Equals(card))
            {
                continue;
            }
            Debug.Log("Drawing single card: " + card.CardName);
            _draw.RemoveAt(i);
            _hand.Add(drawnCard);

            LerpPosition drawnLerp = drawnCard.GetComponent<LerpPosition>();
            drawnCard.transform.position = drawTransform.position;
            drawnLerp.targetLocation = drawTransform.localPosition;
            drawnCard.ResetPlayState();
            drawnCard.transform.SetSiblingIndex(i);
            drawnCard.siblingIndex = i;
        }

        MarkHandLayoutDirty();
        MarkPlayabilityDirty();
        PositionHandCards(0);
    }
    
    public void FullDrawHand(int numToDraw)
    {
        // If amount to draw is more than is in the deck, just draw the amount of cards.
        if (Cards.Count < numToDraw)
        {
            FullDrawHand(Cards.Count);
            return;
        }

        // Recursive case (Have to deal the amount possible, shuffle, and then deal the rest)
        if (_draw.Count < numToDraw)
        {
            int partHand = _draw.Count;
            FullDrawHand(partHand);

            foreach (CardMonobehaviour card in _discard)
            {
                LerpPosition lerp = card.GetComponent<LerpPosition>();
                card.transform.localPosition = drawTransform.localPosition;
                lerp.targetLocation = drawTransform.localPosition;
            }

            _draw.AddRange(_discard);
            _discard.Clear();
            MarkHandLayoutDirty();
            MarkPlayabilityDirty();
            StartCoroutine(WaitToDrawHand(numToDraw - partHand));
            return;
        }

        // Base case (normal deck)
        for (int i = 0; i < numToDraw; i++)
        {
            int index = _randomDeck.Next(0, _draw.Count);
            CardMonobehaviour drawnCard = _draw[index];
            _draw.RemoveAt(index);
            _hand.Add(drawnCard);

            LerpPosition drawnLerp = drawnCard.GetComponent<LerpPosition>();
            drawnCard.transform.position = drawTransform.position;
            drawnLerp.targetLocation = drawTransform.localPosition;
            drawnCard.ResetPlayState();
            drawnCard.transform.SetSiblingIndex(i);
            drawnCard.siblingIndex = i;
        }

        MarkHandLayoutDirty();
        MarkPlayabilityDirty();
        PositionHandCards(0);
    }

    IEnumerator WaitToDrawHand(int numToDraw)
    {
        yield return new WaitForSeconds(0f * (1/GameplayNavSettings.speed));
        FullDrawHand(numToDraw);
        yield break;
    }

    public void ScrapCard(string cardId)
    {
        // Find it in any pile and move it to scrap
        CardMonobehaviour cardMono =
            _hand.Find(c => c.Card.UniqueId == cardId) ??
            _draw.Find(c => c.Card.UniqueId == cardId) ??
            _discard.Find(c => c.Card.UniqueId == cardId) ??
            _scrap.Find(c => c.Card.UniqueId == cardId);

        if (cardMono == null) return;

        ScrapCard(cardMono);
        PositionHandCards(0);
    }

    public void ScrapCard(CardMonobehaviour cardMono)
    {
        // Remove from any pile it might be in
        _hand.Remove(cardMono);
        _draw.Remove(cardMono);
        _discard.Remove(cardMono);

        // Don't double-add
        if (!_scrap.Contains(cardMono))
        {
            _scrap.Add(cardMono);
        }

        // Visually move it if we have a transform
        if (scrapTransform != null)
        {
            cardMono.GetComponent<LerpPosition>().targetLocation = scrapTransform.localPosition;
        }

        MarkHandLayoutDirty();
        MarkPlayabilityDirty();
    }

    public void PositionHandCards(float animationDelayFactor = 0.2f)
    {
        if (!GameStateManager.Instance.IsCurrent<PlayingState>())
        {
            foreach (CardMonobehaviour card in _draw)
                card.GetComponent<LerpPosition>().targetLocation = drawTransform.localPosition;

            foreach (CardMonobehaviour card in _discard)
                card.GetComponent<LerpPosition>().targetLocation = drawTransform.localPosition;

            foreach (CardMonobehaviour card in _hand)
                card.GetComponent<LerpPosition>().targetLocation = drawTransform.localPosition;

            if (scrapTransform != null)
            {
                foreach (CardMonobehaviour card in _scrap)
                    card.GetComponent<LerpPosition>().targetLocation = scrapTransform.localPosition;
            }

            MarkHandLayoutDirty();
            return;
        }
        
        RectTransform handRect = GetHandRect();
        float width = (_hand.Count <= 3) ? handRect.rect.width * 0.65f : handRect.rect.width;

        int cardCount = _hand.Count;
        if (cardCount == 0)
        {
            RememberLayoutSignature();
            return;
        }

        for (int i = 0; i < cardCount; i++)
        {
            CardMonobehaviour card = _hand[i];

            float xHandLocal;
            if (cardCount == 1)
            {
                xHandLocal = 0f;
            }
            else
            {
                float t = i / (float)(cardCount - 1);          // 0..1
                xHandLocal = Mathf.Lerp(-width / 2f, width / 2f, t); // edge to edge
            }

            // Convert hand-local -> world -> card parent local (prevents off-screen issues)
            Vector3 worldPoint = handRect.TransformPoint(new Vector3(xHandLocal, 0f, 0f));
            Vector3 parentLocal = card.transform.parent.InverseTransformPoint(worldPoint);

            Vector2 targetPos = new Vector2(parentLocal.x, parentLocal.y);

            if (card.used)
            {
                targetPos += new Vector2(0, 50);
            }

            float delay = i * animationDelayFactor;

            card.transform.SetAsLastSibling();

            if (delay > 0f)
                StartCoroutine(DelayedAnimation(delay, card, targetPos));
            else
                card.GetComponent<LerpPosition>().targetLocation = targetPos;
        }

        foreach (CardMonobehaviour card in _discard)
        {
            if (card.used)
                continue;
            card.GetComponent<LerpPosition>().targetLocation = discardTransform.localPosition;
        }

        foreach (CardMonobehaviour card in _draw)
            card.GetComponent<LerpPosition>().targetLocation = drawTransform.localPosition;

        if (scrapTransform != null)
            foreach (CardMonobehaviour card in _scrap)
                card.GetComponent<LerpPosition>().targetLocation = scrapTransform.localPosition;

        RememberLayoutSignature();
    }


    
    public string GetRandomHandCardId()
    {
        if (_hand.Count == 0)
            return null;

        int index = _randomDeck.Next(0, _hand.Count);
        return _hand[index].Card.UniqueId;
    }


    IEnumerator DelayedAnimation(float delaySecs, CardMonobehaviour card, Vector2 vector)
    {
        yield return new WaitForSeconds(delaySecs * (1/GameplayNavSettings.speed));
        card.GetComponent<LerpPosition>().targetLocation = vector;
    }
}
