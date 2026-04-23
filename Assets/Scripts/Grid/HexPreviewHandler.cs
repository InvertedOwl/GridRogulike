using System;
using System.Collections.Generic;
using Cards.CardEvents;
using Entities;
using Grid;
using StateManager;
using TMPro;
using UnityEngine;

public class HexPreviewHandler : MonoBehaviour
{
    public Dictionary<AbstractEntity, List<AbstractCardEvent>> eventsOnThisHex = new Dictionary<AbstractEntity, List<AbstractCardEvent>>();
    public GOList GoList;
    public Vector2Int currentPos;

    private static readonly Dictionary<Vector2Int, HexPreviewHandler> AllHexHandlers = new Dictionary<Vector2Int, HexPreviewHandler>();

    private bool _disablePreview = false;
    public bool DisablePreview
    {
        get => _disablePreview;
        set
        {
            _disablePreview = value;
            UpdatePreview(new Dictionary<AbstractEntity, List<AbstractCardEvent>>());
        }
    }

    public List<string> arrowUUIDS = new List<string>();

    private void OnEnable()
    {
        AllHexHandlers[currentPos] = this;
    }

    private void Start()
    {
        AllHexHandlers[currentPos] = this;
    }

    private void OnDisable()
    {
        if (AllHexHandlers.TryGetValue(currentPos, out HexPreviewHandler handler) && handler == this)
        {
            AllHexHandlers.Remove(currentPos);
        }
    }

    public void Update()
    {
        if (DisablePreview)
            return;

        UpdatePreview(eventsOnThisHex);
    }

    private void OnMouseEnter()
    {
        if (!GameStateManager.Instance.IsCurrent<PlayingState>())
            return;
        if (!GameStateManager.Instance.GetCurrent<PlayingState>().AllowUserInput)
            return;
        if (eventsOnThisHex.Count == 0)
            return;
    }

    private void OnMouseExit()
    {
        ClearArrows();
    }

    private void ClearArrows()
    {
        foreach (string uuid in arrowUUIDS)
        {
            SpriteArrowManager.Instance.DestroyArrow(uuid);
        }

        arrowUUIDS.Clear();
    }

    public void UpdatePreview(Dictionary<AbstractEntity, List<AbstractCardEvent>> localEvents)
    {
        int amountOfDamage = GetDamageAmount(localEvents);
        bool hasAttack = amountOfDamage > 0;

        if (amountOfDamage > 0)
        {
            GoList.GetValue("TileWarning").SetActive(true);
            GoList.GetValue("TileWarning").transform.GetChild(1).GetComponent<TextMeshProUGUI>().text =
                "This tile will receive " + "<sprite name=\"damage4\"> " + amountOfDamage;
        }
        else
        {
            try
            {
                GoList.GetValue("TileWarning").SetActive(false);
            }
            catch (Exception) { }
        }

        // Disable old single attack indicator
        try
        {
            GoList.GetValue("ToAttack").SetActive(false);
        }
        catch (Exception) { }

        UpdateAttackEdges(hasAttack);
    }

    private int GetDamageAmount(Dictionary<AbstractEntity, List<AbstractCardEvent>> localEvents)
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

        return amountOfDamage;
    }

    private bool HasDamage()
    {
        return GetDamageAmount(eventsOnThisHex) > 0;
    }

    private void UpdateAttackEdges(bool thisHexHasDamage)
    {
        if (!thisHexHasDamage)
        {
            SetEdge("EdgeNParent", false);
            SetEdge("EdgeNEParent", false);
            SetEdge("EdgeNWParent", false);
            SetEdge("EdgeSParent", false);
            SetEdge("EdgeSEParent", false);
            SetEdge("EdgeSWParent", false);
            return;
        }

        SetEdge("EdgeNParent",  !NeighborHasDamage("n"));
        SetEdge("EdgeNEParent", !NeighborHasDamage("ne"));
        SetEdge("EdgeNWParent", !NeighborHasDamage("nw"));
        SetEdge("EdgeSParent",  !NeighborHasDamage("s"));
        SetEdge("EdgeSEParent", !NeighborHasDamage("se"));
        SetEdge("EdgeSWParent", !NeighborHasDamage("sw"));
    }

    private bool NeighborHasDamage(string direction)
    {
        Vector2Int neighborPos = HexGridManager.MoveHex(currentPos, direction, 1);

        if (AllHexHandlers.TryGetValue(neighborPos, out HexPreviewHandler neighbor))
        {
            return !neighbor.DisablePreview && neighbor.HasDamage();
        }

        return false;
    }

    private void SetEdge(string edgeName, bool value)
    {
        try
        {
            GoList.GetValue(edgeName).SetActive(value);
        }
        catch (Exception) { }
    }
}