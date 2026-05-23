using StateManager;
using UnityEngine;

public class ZoomCamera : MonoBehaviour
{
    public Camera targetCamera;
    public Transform zoomTransform;
    public float zoomSpeed = 2f;
    public float zoomEaseSpeed = 10f;
    public float minMoveDistance = -5f;
    public float maxMoveDistance = 5f;
    public bool requireMouseOnScreen = true;

    private float _currentMoveDistance;
    private float _targetMoveDistance;

    public Transform ZoomTransform
    {
        get
        {
            ResolveReferences();
            return zoomTransform;
        }
    }

    public Vector3 CurrentWorldOffset
    {
        get
        {
            ResolveReferences();
            return zoomTransform != null ? GetZoomDirection() * _currentMoveDistance : Vector3.zero;
        }
    }

    public float CurrentMoveDistance => _currentMoveDistance;
    public float TargetMoveDistance => _targetMoveDistance;

    public Vector3 ZoomDirection
    {
        get
        {
            ResolveReferences();
            return GetZoomDirection();
        }
    }

    private void Awake()
    {
        ResolveReferences();
    }

    private void Update()
    {
        ResolveReferences();

        if (!CanControlCamera())
            return;

        if (requireMouseOnScreen && !IsMouseOnScreen())
        {
            return;
        }

        float scroll = GetScroll();
        if (!Mathf.Approximately(scroll, 0f))
        {
            SetZoomTarget(scroll);
        }
    }

    private void LateUpdate()
    {
        ResolveReferences();

        if (!CanControlCamera() && !CanAutoCamera())
        {
            _targetMoveDistance = _currentMoveDistance;
            return;
        }

        EaseZoom();
    }

    private void SetZoomTarget(float scroll)
    {
        if (zoomTransform == null)
        {
            return;
        }

        _targetMoveDistance = Mathf.Clamp(_targetMoveDistance + scroll * zoomSpeed, minMoveDistance, maxMoveDistance);
    }

    public void SetZoomDistance(float moveDistance)
    {
        ResolveReferences();
        _targetMoveDistance = Mathf.Clamp(moveDistance, minMoveDistance, maxMoveDistance);
    }

    public void InstantSetZoomDistance(float moveDistance)
    {
        ResolveReferences();
        moveDistance = Mathf.Clamp(moveDistance, minMoveDistance, maxMoveDistance);
        ApplyZoomDelta(moveDistance - _currentMoveDistance);
        _currentMoveDistance = moveDistance;
        _targetMoveDistance = moveDistance;
    }

    private void ResolveReferences()
    {
        if (targetCamera == null)
        {
            targetCamera = GetComponent<Camera>();

            if (targetCamera == null)
                targetCamera = GetComponentInChildren<Camera>();

            if (targetCamera == null)
                targetCamera = GetComponentInParent<Camera>();

            if (targetCamera == null)
                targetCamera = Camera.main;
        }

        if (zoomTransform == null)
        {
            zoomTransform = targetCamera != null ? targetCamera.transform : transform;
        }
    }

    private void EaseZoom()
    {
        if (zoomTransform == null)
            return;

        float nextMoveDistance = Mathf.Lerp(_currentMoveDistance, _targetMoveDistance, GetEaseT());

        if (Mathf.Abs(nextMoveDistance - _targetMoveDistance) < 0.001f)
            nextMoveDistance = _targetMoveDistance;

        ApplyZoomDelta(nextMoveDistance - _currentMoveDistance);
        _currentMoveDistance = nextMoveDistance;
    }

    private void ApplyZoomDelta(float delta)
    {
        if (Mathf.Approximately(delta, 0f))
            return;

        zoomTransform.position += GetZoomDirection() * delta;
    }

    private Vector3 GetZoomDirection()
    {
        if (zoomTransform == null)
            return Vector3.forward;

        Vector3 direction = zoomTransform.forward;
        return direction.sqrMagnitude > 0.001f ? direction.normalized : Vector3.forward;
    }

    private float GetEaseT()
    {
        return 1f - Mathf.Exp(-zoomEaseSpeed * Time.deltaTime);
    }

    private float GetScroll()
    {
        float scroll = Input.mouseScrollDelta.y;

        if (Mathf.Approximately(scroll, 0f))
            scroll = Input.GetAxis("Mouse ScrollWheel");

        return scroll;
    }

    private bool IsMouseOnScreen()
    {
        Vector3 mousePosition = Input.mousePosition;
        return mousePosition.x >= 0f &&
               mousePosition.y >= 0f &&
               mousePosition.x <= Screen.width &&
               mousePosition.y <= Screen.height;
    }

    private bool CanControlCamera()
    {
        return !GameplayNavSettings.autocamera &&
               GameStateManager.Instance != null &&
               (GameStateManager.Instance.IsCurrent<PlayingState>() ||
                GameStateManager.Instance.IsCurrent<TilePickState>());
    }

    private bool CanAutoCamera()
    {
        return GameplayNavSettings.autocamera &&
               GameStateManager.Instance != null &&
               GameStateManager.Instance.IsCurrent<PlayingState>();
    }

    private void OnValidate()
    {
        if (maxMoveDistance < minMoveDistance)
            maxMoveDistance = minMoveDistance;

        if (zoomEaseSpeed < 0f)
            zoomEaseSpeed = 0f;
    }
}
