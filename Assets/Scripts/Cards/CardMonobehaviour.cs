using System;
using System.Collections.Generic;
using Cards;
using Entities;
using StateManager;
using TMPro;
using Types.Actions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
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

        this.CardClickedCallback = callback;
        
    }

    private void UpdateDiagram()
    {
        foreach (Transform child in diagram.transform)
        {
            Destroy(child.gameObject);
        }
        
        GameObject startingTile = Instantiate(tilePrefab, diagram.transform);
        
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
