using System.Collections;
using Entities;
using TMPro;
using StateManager;
using UnityEngine;
using UnityEngine.UI;
using Util;

public class TileHover : MonoBehaviour
{
    private const float HoverExitMouseTolerancePixels = 3f;

    public LerpPosition lerpPosition;
    public GameObject activateOnHover;
    public int waitTicks = 20;
    public float hoverYOffset = -0.07f;

    private int ticksHovered = 0;
    public bool activeHover = true;

    public bool hoverWhenNotPlaytate = true;
    public bool ignoreOcclusion = false;
    public bool hideWhenEntityHovered = true;
    public string tileType = "";

    public GameObject sideThing;

    private Collider col3D;
    private Collider2D col2D;
    private Camera mainCam;
    private AbstractEntity owningEntity;
    private Coroutine rebuildHoverDetailsCoroutine;
    private bool _animationHoverLatched;
    private bool _directHoverLatched;
    private Vector3 _animationHoverStartMousePosition;
    private Vector3 _directHoverStartMousePosition;

    private static float _cachedFixedTime = -1f;
    private static Camera _cachedCamera;
    private static Vector3 _cachedMousePosition;
    private static Ray _cachedRay;
    private static bool _hasCached3DHit;
    private static Collider _cached3DHit;
    private static bool _hasCached2DHit;
    private static Collider2D _cached2DHit;
    private static Vector2 _cachedMouseWorldPoint;

    void Awake()
    {
        col3D = GetComponent<Collider>();
        col2D = GetComponent<Collider2D>();
        mainCam = Camera.main;
        owningEntity = GetComponentInParent<AbstractEntity>();
    }

    void FixedUpdate()
    {
        if (mainCam == null)
        {
            mainCam = Camera.main;
            if (mainCam == null) return;
        }

        if (!hoverWhenNotPlaytate && !GameStateManager.Instance.IsCurrent<PlayingState>())
        {
            ResetHoverAnimation();
            ResetHoverDetails();
            ResetHoverLatches();
            return;
        }

        bool rawAnimationHovering = IsMouseHovering(includeChildColliders: true);
        bool rawDirectHovering = IsMouseHovering(includeChildColliders: false);
        bool hideForEntityHover = rawDirectHovering && ShouldHideForEntityHover();
        if (hideForEntityHover)
        {
            rawDirectHovering = false;
        }

        bool isAnimationHovering = StabilizeHover(
            rawAnimationHovering,
            ref _animationHoverLatched,
            ref _animationHoverStartMousePosition);
        bool isDirectHovering = StabilizeHover(
            rawDirectHovering,
            ref _directHoverLatched,
            ref _directHoverStartMousePosition,
            hideForEntityHover);

        if (isAnimationHovering && activeHover)
        {
            if (lerpPosition)
            {
                lerpPosition.targetLocation = new Vector3(0, hoverYOffset, lerpPosition.startPosition.z);
            }
        }
        else
        {
            ResetHoverAnimation();
        }

        if (isDirectHovering && activeHover)
        {
            if (ShouldShowTileInfo() && activateOnHover && ticksHovered > waitTicks)
            {
                ShowHoverDetails();
            }

            if (sideThing)
            {
                sideThing.SetActive(false);
            }

            ticksHovered++;
        }
        else
        {
            ResetHoverDetails();
        }
    }

    private bool StabilizeHover(
        bool isCurrentlyHit,
        ref bool isLatched,
        ref Vector3 hoverStartMousePosition,
        bool forceRelease = false)
    {
        if (forceRelease)
        {
            isLatched = false;
            return false;
        }

        Vector3 mousePosition = Input.mousePosition;
        if (isCurrentlyHit)
        {
            isLatched = true;
            hoverStartMousePosition = mousePosition;
            return true;
        }

        if (isLatched &&
            (mousePosition - hoverStartMousePosition).sqrMagnitude <=
            HoverExitMouseTolerancePixels * HoverExitMouseTolerancePixels)
        {
            return true;
        }

        isLatched = false;
        return false;
    }

    private bool IsMouseHovering(bool includeChildColliders)
    {
        UpdateHoverCache(mainCam);

        if (includeChildColliders && IsCachedHitChildOfThisTile())
            return true;

        // 3D collider support
        if (col3D != null)
        {
            if (ignoreOcclusion)
            {
                return col3D.Raycast(_cachedRay, out _, Mathf.Infinity);
            }

            if (!_hasCached3DHit || _cached3DHit == null)
                return false;

            return includeChildColliders
                ? _cached3DHit.transform == transform || _cached3DHit.transform.IsChildOf(transform)
                : _cached3DHit == col3D;
        }

        // 2D collider support
        if (col2D != null)
        {
            if (ignoreOcclusion)
            {
                return col2D.OverlapPoint(_cachedMouseWorldPoint);
            }

            if (!_hasCached2DHit || _cached2DHit == null)
                return false;

            return includeChildColliders
                ? _cached2DHit.transform == transform || _cached2DHit.transform.IsChildOf(transform)
                : _cached2DHit == col2D;
        }

        return false;
    }

    private bool IsCachedHitChildOfThisTile()
    {
        if (_hasCached3DHit && _cached3DHit != null &&
            (_cached3DHit.transform == transform || _cached3DHit.transform.IsChildOf(transform)))
        {
            return true;
        }

        return _hasCached2DHit && _cached2DHit != null &&
               (_cached2DHit.transform == transform || _cached2DHit.transform.IsChildOf(transform));
    }

    private bool ShouldHideForEntityHover()
    {
        if (!hideWhenEntityHovered || owningEntity != null)
        {
            return false;
        }

        return GetHoveredEntity() != null;
    }

    private bool ShouldShowTileInfo()
    {
        return tileType != "basic";
    }

    private AbstractEntity GetHoveredEntity()
    {
        if (_hasCached3DHit && _cached3DHit != null)
        {
            AbstractEntity hoveredEntity = _cached3DHit.GetComponentInParent<AbstractEntity>();
            if (hoveredEntity != null)
            {
                return hoveredEntity;
            }
        }

        if (_hasCached2DHit && _cached2DHit != null)
        {
            AbstractEntity hoveredEntity = _cached2DHit.GetComponentInParent<AbstractEntity>();
            if (hoveredEntity != null)
            {
                return hoveredEntity;
            }
        }

        return null;
    }

    private static void UpdateHoverCache(Camera camera)
    {
        Vector3 mousePosition = Input.mousePosition;
        if (_cachedFixedTime == Time.fixedTime &&
            _cachedCamera == camera &&
            _cachedMousePosition == mousePosition)
        {
            return;
        }

        _cachedFixedTime = Time.fixedTime;
        _cachedCamera = camera;
        _cachedMousePosition = mousePosition;
        _cachedRay = camera.ScreenPointToRay(mousePosition);
        _cachedMouseWorldPoint = camera.ScreenToWorldPoint(mousePosition);

        _hasCached3DHit = Physics.Raycast(_cachedRay, out RaycastHit hit3D, Mathf.Infinity);
        _cached3DHit = _hasCached3DHit ? hit3D.collider : null;

        RaycastHit2D hit2D = Physics2D.GetRayIntersection(_cachedRay, Mathf.Infinity);
        _hasCached2DHit = hit2D.collider != null;
        _cached2DHit = hit2D.collider;
    }

    private void ResetHoverAnimation()
    {
        if (lerpPosition)
        {
            lerpPosition.targetLocation = new Vector3(0, 0, lerpPosition.startPosition.z);
        }
    }

    private void ResetHoverDetails()
    {
        if (activateOnHover)
        {
            activateOnHover.SetActive(false);
        }

        if (rebuildHoverDetailsCoroutine != null)
        {
            StopCoroutine(rebuildHoverDetailsCoroutine);
            rebuildHoverDetailsCoroutine = null;
        }

        if (sideThing)
        {
            sideThing.SetActive(true);
        }

        ticksHovered = 0;
    }

    private void ResetHoverLatches()
    {
        _animationHoverLatched = false;
        _directHoverLatched = false;
    }

    private void ShowHoverDetails()
    {
        bool wasActive = activateOnHover.activeSelf;
        activateOnHover.SetActive(true);

        if (wasActive)
            return;

        RebuildHoverDetailsLayout();

        if (rebuildHoverDetailsCoroutine != null)
        {
            StopCoroutine(rebuildHoverDetailsCoroutine);
        }

        rebuildHoverDetailsCoroutine = StartCoroutine(RebuildHoverDetailsLayoutAtEndOfFrame());
    }

    private IEnumerator RebuildHoverDetailsLayoutAtEndOfFrame()
    {
        yield return new WaitForEndOfFrame();

        if (activateOnHover && activateOnHover.activeInHierarchy)
        {
            RebuildHoverDetailsLayout();
        }

        rebuildHoverDetailsCoroutine = null;
    }

    private void RebuildHoverDetailsLayout()
    {
        if (!activateOnHover)
            return;

        foreach (TextMeshProUGUI text in activateOnHover.GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            text.ForceMeshUpdate();
        }

        RectTransform[] rectTransforms = activateOnHover.GetComponentsInChildren<RectTransform>(true);
        for (int i = rectTransforms.Length - 1; i >= 0; i--)
        {
            RectTransform rectTransform = rectTransforms[i];

            ContentSizeFitter fitter = rectTransform.GetComponent<ContentSizeFitter>();
            if (fitter != null)
            {
                fitter.SetLayoutHorizontal();
                fitter.SetLayoutVertical();
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
        }

        RectTransform root = activateOnHover.transform as RectTransform;
        while (root != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(root);
            root = root.parent as RectTransform;
        }

        Canvas.ForceUpdateCanvases();
    }
}
