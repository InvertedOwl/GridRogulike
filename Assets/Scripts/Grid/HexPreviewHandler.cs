using System;
using System.Collections.Generic;
using Cards.CardEvents;
using Entities;
using StateManager;
using TMPro;
using UnityEngine;

public class HexPreviewHandler : MonoBehaviour
{
    public Dictionary<AbstractEntity, List<AbstractCardEvent>> eventsOnThisHex = new Dictionary<AbstractEntity, List<AbstractCardEvent>>();
    public GOList GoList;
    public Vector2Int currentPos;

    private bool _disablePreview = false;
    public bool DisablePreview
    {
        get => _disablePreview;
        set
        {
            _disablePreview = value;
            // Clear previews
            UpdatePreview(new Dictionary<AbstractEntity, List<AbstractCardEvent>>());
        }
    }

    public void Update()
    {
        if (DisablePreview)
            return;
        
        UpdatePreview(eventsOnThisHex);
    }


    public List<String> arrowUUIDS = new List<string>();
    
    private void OnMouseEnter()
    {
        if (!GameStateManager.Instance.IsCurrent<PlayingState>())
            return;
        if (!GameStateManager.Instance.GetCurrent<PlayingState>().AllowUserInput)
            return;
        if (eventsOnThisHex.Count == 0)
            return;
        // ClearArrows();
        //
        // foreach (AbstractEntity entity in eventsOnThisHex.Keys)
        // {
        //     foreach (AbstractCardEvent abstractCardEvent in eventsOnThisHex[entity])
        //     {
        //         if (abstractCardEvent is AttackCardEvent attackCardEvent)
        //         {
        //             arrowUUIDS.Add(SpriteArrowManager.Instance.CreateArrow(entity.positionRowCol, 
        //                 currentPos, Color.red, "AttackIcon", attackCardEvent.amount));
        //         }
        //     }
        // }
    }

    private void OnMouseExit()
    {
        ClearArrows();
    }

    private void ClearArrows()
    {
        foreach (String uuid in arrowUUIDS)
        {
            SpriteArrowManager.Instance.DestroyArrow(uuid);
        }
        
        arrowUUIDS.Clear();
    }
    
    public void UpdatePreview(Dictionary<AbstractEntity, List<AbstractCardEvent>> localEvents)
    {
        int amountOfDamage = 0;

        foreach (AbstractEntity entity in localEvents.Keys)
        {
            foreach (AbstractCardEvent cardEvent in localEvents[entity])
            {
                if (cardEvent is AttackCardEvent attackCardEvent)
                {
                    amountOfDamage += attackCardEvent.amount;
                }
            }
        }
        
        
        Debug.Log("Amount of this tile! " + amountOfDamage);
        if (amountOfDamage > 0)
        {
            GoList.GetValue("TileWarning").SetActive(true);
            GoList.GetValue("TileWarning").transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "This tile will receive "  + "<sprite name=\"damage4\"> " + amountOfDamage.ToString();
        }
        else
        {
            GoList.GetValue("TileWarning").SetActive(false);
        }
        
        
        bool hasAttack = false;

        foreach (AbstractEntity entity in localEvents.Keys)
        {
            foreach (AbstractCardEvent abstractCardEvent in localEvents[entity])
            {
                if (abstractCardEvent is AttackCardEvent)
                {
                    hasAttack = true;
                }
            }
        }


        try
        {
            if (hasAttack)
                GoList.GetValue("ToAttack").SetActive(true);
            else
                GoList.GetValue("ToAttack").SetActive(false);
        } catch (Exception _) {}
    }
}
