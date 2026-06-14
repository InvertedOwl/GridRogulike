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
    private static readonly Dictionary<AbstractEntity, List<AbstractCardEvent>> EmptyPreviewEvents =
        new Dictionary<AbstractEntity, List<AbstractCardEvent>>();
    private static int _globalPreviewRevision;

    private bool _forcePreviewRefresh = true;
    private bool _hasLocalPreviewSignature;
    private int _lastLocalPreviewSignature;
    private int _lastRenderedGlobalPreviewRevision = -1;

    public static void ResetStatics()
    {
        AllHexHandlers.Clear();
        EmptyPreviewEvents.Clear();
        _globalPreviewRevision = 0;
    }

    private bool _disablePreview = false;
    public bool DisablePreview
    {
        get => _disablePreview;
        set
        {
            _disablePreview = value;
            _forcePreviewRefresh = true;
            UpdatePreview(EmptyPreviewEvents);
            _lastRenderedGlobalPreviewRevision = _globalPreviewRevision;
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

        RefreshPreviewIfNeeded();
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

    public void MarkPreviewDirty()
    {
        _forcePreviewRefresh = true;
        _hasLocalPreviewSignature = false;
        _globalPreviewRevision++;
    }

    public void ClearPreviewEvents()
    {
        if (eventsOnThisHex.Count == 0)
            return;

        eventsOnThisHex.Clear();
        MarkPreviewDirty();
    }

    public void RemoveEventsForEntity(AbstractEntity entity)
    {
        if (eventsOnThisHex.Remove(entity))
            MarkPreviewDirty();
    }

    public void RemoveEventsForEntityAction(AbstractEntity entity, int actionIndex)
    {
        if (!eventsOnThisHex.TryGetValue(entity, out List<AbstractCardEvent> entityEvents))
            return;

        bool removed = entityEvents.RemoveAll(cardEvent => cardEvent.PreviewSourceActionIndex == actionIndex) > 0;
        if (entityEvents.Count == 0)
            eventsOnThisHex.Remove(entity);

        if (removed)
            MarkPreviewDirty();
    }

    public void AddPreviewEvent(AbstractEntity entity, AbstractCardEvent cardEvent)
    {
        if (eventsOnThisHex.ContainsKey(entity))
            eventsOnThisHex[entity].Add(cardEvent);
        else
            eventsOnThisHex[entity] = new List<AbstractCardEvent> { cardEvent };

        MarkPreviewDirty();
    }

    private void RefreshPreviewIfNeeded()
    {
        int localPreviewSignature = GetPreviewSignature(eventsOnThisHex);

        if (!_hasLocalPreviewSignature)
        {
            _lastLocalPreviewSignature = localPreviewSignature;
            _hasLocalPreviewSignature = true;
            _forcePreviewRefresh = true;
        }
        else if (_lastLocalPreviewSignature != localPreviewSignature)
        {
            _lastLocalPreviewSignature = localPreviewSignature;
            _globalPreviewRevision++;
            _forcePreviewRefresh = true;
        }

        if (!_forcePreviewRefresh && _lastRenderedGlobalPreviewRevision == _globalPreviewRevision)
            return;

        UpdatePreview(eventsOnThisHex);
        _lastRenderedGlobalPreviewRevision = _globalPreviewRevision;
        _forcePreviewRefresh = false;
    }

    private int GetPreviewSignature(Dictionary<AbstractEntity, List<AbstractCardEvent>> localEvents)
    {
        unchecked
        {
            int hash = 17;
            foreach (KeyValuePair<AbstractEntity, List<AbstractCardEvent>> entry in localEvents)
            {
                hash = hash * 31 + (entry.Key != null ? entry.Key.GetInstanceID() : 0);
                hash = hash * 31 + (entry.Value != null ? entry.Value.Count : 0);

                if (entry.Value == null)
                    continue;

                foreach (AbstractCardEvent cardEvent in entry.Value)
                {
                    if (cardEvent is AttackCardEvent attackCardEvent)
                    {
                        hash = hash * 31 + attackCardEvent.amount;
                    }
                }
            }

            return hash;
        }
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
        foreach (string direction in HexGridManager.HexDirections)
        {
            SetEdge(GetEdgeName(direction), thisHexHasDamage && !NeighborHasDamage(direction));
        }
    }

    private string GetEdgeName(string direction)
    {
        return direction switch
        {
            "e" => "EdgeSEParent",
            "ne" => "EdgeNEParent",
            "nw" => "EdgeNParent",
            "w" => "EdgeNWParent",
            "sw" => "EdgeSWParent",
            "se" => "EdgeSParent",
            _ => ""
        };
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
        if (string.IsNullOrEmpty(edgeName))
            return;

        try
        {
            GoList.GetValue(edgeName).SetActive(value);
        }
        catch (Exception) { }
    }
}
