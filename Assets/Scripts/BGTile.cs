using System.Collections.Generic;
using Grid;
using UnityEngine;
using Util;

public class BGTile : MonoBehaviour
{
    private SpriteRenderer _mainRenderer;
    private SpriteRenderer _shadowRenderer;
    private EaseScale _easeScale;
    private GOList _goList;
    private GameObject _activeNamedDecoration;

    public EaseScale EaseScale => _easeScale;

    public GameObject decoration;
    private Vector2Int _gridCoords;
    private bool _hasGridCoords;

    private void Awake()
    {
        CacheComponents();

        // BG tiles are static once spawned. The prefab may share generic movement
        // components with gameplay tiles, but 2k idle LerpPosition Updates are expensive.
        foreach (LerpPosition lerpPosition in GetComponentsInChildren<LerpPosition>(true))
        {
            lerpPosition.enabled = false;
            Destroy(lerpPosition);
        }
    }

    public void SetColor(Color color)
    {
        SetColor(color, null);
    }

    public void SetColor(Color color, List<string> decorationNames)
    {
        CacheComponents();

        Color newColor = color;
        newColor.a = 1;
        if (_mainRenderer != null)
            _mainRenderer.color = newColor;

        Color darkColor = color * 0.8f;
        darkColor.a = 1;
        if (_shadowRenderer != null)
            _shadowRenderer.color = darkColor;

        SetRandomDecoration(decorationNames);
    }

    public void SetGridCoords(Vector2Int coords)
    {
        _gridCoords = coords;
        _hasGridCoords = true;
        SetRandomDecoration();
    }

    public void SetRandomDecoration()
    {
        SetRandomDecoration(null);
    }

    public void SetRandomDecoration(List<string> decorationNames)
    {
        if (HasGameplayTileNearby())
        {
            ClearAllDecorations(decorationNames);
            return;
        }

        if (decorationNames != null)
        {
            SetRandomNamedDecoration(decorationNames);
            return;
        }

        ClearNamedDecoration();

        if (decoration == null)
            return;

        if (Random.Range(0, 100) <= 10)
        {
            decoration.SetActive(true);
        }
        else
        {
            decoration.SetActive(false);
        }
    }

    private void SetRandomNamedDecoration(List<string> decorationNames)
    {
        ClearNamedDecorations(decorationNames);

        if (_goList == null)
        {
            if (decoration != null)
                decoration.SetActive(false);

            return;
        }

        List<GameObject> validDecorations = new List<GameObject>();
        foreach (string decorationName in decorationNames)
        {
            if (string.IsNullOrWhiteSpace(decorationName))
                continue;

            if (_goList.TryGetValue(decorationName.Trim(), out GameObject decorationObject) &&
                decorationObject != null)
            {
                validDecorations.Add(decorationObject);
            }
        }

        if (validDecorations.Count == 0)
        {
            if (decoration != null)
                decoration.SetActive(false);

            return;
        }

        bool shouldShowDecoration = Random.Range(0, 100) <= 10;
        _activeNamedDecoration = validDecorations[Random.Range(0, validDecorations.Count)];
        _activeNamedDecoration.SetActive(true);

        if (decoration != null && _activeNamedDecoration.transform.IsChildOf(decoration.transform))
        {
            decoration.SetActive(shouldShowDecoration);
        }
        else if (!shouldShowDecoration)
        {
            _activeNamedDecoration.SetActive(false);
        }
    }

    private void ClearAllDecorations(List<string> decorationNames)
    {
        if (decoration != null)
            decoration.SetActive(false);

        ClearNamedDecorations(decorationNames);
    }

    private void ClearNamedDecorations(List<string> decorationNames)
    {
        ClearNamedDecoration();

        if (_goList == null || decorationNames == null)
            return;

        foreach (string decorationName in decorationNames)
        {
            if (string.IsNullOrWhiteSpace(decorationName))
                continue;

            if (_goList.TryGetValue(decorationName.Trim(), out GameObject decorationObject) &&
                decorationObject != null)
            {
                decorationObject.SetActive(false);
            }
        }
    }

    private void ClearNamedDecoration()
    {
        if (_activeNamedDecoration != null)
        {
            _activeNamedDecoration.SetActive(false);
            _activeNamedDecoration = null;
        }
    }

    private bool HasGameplayTileNearby()
    {
        if (!_hasGridCoords || HexGridManager.Instance == null)
            return false;

        if (IsGameplayTile(_gridCoords))
            return true;

        foreach (Vector2Int adjacentCoords in HexGridManager.AdjacentHexes(_gridCoords))
        {
            if (IsGameplayTile(adjacentCoords))
                return true;
        }

        return false;
    }

    private bool IsGameplayTile(Vector2Int coords)
    {
        return HexGridManager.Instance.BoardDictionary.TryGetValue(coords, out string tileType) &&
               tileType != "none";
    }

    private void CacheComponents()
    {
        if (_mainRenderer == null && transform.childCount > 0)
            _mainRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();

        if (_shadowRenderer == null && transform.childCount > 1)
            _shadowRenderer = transform.GetChild(1).GetComponent<SpriteRenderer>();

        if (_easeScale == null)
            _easeScale = GetComponent<EaseScale>();

        if (_goList == null)
            _goList = GetComponentInChildren<GOList>(true);
    }
}
