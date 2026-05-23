using Entities;
using Grid;
using StateManager;
using System.Collections.Generic;
using UnityEngine;

public class AutoCamera : MonoBehaviour
{
    public MoveWithMouse moveWithMouse;
    public Transform moveTransform;
    public ZoomCamera zoomCamera;
    public Camera targetCamera;

    [Header("Position")]
    public float worldToOffsetScale = 0.5f;
    public Vector2 focusWorldOffset = new Vector2(0f, -1f);
    public Vector2 positionOffset = Vector2.zero;
    public bool useLocalPosition = true;
    public float positionEaseSpeed = 8f;

    [Header("Zoom")]
    public float viewportPadding = 0.08f;
    public float targetViewportCoverage = 0.65f;
    public int zoomSearchSteps = 12;

    private void Awake()
    {
        ResolveReferences();
    }

    private void LateUpdate()
    {
        ResolveReferences();

        if (!ShouldAutoCamera())
            return;

        if (!TryGetCameraTargets(out List<Vector3> targetPoints, out Vector2 min, out Vector2 max))
            return;

        Vector2 center = (min + max) * 0.5f + focusWorldOffset;

        ApplyPosition(center);
        ApplyZoom(targetPoints);
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

        if (zoomCamera == null)
        {
            zoomCamera = GetComponent<ZoomCamera>();

            if (zoomCamera == null && moveTransform != null)
                zoomCamera = moveTransform.GetComponent<ZoomCamera>();

            if (zoomCamera == null)
                zoomCamera = GetComponentInChildren<ZoomCamera>();

            if (zoomCamera == null && moveTransform != null && moveTransform != transform)
                zoomCamera = moveTransform.GetComponentInChildren<ZoomCamera>();
        }

        if (targetCamera == null)
        {
            if (zoomCamera != null && zoomCamera.targetCamera != null)
                targetCamera = zoomCamera.targetCamera;

            if (targetCamera == null)
                targetCamera = Camera.main;

            if (targetCamera == null)
                targetCamera = GetComponentInChildren<Camera>();
        }
    }

    private bool ShouldAutoCamera()
    {
        return GameplayNavSettings.autocamera &&
               GameStateManager.Instance != null &&
               GameStateManager.Instance.IsCurrent<PlayingState>();
    }

    private bool TryGetCameraTargets(out List<Vector3> targetPoints, out Vector2 min, out Vector2 max)
    {
        targetPoints = new List<Vector3>();
        min = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
        max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);

        PlayingState playingState = GameStateManager.Instance.GetCurrent<PlayingState>();
        bool hasTarget = false;

        foreach (AbstractEntity entity in playingState.GetEntities())
        {
            if (!ShouldIncludeEntity(entity))
                continue;

            Vector3 entityPosition = GetEntityPosition(entity);
            targetPoints.Add(entityPosition);
            min = Vector2.Min(min, entityPosition);
            max = Vector2.Max(max, entityPosition);
            hasTarget = true;
        }

        return hasTarget;
    }

    private bool ShouldIncludeEntity(AbstractEntity entity)
    {
        if (entity == null)
            return false;

        return entity.entityType == EntityType.Player || entity.Health > 0f;
    }

    private Vector3 GetEntityPosition(AbstractEntity entity)
    {
        if (HexGridManager.Instance != null &&
            HexGridManager.Instance._hexObjects.TryGetValue(entity.positionRowCol, out GameObject hexObject) &&
            hexObject != null)
        {
            return hexObject.transform.position;
        }

        return entity.transform.position;
    }

    private void ApplyPosition(Vector2 focusPosition)
    {
        Vector2 targetOffset = focusPosition * worldToOffsetScale + positionOffset;

        if (moveWithMouse != null)
        {
            Vector2 targetCameraOffset = targetOffset - moveWithMouse.moveOffset;
            moveWithMouse.offset = Vector2.Lerp(
                moveWithMouse.offset,
                targetCameraOffset,
                GetEaseT(positionEaseSpeed)
            );
            return;
        }

        if (moveTransform == null)
            return;

        Vector3 currentPosition = useLocalPosition ? moveTransform.localPosition : moveTransform.position;
        Vector3 targetPosition = new Vector3(targetOffset.x, targetOffset.y, currentPosition.z);
        Vector3 nextPosition = Vector3.Lerp(currentPosition, targetPosition, GetEaseT(positionEaseSpeed));

        if (useLocalPosition)
            moveTransform.localPosition = nextPosition;
        else
            moveTransform.position = nextPosition;
    }

    private void ApplyZoom(List<Vector3> targetPoints)
    {
        if (zoomCamera == null || targetCamera == null || targetPoints == null || targetPoints.Count == 0)
            return;

        float minDistance = zoomCamera.minMoveDistance;
        float maxDistance = zoomCamera.maxMoveDistance;

        if (Mathf.Approximately(minDistance, maxDistance))
        {
            zoomCamera.SetZoomDistance(minDistance);
            return;
        }

        float zoomedInDistance = GetMoreZoomedInDistance(targetPoints, minDistance, maxDistance);
        float zoomedOutDistance = Mathf.Approximately(zoomedInDistance, minDistance)
            ? maxDistance
            : minDistance;

        float desiredCoverage = Mathf.Clamp(
            targetViewportCoverage,
            0.05f,
            Mathf.Max(0.05f, 1f - Mathf.Clamp01(viewportPadding) * 2f)
        );
        float zoomedInCoverage = GetViewportBoundsMaxDimensionAtDistance(targetPoints, zoomedInDistance);
        float zoomedOutCoverage = GetViewportBoundsMaxDimensionAtDistance(targetPoints, zoomedOutDistance);

        if (zoomedInCoverage <= desiredCoverage)
        {
            zoomCamera.SetZoomDistance(zoomedInDistance);
            return;
        }

        if (zoomedOutCoverage >= desiredCoverage)
        {
            zoomCamera.SetZoomDistance(zoomedOutDistance);
            return;
        }

        float low = 0f;  // zoomed in
        float high = 1f; // zoomed out

        for (int i = 0; i < Mathf.Max(1, zoomSearchSteps); i++)
        {
            float middle = (low + high) * 0.5f;
            float candidateDistance = Mathf.Lerp(zoomedInDistance, zoomedOutDistance, middle);
            float candidateCoverage = GetViewportBoundsMaxDimensionAtDistance(targetPoints, candidateDistance);

            if (candidateCoverage > desiredCoverage)
                low = middle;
            else
                high = middle;
        }

        zoomCamera.SetZoomDistance(Mathf.Lerp(zoomedInDistance, zoomedOutDistance, high));
    }

    private float GetMoreZoomedInDistance(List<Vector3> targetPoints, float firstDistance, float secondDistance)
    {
        float firstViewportSize = GetViewportBoundsMaxDimensionAtDistance(targetPoints, firstDistance);
        float secondViewportSize = GetViewportBoundsMaxDimensionAtDistance(targetPoints, secondDistance);

        return firstViewportSize >= secondViewportSize ? firstDistance : secondDistance;
    }

    private float GetViewportBoundsMaxDimensionAtDistance(List<Vector3> targetPoints, float moveDistance)
    {
        Vector3 originalPosition = targetCamera.transform.position;
        targetCamera.transform.position = GetCameraPositionAtDistance(moveDistance);

        Vector2 min = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
        Vector2 max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);

        foreach (Vector3 targetPoint in targetPoints)
        {
            Vector3 viewportPoint = targetCamera.WorldToViewportPoint(targetPoint);
            min = Vector2.Min(min, viewportPoint);
            max = Vector2.Max(max, viewportPoint);
        }

        targetCamera.transform.position = originalPosition;
        Vector2 size = max - min;
        return Mathf.Max(Mathf.Abs(size.x), Mathf.Abs(size.y));
    }

    private Vector3 GetCameraPositionAtDistance(float moveDistance)
    {
        if (zoomCamera.ZoomTransform == null)
            return targetCamera.transform.position;

        return targetCamera.transform.position + zoomCamera.ZoomDirection * (moveDistance - zoomCamera.CurrentMoveDistance);
    }

    private float GetEaseT(float easeSpeed)
    {
        if (easeSpeed <= 0f)
            return 1f;

        return 1f - Mathf.Exp(-easeSpeed * Time.deltaTime);
    }
}
