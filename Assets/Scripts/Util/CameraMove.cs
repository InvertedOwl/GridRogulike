using UnityEngine;
using StateManager;

public class CameraMove : MonoBehaviour
{
    public Transform moveTransform;
    public ZoomCamera zoomCamera;
    public MoveWithMouse moveWithMouse;
    public float moveSpeed = 5f;
    public float smoothTime = 0.12f;
    public bool useLocalPosition = true;
    public bool useMoveBounds = true;
    public bool compensateForZoom = true;
    public Vector2 minBounds = new Vector2(-10f, -10f);
    public Vector2 maxBounds = new Vector2(10f, 10f);
    public Vector2 boundsOffset;
    [Header("Auto Camera")]
    public Vector2 autoCameraOffset = new Vector2(0f, -3.2f);

    private Vector3 _targetPosition;
    private Vector3 _velocity;
    private bool _hasTargetPosition;
    private bool _autoCameraActive;

    private void Awake()
    {
        ResolveReferences();

        if (moveTransform != null)
            SetTargetPosition(GetBasePosition());
    }

    private void Update()
    {
        ResolveReferences();

        if (moveTransform == null)
            return;

        if (!_hasTargetPosition)
            SetTargetPosition(GetBasePosition());

        Vector3 basePosition = GetBasePosition();

        if (!_autoCameraActive && IsCameraInputAllowed())
        {
            Vector2 input = GetMoveInput();
            if (input.sqrMagnitude > 1f)
                input.Normalize();

            _targetPosition += new Vector3(input.x, input.y, 0f) * (moveSpeed * Time.deltaTime);
            _targetPosition.z = basePosition.z;
        }

        ClampTargetPosition();

        Vector3 nextBasePosition = Vector3.SmoothDamp(
            basePosition,
            _targetPosition,
            ref _velocity,
            smoothTime
        );

        SetMovePosition(nextBasePosition + GetZoomOffset());
    }

    public void SetAutoCameraTarget(Vector3 worldCenter)
    {
        ResolveReferences();

        if (moveTransform == null)
            return;

        Vector3 targetPosition = useLocalPosition && moveTransform.parent != null
            ? moveTransform.parent.InverseTransformPoint(worldCenter)
            : worldCenter;

        targetPosition.x += autoCameraOffset.x;
        targetPosition.y += autoCameraOffset.y;
        targetPosition.z = GetBasePosition().z;

        SetTargetPosition(targetPosition);
        ClampTargetPosition();
        _autoCameraActive = true;
    }

    public void ClearAutoCameraTarget()
    {
        if (!_autoCameraActive)
            return;

        ResolveReferences();

        if (moveTransform != null)
            SetTargetPosition(GetBasePosition());

        _autoCameraActive = false;
    }

    private void ResolveReferences()
    {
        if (moveTransform == null)
            moveTransform = transform;

        if (moveWithMouse == null)
        {
            moveWithMouse = GetComponent<MoveWithMouse>();

            if (moveWithMouse == null && moveTransform != null)
                moveWithMouse = moveTransform.GetComponent<MoveWithMouse>();
        }

        if (zoomCamera != null)
            return;

        zoomCamera = GetComponent<ZoomCamera>();

        if (zoomCamera == null && moveTransform != null)
            zoomCamera = moveTransform.GetComponent<ZoomCamera>();

        if (zoomCamera == null)
            zoomCamera = GetComponentInChildren<ZoomCamera>();

        if (zoomCamera == null && moveTransform != null && moveTransform != transform)
            zoomCamera = moveTransform.GetComponentInChildren<ZoomCamera>();
    }

    private void SetTargetPosition(Vector3 position)
    {
        _targetPosition = position;
        _hasTargetPosition = true;
    }

    private Vector3 GetBasePosition()
    {
        return GetMovePosition() - GetZoomOffset();
    }

    private Vector3 GetMovePosition()
    {
        if (ShouldUseMoveWithMouseOffset())
            return new Vector3(moveWithMouse.moveOffset.x, moveWithMouse.moveOffset.y, 0f);

        return useLocalPosition ? moveTransform.localPosition : moveTransform.position;
    }

    private void SetMovePosition(Vector3 position)
    {
        if (ShouldUseMoveWithMouseOffset())
        {
            moveWithMouse.moveOffset = new Vector2(position.x, position.y);
            return;
        }

        if (useLocalPosition)
            moveTransform.localPosition = position;
        else
            moveTransform.position = position;
    }

    private Vector3 GetZoomOffset()
    {
        if (!ShouldCompensateForZoom())
            return Vector3.zero;

        Vector3 worldOffset = zoomCamera.CurrentWorldOffset;

        if (!useLocalPosition || moveTransform.parent == null)
            return worldOffset;

        return moveTransform.parent.InverseTransformVector(worldOffset);
    }

    private bool ShouldCompensateForZoom()
    {
        return !ShouldUseMoveWithMouseOffset() &&
               zoomCamera != null &&
               zoomCamera.ZoomTransform == moveTransform;
    }

    private bool ShouldUseMoveWithMouseOffset()
    {
        return moveWithMouse != null && moveWithMouse.transform == moveTransform;
    }

    private bool IsCameraInputAllowed()
    {
        return GameStateManager.Instance != null &&
               GameStateManager.Instance.IsCurrent<PlayingState>();
    }

    private Vector2 GetMoveInput()
    {
        Vector2 input = Vector2.zero;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            input.x -= 1f;

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            input.x += 1f;

        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            input.y -= 1f;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            input.y += 1f;

        return input;
    }

    private void ClampTargetPosition()
    {
        if (!useMoveBounds)
            return;

        Vector2 effectiveMinBounds = minBounds + boundsOffset;
        Vector2 effectiveMaxBounds = maxBounds + boundsOffset;

        _targetPosition.x = Mathf.Clamp(_targetPosition.x, effectiveMinBounds.x, effectiveMaxBounds.x);
        _targetPosition.y = Mathf.Clamp(_targetPosition.y, effectiveMinBounds.y, effectiveMaxBounds.y);
    }

    private void OnValidate()
    {
        if (maxBounds.x < minBounds.x)
            maxBounds.x = minBounds.x;

        if (maxBounds.y < minBounds.y)
            maxBounds.y = minBounds.y;

        if (smoothTime < 0f)
            smoothTime = 0f;
    }
}
