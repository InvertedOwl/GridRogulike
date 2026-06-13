using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cards;
using Cards.Actions;
using Entities;
using StateManager;
using TMPro;
using Cards.CardEvents;
using Cards.CardStatuses;
using Grid;
using Passives;
using ScriptableObjects;
using Types.Passives;
using Types.CardRestrictions;
using Types.Statuses;
using Types.Tiles;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Util;

public class CardMonobehaviour : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private bool isPointerOver = false;

    public bool IsPointerOver => isPointerOver;

    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerOver = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOver = false;
    }

    public Dictionary<TextMeshProUGUI, AbstractAction> TypeTitles = new Dictionary<TextMeshProUGUI, AbstractAction>();

    public TextMeshProUGUI titleText;
    public TextMeshProUGUI costText;
    public TextMeshProUGUI actionText;
    public bool used = false;
    public bool played;
    private bool _waitingForManualAttackResolution;
    public CardStatusDatabase.CardStatus? CardStatus;
    private Card _card;
    public bool onlyDisplay = false;
    public RandomPitchSound sound;

    public Card Card => _card;
    public float CostOverride = -1f;

    private bool _cardSet;

    public GameObject MainPanel;

    public GameObject inactiveImage;
    public GOList GoList;

    public InfoPanelManager InfoPanel;

    public CardStatusDatabase cardStatusDatabase;
    public SpriteDatabase spriteDatabase;

    public float hoverScale = 1.05f;
    public float hoverOffset = 60;

    public List<GameObject> types = new List<GameObject>();

    public bool inactive;

    public Action CardClickedCallback;
    public int sortingLayer = 170;
    public int siblingIndex;

    private RandomState _cardRandom;

    public Image modifierBG;

    public void SetCard(Card card, Action callback = null, bool active = true, float costOverride = -1f)
    {
        InfoPanel.RemovePanels();

        this.CostOverride = costOverride;
        _card = card;
        _cardSet = true;

        titleText.text = _card.CardName;

        if (costOverride > -1f)
        {
            costText.color = Color.darkGreen;
        }
        else
        {
            costText.color = Color.black;
        }
        costText.text = (costOverride>-1)?costOverride.ToString():_card.Cost.ToString();


        UpdateTypes();
        UpdateDiagram();
        UpdateBuff();
        MainPanel.GetComponent<Image>().color = CardRarityColors.GetColor(card.Rarity);
        SetInactive(!active);

        this.CardClickedCallback = callback;
        _cardRandom = card.cardRandom;

        GoList.GetValue("rarityText").GetComponent<TextMeshProUGUI>().text = _card.Rarity.ToString();

        SetCardSetIcon();

        SetCardStatus(null);
    }

    public void SetCardSetIcon()
    {
        GoList.GetValue("CardSetImage").GetComponent<Image>().sprite =
            spriteDatabase.Get(_card.CardSet.ToString()).Value.sprite;
    }

    public void SetCardStatus(CardStatusDatabase.CardStatus cardStatusNullable)
    {
        StartCoroutine(WaitFrameToCardStatus(cardStatusNullable));
    }

    IEnumerator WaitFrameToCardStatus(CardStatusDatabase.CardStatus cardStatusNullable)
    {
        yield return new WaitForFixedUpdate();
        CardStatus = cardStatusNullable;
        if (cardStatusNullable == null)
        {
            GoList.GetValue("CardStatusBorder2").GetComponent<EaseColor>().targetColor = new Color32(29, 59, 94, 0);
            GoList.GetValue("CardStatusIconParent").GetComponent<EaseScale>().SetScale(Vector3.zero);
            GoList.GetValue("CardStatusBorder1").GetComponent<EaseColor>().targetColor = new Color32(29, 59, 94, 255);
            yield break;
        }

        CardStatusDatabase.CardStatus cardStatus = cardStatusNullable;
        InfoPanel.AddPanels(cardStatus.key);

        GoList.GetValue("CardStatusBorder1").GetComponent<EaseColor>().targetColor = cardStatus.color;
        GoList.GetValue("CardStatusBorder2").GetComponent<EaseColor>().targetColor = new Color(cardStatus.color.r, cardStatus.color.g, cardStatus.color.b, .4f);
        GoList.GetValue("CardStatusIconParent").GetComponent<EaseScale>().SetScale(Vector3.one);
        GoList.GetValue("BGColor").GetComponent<Image>().color = cardStatus.color;
        GoList.GetValue("CardStatusIcon").GetComponent<Image>().sprite = cardStatus.sprite;
    }

    public void SetInactive(bool setinactive)
    {
        SetInactive(setinactive, true);
    }

    public void SetInactive(bool setinactive, bool darken)
    {
        this.inactive = setinactive;
        float inactiveAlpha = (setinactive && darken) ? 0.7f : 0.0f;
        inactiveImage.GetComponent<EaseColor>().targetColor = new Color(inactiveImage.GetComponent<EaseColor>().targetColor.r,
            inactiveImage.GetComponent<EaseColor>().targetColor.g,
            inactiveImage.GetComponent<EaseColor>().targetColor.b, inactiveAlpha);
    }

    public void ResetPlayState()
    {
        used = false;
        played = false;
        _waitingForManualAttackResolution = false;
    }

    public void CancelManualAttackTargeting()
    {
        _waitingForManualAttackResolution = false;
        used = false;
    }

    public bool IsResolvingManualAttack => _waitingForManualAttackResolution;

    public string FormatTextForInfo(string info)
    {
        return InfoPanel != null ? InfoPanel.FormatTextForInfo(info) : info;
    }

    private void UpdateBuff()
    {
        // this.Modifier.GetComponent<TextMeshProUGUI>().text = FormatTextForInfo(_card.Modifier.ModifierText);
        // this.Condition.GetComponent<TextMeshProUGUI>().text = FormatTextForInfo(_card.Condition.ConditionText);

    }

    private void UpdateDiagram()
    {
        if (!GoList.HasValue("diagram") || GoList.GetValue("diagram") == null )
            return;

        foreach (Transform child in GoList.GetValue("diagram").transform)
        {
            Destroy(child.gameObject);
        }

        List<RectTransform> allElements = new List<RectTransform>();

        allElements.Add(Instantiate(GoList.GetValue("tilePrefab"), GoList.GetValue("diagram").transform).GetComponent<RectTransform>());

        foreach (AbstractAction action in _card.Actions)
        {
            allElements.AddRange(action.UpdateGraphic(GoList.GetValue("diagram"), GoList.GetValue("tilePrefab"), GoList.GetValue("arrowPrefab")));
        }

        Vector3 averagePos = GetUIAveragePosition(allElements);

        foreach (RectTransform rectTransform in allElements)
        {
            rectTransform.localPosition -= averagePos;
        }
    }

    public static Vector3 GetUIAveragePosition(List<RectTransform> uiElements)
    {
        if (uiElements == null || uiElements.Count == 0)
            return Vector3.zero;

        Vector3 total = Vector3.zero;
        int count = 0;

        foreach (RectTransform rt in uiElements)
        {
            if (rt == null) continue;
            total += rt.localPosition;
            count++;
        }

        return count > 0 ? total / count : Vector3.zero;
    }

    public static Vector2 GetUIBounds(List<RectTransform> uiElements)
    {
        if (uiElements == null || uiElements.Count == 0)
            return Vector2.zero;

        Vector3 min = new Vector3(float.MaxValue, float.MaxValue, 0);
        Vector3 max = new Vector3(float.MinValue, float.MinValue, 0);

        foreach (RectTransform rectTransform in uiElements)
        {
            if (rectTransform == null) continue;

            Vector3[] corners = new Vector3[4];
            rectTransform.GetLocalCorners(corners);

            foreach (Vector3 corner in corners)
            {
                min = Vector3.Min(min, corner);
                max = Vector3.Max(max, corner);
            }
        }

        float width = max.x - min.x;
        float height = max.y - min.y;

        return new Vector2(width, height);
    }


    private void UpdateTypes()
    {

        foreach (GameObject type in types)
        {
            Destroy(type);
        }

        TypeTitles.Clear();

        int posY = -175;
        foreach (AbstractAction action in _card.Actions)
        {
            if (!action.visible)
            {
                continue;
            }

            GameObject text = null;
            bool setText = true;
            bool isPassiveAction = false;

            switch (action)
            {
                case SpawnPassiveAction spawnPassiveAction:
                    PassiveEntry passiveEntry = PassiveData.GetPassiveEntry(spawnPassiveAction.GetPassive());
                    text = Instantiate(GoList.GetValue("environPrefab"), MainPanel.transform);
                    EnvironMonobehavior environ = text.GetComponent<EnvironMonobehavior>();
                    environ.SetEnviron(passiveEntry.Name, passiveEntry.Desc, passiveEntry.Color);
                    InfoPanel.AddTextSources(new[] { environ.title, environ.description });
                    text.transform.localScale = new Vector3(4.5f, 4.5f, 1);
                    posY -= 140;
                    setText = false;
                    isPassiveAction = true;
                    break;

                default:
                    text = Instantiate(GoList.GetValue("normalPrefab"), MainPanel.transform);
                    break;
            }

            if (setText)
            {
                TextMeshProUGUI actionText = text.transform.GetComponentInChildren<TextMeshProUGUI>();
                actionText.text = FormatTextForInfo(action.GetText());
                TypeTitles[actionText] = action;
                InfoPanel.AddTextSource(actionText);
            }
            types.Add(text);
            text.GetComponent<RectTransform>().localPosition = new Vector2(0, posY);
            posY -= 140;
            if (isPassiveAction)
                posY -= 140;
        }
    }

    public void UpdateTypesTitles()
    {
        Dictionary<AbstractAction, CardActionPreview> actionPreviews = BuildActionPreviews();

        foreach (TextMeshProUGUI text in TypeTitles.Keys)
        {
            AbstractAction action = TypeTitles[text];
            string actionText = actionPreviews.TryGetValue(action, out CardActionPreview preview)
                ? action.GetText(preview)
                : action.GetText();

            text.text = FormatTextForInfo(actionText);
        }
    }

    private Dictionary<AbstractAction, CardActionPreview> BuildActionPreviews()
    {
        if (!_cardSet ||
            _card.Actions == null ||
            GameStateManager.Instance == null ||
            !GameStateManager.Instance.IsCurrent<PlayingState>())
        {
            return new Dictionary<AbstractAction, CardActionPreview>();
        }

        Dictionary<AbstractAction, CardActionPreview> previews = BuildEmptyActionPreviews();
        int previousCardsPlayedThisBattle = BattleStats.CardsPlayedThisBattle;
        int previousCardsPlayedThisTurn = BattleStats.CardsPlayedThisTurn;

        try
        {
            BattleStats.CardsPlayedThisBattle += 1;
            BattleStats.CardsPlayedThisTurn += 1;

            List<CardEventPreviewSnapshot> baseEventSnapshots;
            List<AbstractCardEvent> modifiedEventQueue = BuildBaseEventQueue(out baseEventSnapshots, previewMode: true);
            modifiedEventQueue = ApplyCardModifiers(modifiedEventQueue, previewMode: true);

            for (int i = 0; i < _card.Actions.Count; i++)
            {
                previews[_card.Actions[i]] = new CardActionPreview(i, baseEventSnapshots, modifiedEventQueue);
            }
        }
        catch
        {
            return previews;
        }
        finally
        {
            BattleStats.CardsPlayedThisBattle = previousCardsPlayedThisBattle;
            BattleStats.CardsPlayedThisTurn = previousCardsPlayedThisTurn;
        }

        return previews;
    }

    private Dictionary<AbstractAction, CardActionPreview> BuildEmptyActionPreviews()
    {
        Dictionary<AbstractAction, CardActionPreview> previews = new Dictionary<AbstractAction, CardActionPreview>();

        for (int i = 0; i < _card.Actions.Count; i++)
        {
            previews[_card.Actions[i]] = new CardActionPreview(
                i,
                new List<CardEventPreviewSnapshot>(),
                new List<AbstractCardEvent>());
        }

        return previews;
    }

    public void RotateArrow(string direction, Transform arrow)
    {
        switch (direction)
        {
            case "e": arrow.eulerAngles = new Vector3(0, 0, -90); break;
            case "w": arrow.eulerAngles = new Vector3(0, 0, 90); break;
            case "ne": arrow.eulerAngles = new Vector3(0, 0, -30); break;
            case "nw": arrow.eulerAngles = new Vector3(0, 0, 30); break;
            case "se": arrow.eulerAngles = new Vector3(0, 0, -150); break;
            case "sw": arrow.eulerAngles = new Vector3(0, 0, 150); break;
        }
    }

    public void Update()
    {
        // Modifiers
        // UpdateCondition();

        // Mouse events
        if (IsPointerOverThisUIElement() && _cardSet)
        {
            HandleHoverEffects();
            HandleCardUsage();
        }
        else if (_cardSet)
        {
            ResetHoverEffects();
        }

        UpdateTypesTitles();
    }

    private bool washover;

    private void HandleHoverEffects()
    {
        InfoPanel.gameObject.SetActive(true);
        siblingIndex = transform.GetSiblingIndex();
        LerpPosition lerpPosition = transform.GetChild(0).GetChild(0).GetComponent<LerpPosition>();
        lerpPosition.targetLocation = new Vector2(0, hoverOffset);
        lerpPosition.targetScale = lerpPosition.startScale * hoverScale;
        GetComponent<Canvas>().overrideSorting = true;
        GetComponent<Canvas>().sortingOrder = sortingLayer;
        // transform.SetSiblingIndex(50);


        if (!_cardSet)
            return;
        if (_card.Actions.Count < 0)
            return;

        foreach (AbstractAction action in _card.Actions)
        {
            action.Hover();
        }

        if (!washover)
        {

        }

        washover = true;
    }

    private void ResetHoverEffects()
    {
        InfoPanel.gameObject.SetActive(false);
        LerpPosition lerpPosition = transform.GetChild(0).GetChild(0).GetComponent<LerpPosition>();
        lerpPosition.targetLocation = new Vector2(0, 0);
        lerpPosition.targetScale = lerpPosition.startScale * 1f;
        GetComponent<Canvas>().overrideSorting = false;
        foreach (AbstractAction action in _card.Actions)
        {
            action.NotHover();
        }

        washover = false;
    }

    private void HandleCardUsage()
    {

        if (!_cardSet || inactive)
            return;

        bool isLeftClick = Input.GetMouseButtonDown(0);
        bool wasUsed = used;

        bool hasEnoughEnergy = RunInfo.Instance.CurrentEnergy >= ((CostOverride>-1)?CostOverride:_card.Cost);
        bool isPlayerTurn = false;
        if (GameStateManager.Instance.GetCurrent<PlayingState>() is { } playing)
            isPlayerTurn = playing.CurrentTurn.entityType == EntityType.Player;
        bool canPlayByRestrictions = CanPlayByRestrictions(out _);

        if (isLeftClick)
        {
            Deck.Instance.SetHandToUnused();
            used = true;
            CardClickedCallback?.Invoke();

            sound.PlaySound("hover", 0.5f);

            if (!onlyDisplay && !played && hasEnoughEnergy && isPlayerTurn && canPlayByRestrictions && HasManualAttackAction())
            {
                BeginManualAttackTargeting();
                HexClickPlayerController.instance.UpdateMovableParticles(GameStateManager.Instance.GetCurrent<PlayingState>());
                return;
            }

            if (!wasUsed && !onlyDisplay && !played && hasEnoughEnergy && isPlayerTurn && canPlayByRestrictions)
            {
                PreviewNonManualAttacks();
            }
        }

        if (!wasUsed || onlyDisplay)
            return;

        if (isLeftClick && !played && hasEnoughEnergy && isPlayerTurn && canPlayByRestrictions)
        {
            if (HexClickPlayerController.instance != null)
                HexClickPlayerController.instance.ClearToAttackEmitters();

            PlayCard(out List<AttackCardEvent> attackCardEvents);

            if (attackCardEvents.Count > 0)
                HexClickPlayerController.instance.AddToAttack(attackCardEvents);

            played = true;
        }


        HexClickPlayerController.instance.UpdateMovableParticles(GameStateManager.Instance.GetCurrent<PlayingState>());
    }

    private bool HasManualAttackAction()
    {
        return _card.Actions.Any(action => action is AttackAction);
    }

    private void PreviewNonManualAttacks()
    {
        if (HexClickPlayerController.instance == null)
            return;

        List<AttackCardEvent> attackCardEvents = new List<AttackCardEvent>();

        foreach (AbstractAction action in _card.Actions)
        {
            foreach (AbstractCardEvent cardEvent in action.Preview(cardMono:this))
            {
                if (cardEvent is AttackCardEvent attackCardEvent && !attackCardEvent.manual)
                    attackCardEvents.Add(attackCardEvent);
            }
        }

        if (attackCardEvents.Count > 0)
        {
            HexClickPlayerController.instance.PreviewAttackEvents(attackCardEvents);
            HexClickPlayerController.instance.BeginNonManualAttackPreview(attackCardEvents, this);
        }
    }

    private void BeginManualAttackTargeting()
    {
        List<AttackCardEvent> attackCardEvents = new List<AttackCardEvent>();

        foreach (AbstractAction action in _card.Actions)
        {
            if (!(action is AttackAction))
                continue;

            foreach (AbstractCardEvent cardEvent in action.Activate(cardMono:this))
            {
                if (cardEvent is AttackCardEvent attackCardEvent && attackCardEvent.manual)
                    attackCardEvents.Add(attackCardEvent);
            }
        }

        if (attackCardEvents.Count == 0)
            return;

        _waitingForManualAttackResolution = true;
        HexClickPlayerController.instance.BeginCardAttack(attackCardEvents, this);
    }

    public bool TryStartManualAttackPlay(out List<AttackCardEvent> attackCardEvents)
    {
        if (!_waitingForManualAttackResolution)
        {
            attackCardEvents = new List<AttackCardEvent>();
            return false;
        }

        bool hasEnoughEnergy = RunInfo.Instance.CurrentEnergy >= ((CostOverride>-1)?CostOverride:_card.Cost);
        bool isPlayerTurn = false;
        if (GameStateManager.Instance.GetCurrent<PlayingState>() is { } playing)
            isPlayerTurn = playing.CurrentTurn.entityType == EntityType.Player;

        if (!hasEnoughEnergy || !isPlayerTurn || !CanPlayByRestrictions(out _))
        {
            attackCardEvents = new List<AttackCardEvent>();
            CancelManualAttackTargeting();
            return false;
        }

        PlayCard(out attackCardEvents);
        return true;
    }

    public bool TryPlayNonManualAttackPreview()
    {
        bool hasEnoughEnergy = RunInfo.Instance.CurrentEnergy >= ((CostOverride > -1) ? CostOverride : _card.Cost);
        bool isPlayerTurn = false;
        if (GameStateManager.Instance.GetCurrent<PlayingState>() is { } playing)
            isPlayerTurn = playing.CurrentTurn.entityType == EntityType.Player;

        if (!used || played || onlyDisplay || !hasEnoughEnergy || !isPlayerTurn || !CanPlayByRestrictions(out _))
            return false;

        PlayCard(out List<AttackCardEvent> attackCardEvents);

        if (attackCardEvents.Count > 0 && HexClickPlayerController.instance != null)
            HexClickPlayerController.instance.AddToAttack(attackCardEvents);

        played = true;
        return true;
    }

    private void PlayCard(out List<AttackCardEvent> attackCardEvents)
    {
        sound.PlaySound("play", 0.5f);
        int currentCost = (int)((CostOverride > -1) ? CostOverride : _card.Cost);
        Player player = GameStateManager.Instance.GetCurrent<PlayingState>().player;

        BattleStats.CardsPlayedThisBattle += 1;
        BattleStats.CardsPlayedThisTurn += 1;

        List<CardEventPreviewSnapshot> unusedBaseEventSnapshots;
        List<AbstractCardEvent> eventQueue = ApplyCardModifiers(
            BuildBaseEventQueue(out unusedBaseEventSnapshots, previewMode: false),
            previewMode: false);

        List<AttackCardEvent> manualAttackEvents = new List<AttackCardEvent>();

        // Extract attack actions for HexClick whatever its called
        foreach (AbstractCardEvent cardEvent in eventQueue)
        {
            if (cardEvent is AttackCardEvent attackCardEvent && attackCardEvent.manual)
            {
                manualAttackEvents.Add(attackCardEvent);
            }
        }

        // Remove attack actions
        HashSet<AbstractCardEvent> manualAttackEventSet = new HashSet<AbstractCardEvent>(manualAttackEvents);
        eventQueue.RemoveAll(item => manualAttackEventSet.Contains(item));
        attackCardEvents = manualAttackEvents;

        // Activate queue (excluding attacks)
        foreach (AbstractCardEvent cardEvent in eventQueue)
        {
            cardEvent.Activate(player);
        }

        RunInfo.Instance.CurrentEnergy -= currentCost;
    }

    public bool CanPlayByRestrictions(out string blockedReason)
    {
        return CardPlayRestrictionSystem.CanPlay(this, out blockedReason);
    }

    public List<AbstractCardEvent> BuildRestrictionPreviewEvents()
    {
        int previousCardsPlayedThisBattle = BattleStats.CardsPlayedThisBattle;
        int previousCardsPlayedThisTurn = BattleStats.CardsPlayedThisTurn;

        try
        {
            BattleStats.CardsPlayedThisBattle += 1;
            BattleStats.CardsPlayedThisTurn += 1;

            List<CardEventPreviewSnapshot> unusedBaseEventSnapshots;
            return ApplyCardModifiers(
                BuildBaseEventQueue(out unusedBaseEventSnapshots, previewMode: true),
                previewMode: true);
        }
        catch (Exception exception)
        {
            Debug.LogWarning("Could not build card restriction preview events for " + name + ": " + exception.Message);
            return new List<AbstractCardEvent>();
        }
        finally
        {
            BattleStats.CardsPlayedThisBattle = previousCardsPlayedThisBattle;
            BattleStats.CardsPlayedThisTurn = previousCardsPlayedThisTurn;
        }
    }

    private List<AbstractCardEvent> BuildBaseEventQueue(
        out List<CardEventPreviewSnapshot> baseEventSnapshots,
        bool previewMode)
    {
        baseEventSnapshots = new List<CardEventPreviewSnapshot>();
        List<AbstractCardEvent> eventQueue = new List<AbstractCardEvent>();

        for (int actionIndex = 0; actionIndex < _card.Actions.Count; actionIndex++)
        {
            List<AbstractCardEvent> cardEvents = _card.Actions[actionIndex].Activate(this, previewMode);

            foreach (AbstractCardEvent cardEvent in cardEvents)
            {
                cardEvent.PreviewSourceActionIndex = actionIndex;
                baseEventSnapshots.Add(new CardEventPreviewSnapshot(cardEvent));
                eventQueue.Add(cardEvent);
            }
        }

        return eventQueue;
    }

    private List<AbstractCardEvent> ApplyCardModifiers(List<AbstractCardEvent> eventQueue, bool previewMode)
    {
        PlayingState playingState = GameStateManager.Instance.GetCurrent<PlayingState>();
        return CardEventPipeline.Apply(eventQueue, playingState?.player, _card, CardStatus, previewMode);
    }

    public void FinishManualAttackResolution()
    {
        if (this == null)
            return;

        _waitingForManualAttackResolution = false;
        used = true;
        played = true;
    }


    bool IsPointerOverThisUIElement()
    {
        return isPointerOver;
    }
}
