using Entities;
using StateManager;
using UnityEngine;
using Util;

public class TileHover : MonoBehaviour
{
    public LerpPosition lerpPosition;
    public GameObject activateOnHover;
    public int waitTicks = 20;
    public float hoverYOffset = -0.07f;

    private int ticksHovered = 0;
    public bool activeHover = true;

    public bool hoverWhenNotPlaytate = true;
    public bool ignoreOcclusion = false;
    public bool hideWhenEntityHovered = true;

    public GameObject sideThing;

    private Collider col3D;
    private Collider2D col2D;
    private Camera mainCam;
    private AbstractEntity owningEntity;

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
            ResetHoverState();
            return;
        }

        bool isHovering = IsMouseHovering();
        if (isHovering && ShouldHideForEntityHover())
        {
            isHovering = false;
        }

        if (isHovering && activeHover)
        {
            if (lerpPosition)
            {
                lerpPosition.targetLocation = new Vector3(0, hoverYOffset, lerpPosition.startPosition.z);
            }

            if (activateOnHover && ticksHovered > waitTicks)
            {
                activateOnHover.SetActive(true);
            }

            if (sideThing)
            {
                sideThing.SetActive(false);
            }

            ticksHovered++;
        }
        else
        {
            ResetHoverState();
        }
    }

    private bool IsMouseHovering()
    {
        UpdateHoverCache(mainCam);

        // 3D collider support
        if (col3D != null)
        {
            if (ignoreOcclusion)
            {
                return col3D.Raycast(_cachedRay, out _, Mathf.Infinity);
            }

            return _hasCached3DHit && _cached3DHit == col3D;
        }

        // 2D collider support
        if (col2D != null)
        {
            if (ignoreOcclusion)
            {
                return col2D.OverlapPoint(_cachedMouseWorldPoint);
            }

            return _hasCached2DHit && _cached2DHit == col2D;
        }

        return false;
    }

    private bool ShouldHideForEntityHover()
    {
        if (!hideWhenEntityHovered || owningEntity != null)
        {
            return false;
        }

        return GetHoveredEntity() != null;
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

    private void ResetHoverState()
    {
        if (lerpPosition)
        {
            lerpPosition.targetLocation = new Vector3(0, 0, lerpPosition.startPosition.z);
        }

        if (activateOnHover)
        {
            activateOnHover.SetActive(false);
        }

        if (sideThing)
        {
            sideThing.SetActive(true);
        }

        ticksHovered = 0;
    }
}
