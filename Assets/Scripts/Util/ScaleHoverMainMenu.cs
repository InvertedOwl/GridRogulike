using System.Collections.Generic;
using UnityEngine;

public class ScaleHoverMainMenu : MonoBehaviour
{
    [SerializeField] private float hoverScaleMultiplier = 0.5f;
    [SerializeField] private float hoverScaleDurationSeconds = 0.1f;

    private readonly Dictionary<Transform, TileScaleState> _tileScaleStates = new Dictionary<Transform, TileScaleState>();
    private readonly HashSet<Transform> _hoveredTilesThisFrame = new HashSet<Transform>();
    private readonly List<Transform> _trackedTiles = new List<Transform>();

    void Update()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
            return;

        Vector2 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Collider2D[] hits = Physics2D.OverlapPointAll(mouseWorldPos);
        _hoveredTilesThisFrame.Clear();

        foreach (Collider2D hit in hits)
        {
            BGTile tile = hit.GetComponentInParent<BGTile>();
            if (tile == null)
                continue;

            Transform tileTransform = tile.transform;
            _hoveredTilesThisFrame.Add(tileTransform);
            SetTileHovered(tileTransform, true);
        }

        UpdateExitedTiles();
    }

    private void SetTileHovered(Transform tile, bool isHovered)
    {
        TileScaleState state = GetTileScaleState(tile);
        if (state == null || state.IsHovered == isHovered)
            return;

        state.IsHovered = isHovered;
        float scaleMultiplier = isHovered ? hoverScaleMultiplier : 1f;

        foreach (ScaleTarget target in state.Targets)
        {
            if (target.Transform == null || target.EaseScale == null)
                continue;

            target.EaseScale.durationSeconds = hoverScaleDurationSeconds;
            target.EaseScale.SetScale(target.NormalScale * scaleMultiplier);
        }
    }

    private TileScaleState GetTileScaleState(Transform tile)
    {
        if (tile == null)
            return null;

        if (_tileScaleStates.TryGetValue(tile, out TileScaleState state))
            return state;

        state = new TileScaleState();
        for (int i = 0; i < tile.childCount; i++)
        {
            Transform child = tile.GetChild(i);
            if (child.GetComponent<SpriteRenderer>() == null)
                continue;

            EaseScale easeScale = child.GetComponent<EaseScale>();
            if (easeScale == null)
                easeScale = child.gameObject.AddComponent<EaseScale>();

            easeScale.durationSeconds = hoverScaleDurationSeconds;

            state.Targets.Add(new ScaleTarget
            {
                Transform = child,
                EaseScale = easeScale,
                NormalScale = child.localScale
            });
        }

        _tileScaleStates[tile] = state;
        return state;
    }

    private void UpdateExitedTiles()
    {
        _trackedTiles.Clear();

        foreach (Transform tile in _tileScaleStates.Keys)
        {
            _trackedTiles.Add(tile);
        }

        foreach (Transform tile in _trackedTiles)
        {
            if (tile == null)
            {
                _tileScaleStates.Remove(tile);
                continue;
            }

            if (!_hoveredTilesThisFrame.Contains(tile))
                SetTileHovered(tile, false);
        }
    }

    private void OnValidate()
    {
        hoverScaleMultiplier = Mathf.Max(0f, hoverScaleMultiplier);
        hoverScaleDurationSeconds = Mathf.Max(0f, hoverScaleDurationSeconds);
    }

    private class TileScaleState
    {
        public readonly List<ScaleTarget> Targets = new List<ScaleTarget>();
        public bool IsHovered;
    }

    private class ScaleTarget
    {
        public Transform Transform;
        public EaseScale EaseScale;
        public Vector3 NormalScale;
    }
}
