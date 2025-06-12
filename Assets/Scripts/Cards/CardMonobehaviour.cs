using System;
using System.Collections.Generic;
using Cards;
using Entities;
using Grid;
using StateManager;
using TMPro;
using Types.Actions;
using Types.Tiles;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Util;
using Random = System.Random;

public class CardMonobehaviour : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI costText;
    public TextMeshProUGUI actionText;
    public bool used = false;
    private Card _card;
    private bool _cardSet;
    
    public GameObject MainPanel;

    public GameObject attackPrefab;
    public GameObject movePrefab;
    public GameObject diagram;
    public GameObject tilePrefab;
    public GameObject arrowPrefab;
    
    public List<GameObject> types = new List<GameObject>();

    public Action CardClickedCallback;
    public int siblingIndex;

    public void SetCard(Card card, Action callback = null)
    {
        _card = card;
        _cardSet = true;
        
        titleText.text = _card.CardName;
        costText.text = _card.Cost.ToString();

        UpdateTypes();
        UpdateDiagram();
        MainPanel.GetComponent<Image>().color = CardRarityColors.GetColor(card.Rarity);

        this.CardClickedCallback = callback;
        
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
            if (action is MoveAction)
            {
                MoveAction moveAction = action as MoveAction;
                
                GameObject basic = Instantiate(tilePrefab, diagram.transform);
                Vector2Int newPos =
                    HexGridManager.MoveHex(new Vector2Int(0, 0), moveAction.Direction, moveAction.Distance);
                Vector2 newPosWorld = HexGridManager.GetHexCenter(newPos.x, newPos.y) * 46.2222f;
                
                basic.GetComponent<RectTransform>().localPosition = newPosWorld;
                basic.GetComponent<Image>().color = TileData.tiles["basic"].color;
                allElements.Add(basic.GetComponent<RectTransform>());
            }
            if (action is AttackAction)
            {
                AttackAction moveAction = action as AttackAction;
                
                GameObject basic = Instantiate(tilePrefab, diagram.transform);
                Vector2Int newPos =
                    HexGridManager.MoveHex(new Vector2Int(0, 0), moveAction.Direction, moveAction.Distance);
                Vector2 newPosWorld = HexGridManager.GetHexCenter(newPos.x, newPos.y) * 46.2222f;
                
                basic.GetComponent<RectTransform>().localPosition = newPosWorld;
                basic.GetComponent<Image>().color = new Color(212/255.0f, 81/255.0f, 81/255.0f);
                allElements.Add(basic.GetComponent<RectTransform>());
            }
        }
        
        foreach (AbstractAction action in _card.Actions)
        {
            if (action is MoveAction)
            {
                MoveAction moveAction = action as MoveAction;
                GameObject move = Instantiate(arrowPrefab, diagram.transform);
                move.GetComponent<ArrowController>().SetHeight(40 * moveAction.Distance);
                Vector2Int newPos =
                    HexGridManager.MoveHex(new Vector2Int(0, 0), moveAction.Direction, moveAction.Distance);
                Vector2 newPosWorld = HexGridManager.GetHexCenter(newPos.x, newPos.y);
                
                float angle = Vector2.SignedAngle(Vector2.up, newPosWorld);
                
                move.transform.eulerAngles = new Vector3(0, 0, angle);
                allElements.Add(move.GetComponent<RectTransform>());
            }
        }

        Vector3 averagePos = GetUIAveragePosition(allElements);
        Vector2 uiBounds = GetUIBounds(allElements);

        foreach (RectTransform rectTransform in allElements)
        {
            rectTransform.localPosition -= averagePos;
        }
        
        // float maxSide = Mathf.Max(uiBounds.x, uiBounds.y);
        // float sideOut = maxSide/120.0f;
        // Debug.Log(sideOut + " Side out");
        //
        // if (sideOut > 1)
        // {
        //     diagram.transform.localScale = new Vector3(1/sideOut, 1/sideOut, 1);
        // }
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
        int posY = 170;
        foreach (AbstractAction action in _card.Actions)
        {
            if (action is MoveAction)
            {
                GameObject text = Instantiate(movePrefab, MainPanel.transform);
                types.Add(text);
                text.GetComponent<RectTransform>().localPosition = new Vector2(0, posY);
                text.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = ((MoveAction)action).Distance.ToString();
                RotateArrow(((MoveAction)action).Direction, text.transform.GetChild(1));
            }
            
            if (action is AttackAction)
            {
                GameObject text = Instantiate(attackPrefab, MainPanel.transform);
                types.Add(text);
                text.GetComponent<RectTransform>().localPosition = new Vector2(0, posY);
                text.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = ((AttackAction)action).Amount.ToString();
                RotateArrow(((AttackAction)action).Direction, text.transform.GetChild(2));
            }
            
            posY -= 100;
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
        if (IsPointerOverThisUIElement() && _cardSet)
        {
            HandleHoverEffects();
            HandleCardUsage();
        }
        else if (_cardSet)
        {
            ResetHoverEffects();
        }
    }

    private void HandleHoverEffects()
    {
        siblingIndex = transform.GetSiblingIndex();
        LerpPosition lerpPosition = transform.GetChild(0).GetComponent<LerpPosition>();
        lerpPosition.targetLocation = new Vector2(0, 60);
        lerpPosition.targetScale = lerpPosition.startScale * 1.05f;
        transform.SetSiblingIndex(50);


        if (!_cardSet)
            return;
        if (_card.Actions.Count < 0)
            return;

        foreach (AbstractAction action in _card.Actions)
        {
            action.Hover();
        }
    }

    private void HandleCardUsage()
    {
        if (!_cardSet)
            return;
        
        bool isLeftClick = Input.GetMouseButtonDown(0);
        bool hasEnoughEnergy = RunInfo.Instance.CurrentEnergy >= _card.Cost;
        bool isPlayerTurn = false;
        if (GameStateManager.Instance.GetCurrent<PlayingState>() is { } playing)
            isPlayerTurn = playing.CurrentTurn is Player;
        
        if (isLeftClick && !used && hasEnoughEnergy && isPlayerTurn)
        {
            foreach (AbstractAction action in _card.Actions)
            {
                action.Activate();
            }

            RunInfo.Instance.CurrentEnergy -= _card.Cost;
            used = true;
        }
        
        if (isLeftClick)
            CardClickedCallback?.Invoke();
    }

    private void ResetHoverEffects()
    {
        // transform.SetSiblingIndex(siblingIndex);
        LerpPosition lerpPosition = transform.GetChild(0).GetComponent<LerpPosition>();
        lerpPosition.targetLocation = new Vector2(0, 0);
        lerpPosition.targetScale = lerpPosition.startScale * 1f;
    }


    bool IsPointerOverThisUIElement()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        if (results.Count == 0) return false;
        if (results[0].gameObject.transform.IsChildOf(gameObject.transform))
        {
            return true;
        }

        return false;
    }
}
