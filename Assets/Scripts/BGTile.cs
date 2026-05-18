using UnityEngine;
using Util;

public class BGTile : MonoBehaviour
{
    private SpriteRenderer _mainRenderer;
    private SpriteRenderer _shadowRenderer;
    private EaseScale _easeScale;

    public EaseScale EaseScale => _easeScale;

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
        CacheComponents();

        Color newColor = color;
        newColor.a = 1;
        if (_mainRenderer != null)
            _mainRenderer.color = newColor;

        Color darkColor = color * 0.8f;
        darkColor.a = 1;
        if (_shadowRenderer != null)
            _shadowRenderer.color = darkColor;
    }

    private void CacheComponents()
    {
        if (_mainRenderer == null && transform.childCount > 0)
            _mainRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();

        if (_shadowRenderer == null && transform.childCount > 1)
            _shadowRenderer = transform.GetChild(1).GetComponent<SpriteRenderer>();

        if (_easeScale == null)
            _easeScale = GetComponent<EaseScale>();
    }
}
