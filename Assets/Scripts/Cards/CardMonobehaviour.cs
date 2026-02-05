using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Cards;
using Cards.Actions;
using Entities;
using StateManager;
using TMPro;
using Cards.CardEvents;
using Cards.CardList;
using Grid;
using Passives;
using Types.Tiles;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Util;
using Random = System.Random;

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
    
    public Dictionary<TextMeshProUGUI, string> TypeTitles = new Dictionary<TextMeshProUGUI, string>();
    
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI costText;
    public TextMeshProUGUI actionText;
    public bool used = false;
    private Card _card;
    
    public Card Card => _card;
    public float CostOverride = -1f;
    
    private bool _cardSet;
    
    public GameObject MainPanel;

    public GameObject attackPrefab;
    public GameObject movePrefab;
    public GameObject shieldPrefab;
    public GameObject normalPrefab;
    public GameObject poisonPrefab;
    public GameObject drawPrefab;
    public GameObject diagram;
    public GameObject tilePrefab;
    public GameObject arrowPrefab;
    public GameObject inactiveImage;
    public GOList GoList;
    
    public GameObject Condition;
    public GameObject Modifier;
    public GameObject InfoPanel;
    public InfoData InfoData;
    
    public float hoverScale = 1.05f;
    public float hoverOffset = 60;
    
    public List<GameObject> types = new List<GameObject>();

    public bool inactive;

    public Action CardClickedCallback;
    public int sortingLayer = 170;
    public int siblingIndex;

    private Random _cardRandom;

    public Image modifierBG;

    public void SetCard(Card card, Action callback = null, bool active = true, float costOverride = -1f)
    {
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
        inactive = !active;
        inactiveImage.SetActive(!active);

        this.CardClickedCallback = callback;
        _cardRandom = card.cardRandom;
    }

    public string FormatTextForInfo(string info)
    {
        string newInfo = info;

        foreach (InfoEntry entry in InfoData.info)
        {
            if (newInfo.ToLower().Contains(entry.name))
            {
                newInfo = Regex.Replace(newInfo, entry.name, entry.formattedName, RegexOptions.IgnoreCase);
                entry.infoPanel.SetActive(true);
            }
        }

        foreach (string key in BattleStats.names.Keys)
        {
            if (newInfo.ToLower().Contains(key))
            {
                newInfo = Regex.Replace(newInfo, Regex.Escape(key), BattleStats.names[key](), RegexOptions.IgnoreCase);
            }
        }
        
        return newInfo;
    }

    private void UpdateBuff()
    {
        // this.Modifier.GetComponent<TextMeshProUGUI>().text = FormatTextForInfo(_card.Modifier.ModifierText);
        // this.Condition.GetComponent<TextMeshProUGUI>().text = FormatTextForInfo(_card.Condition.ConditionText);
        
    }
    
    private void UpdateDiagram()
    {
        foreach (Transform child in diagram.transform)
        {
            Destroy(child.gameObject);
        }
        
        List<RectTransform> allElements = new List<RectTransform>();
        
        allElements.Add(Instantiate(tilePrefab, diagram.transform).GetComponent<RectTransform>());

        foreach (AbstractAction action in _card.Actions)
        {
            allElements.AddRange(action.UpdateGraphic(diagram, tilePrefab, arrowPrefab));
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
        int posY = -175;
        foreach (AbstractAction action in _card.Actions)
        {
            GameObject text = null;

            switch (action)
            {
                case MoveAction moveAction:
                    text = Instantiate(movePrefab, MainPanel.transform);
                    RotateArrow(moveAction.Direction, text.transform.GetChild(2));
                    break;

                case PoisonAttackAction:
                    text = Instantiate(poisonPrefab, MainPanel.transform);
                    break;

                case AttackAction attackAction:
                    text = Instantiate(attackPrefab, MainPanel.transform);
                    RotateArrow(attackAction.Direction, text.transform.GetChild(3));
                    break;

                case ShieldAction:
                    text = Instantiate(shieldPrefab, MainPanel.transform);
                    GoList.GetValue("ShieldInfo").SetActive(true);
                    break;

                case DrawCardAction:
                    text = Instantiate(drawPrefab, MainPanel.transform);
                    break;
                

                default:
                    text = Instantiate(normalPrefab, MainPanel.transform);
                    break;
            }

            text.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = FormatTextForInfo(action.GetText());
            TypeTitles[text.transform.GetChild(1).GetComponent<TextMeshProUGUI>()] = action.GetText();
            types.Add(text);
            text.GetComponent<RectTransform>().localPosition = new Vector2(0, posY);

            posY -= 140;
        }
    }

    public void UpdateTypesTitles()
    {
        foreach (TextMeshProUGUI text in TypeTitles.Keys)
        {
            text.text = FormatTextForInfo(TypeTitles[text]);
        }
    }

    public void RotateArrow(string direction, Transform arrow)
    {
        switch (direction)
        {
            case "n": arrow.eulerAngles = new Vector3(0, 0, 0); break;
            case "s": arrow.eulerAngles = new Vector3(0, 0, 180); break;
            case "ne": arrow.eulerAngles = new Vector3(0, 0, -45); break;
            case "nw": arrow.eulerAngles = new Vector3(0, 0, 45); break;
            case "se": arrow.eulerAngles = new Vector3(0, 0, -135); break;
            case "sw": arrow.eulerAngles = new Vector3(0, 0, 135); break;
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

    public void UpdateCondition()
    {
        if (_card.Condition == null)
            return;
        if (GameStateManager.Instance.IsCurrent<PlayingState>())
        {
            if (_card.Condition.Condition(_card))
            {
                modifierBG.color = new Color(1, 1, 1, 1);
            }
            else
            {
                modifierBG.color = new Color(1, 1, 1, 0.5f);
            }
        }
        else
        {
            modifierBG.color = new Color(1, 1, 1, 1);
        }
    }

    private void HandleHoverEffects()
    {
        InfoPanel.SetActive(true);
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
    }
    
    private void ResetHoverEffects()
    {
        InfoPanel.SetActive(false);
        LerpPosition lerpPosition = transform.GetChild(0).GetChild(0).GetComponent<LerpPosition>();
        lerpPosition.targetLocation = new Vector2(0, 0);
        lerpPosition.targetScale = lerpPosition.startScale * 1f;
        GetComponent<Canvas>().overrideSorting = false;
        foreach (AbstractAction action in _card.Actions)
        {
            action.NotHover();
        }
    }

    private void HandleCardUsage()
    {
        if (!_cardSet || inactive)
            return;
        
        bool isLeftClick = Input.GetMouseButtonDown(0);
        bool hasEnoughEnergy = RunInfo.Instance.CurrentEnergy >= ((CostOverride>-1)?CostOverride:_card.Cost);
        bool isPlayerTurn = false;
        if (GameStateManager.Instance.GetCurrent<PlayingState>() is { } playing)
            isPlayerTurn = playing.CurrentTurn is Player;
        
        if (isLeftClick && !used && hasEnoughEnergy && isPlayerTurn)
        {
            Player player = GameStateManager.Instance.GetCurrent<PlayingState>().player;
            
            List<AbstractCardEvent> eventQueue = new List<AbstractCardEvent>();

            BattleStats.CardsPlayedThisBattle += 1;
            BattleStats.CardsPlayedThisTurn += 1;
            
            // Build queue
            foreach (AbstractAction action in _card.Actions)
            {
                List<AbstractCardEvent> cardEvents = action.Activate(cardMono:this);

                foreach (AbstractCardEvent cardEvent in cardEvents)
                {
                    eventQueue.Add(cardEvent);
                }
                
            }
            
            // Only modify events if condition is satisfied
            if (_card.Condition != null && _card.Modifier != null && _card.Condition.Condition(_card))
                eventQueue = _card.Modifier.Modify(eventQueue);



            foreach (PassiveEntry entry in EnvironmentManager.instance.GetPassiveEntries())
            {
                Debug.Log(entry + " Current entry");
                if (entry.Condition.Condition(_card))
                {
                    eventQueue = entry.CardModifier.Modify(eventQueue);
                    Debug.Log("ACTIVATED MODIFIER");
                        
                }
            }
            
            // Modify by tile
            Vector2Int playerPos = GameStateManager.Instance.GetCurrent<PlayingState>().player.positionRowCol;
            TileEntry tile = TileData.tiles[HexGridManager.Instance.HexType(playerPos)];
            eventQueue = tile.cardModifier.Invoke(eventQueue);

            // Activate queue
            foreach (AbstractCardEvent cardEvent in eventQueue)
            {
                cardEvent.Activate(player);
            }

            RunInfo.Instance.CurrentEnergy -= (int)((CostOverride>-1)?CostOverride:_card.Cost);
            used = true;
        }
        
        if (isLeftClick)
            CardClickedCallback?.Invoke();
    }


    bool IsPointerOverThisUIElement()
    {
        return isPointerOver;
    }
}
