using Grid;
using UnityEngine;

namespace Util
{
    public class SpriteArrowController : MonoBehaviour
    {
        public float heightScale = 1; 
        public LineRenderer lineRenderer;
        public Transform iconContainer;
        public Transform arrowHead;
        public SpriteRenderer arrowHeadRenderer;
        public float startPadding = 0.75f;
        public float endPadding = 0.75f;
        public float arrowHeadOffset = 0f;
        public float arrowHeadAngleOffset = -90f;
        public float iconHeadOffset = 0f;
        public float zPosition = 0.9601f;

        public Vector2Int Tail
        {
            get {return _tail;}
            set
            {
                _tail = value;
                SetArrowPos(Tail, head);
            }
        }
        private Vector2Int _tail;
        public Vector2Int head;

        private void Awake()
        {
            CacheReferences();
        }

        public void Update()
        {
            SetArrowPos(_tail, head);
        }

        public void SetColor(Color color)
        {
            CacheReferences();

            color.a = 1f;
            if (lineRenderer != null)
            {
                lineRenderer.startColor = color * lineRenderer.startColor;
                lineRenderer.endColor = color * lineRenderer.endColor;
            }

            SetArrowHeadColor();
        }

        private void SetArrowPos(Vector2Int tail, Vector2Int head)
        {
            Vector2 tailWorldPos = HexGridManager.GetHexCenter(tail.x, tail.y);
            Vector2 headWorldPos = HexGridManager.GetHexCenter(head.x, head.y);
            Vector2 arrowWorldPos2d = (tailWorldPos + headWorldPos)/2.0f;

            float distance = Vector2.Distance(tailWorldPos, headWorldPos);
            float scaledDistance = distance * heightScale;
            float lineLength = Mathf.Max(0f, scaledDistance - startPadding - endPadding);
            Vector2 direction = distance > 0f ? (headWorldPos - tailWorldPos) / distance : Vector2.up;
            Vector3 centerShift = new Vector3(direction.x, direction.y, 0f) * ((startPadding - endPadding) / 2f);
            Vector3 offset = new Vector3(direction.x, direction.y, 0f) * (lineLength / 2f);
            Vector3 center = new Vector3(arrowWorldPos2d.x, arrowWorldPos2d.y, zPosition) + centerShift;
            Vector3 lineEnd = center + offset;

            SetIconPosition(lineEnd, direction);

            if (lineRenderer == null)
                return;

            lineRenderer.positionCount = 2;
            if (lineRenderer.useWorldSpace)
            {
                lineRenderer.SetPosition(0, center - offset);
                lineRenderer.SetPosition(1, center + offset);
            }
            else
            {
                lineRenderer.transform.position = center;
                lineRenderer.SetPosition(0, -offset);
                lineRenderer.SetPosition(1, offset);
            }

            SetArrowHeadPosition(lineEnd, direction);
            SetArrowHeadColor();
        }

        private void CacheReferences()
        {
            if (lineRenderer == null)
                lineRenderer = GetComponent<LineRenderer>() ?? GetComponentInChildren<LineRenderer>();

            if (iconContainer == null)
                iconContainer = FindIconContainer();

            if (arrowHead == null)
                arrowHead = FindArrowHead();

            if (arrowHeadRenderer == null && arrowHead != null)
                arrowHeadRenderer = arrowHead.GetComponent<SpriteRenderer>();
        }

        private void SetArrowHeadPosition(Vector3 lineEnd, Vector2 direction)
        {
            if (arrowHead == null)
                return;

            Vector3 direction3 = new Vector3(direction.x, direction.y, 0f);
            arrowHead.position = lineEnd + direction3 * arrowHeadOffset;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            arrowHead.rotation = Quaternion.Euler(0f, 0f, angle + arrowHeadAngleOffset);
        }

        private void SetIconPosition(Vector3 lineEnd, Vector2 direction)
        {
            if (iconContainer == null)
                return;

            Vector3 direction3 = new Vector3(direction.x, direction.y, 0f);
            iconContainer.position = lineEnd + direction3 * iconHeadOffset;
        }

        private void SetArrowHeadColor()
        {
            if (arrowHeadRenderer == null || lineRenderer == null)
                return;

            arrowHeadRenderer.color = lineRenderer.endColor;
        }

        private Transform FindIconContainer()
        {
            Transform found = transform.Find("IconList");
            if (found != null)
                return found;

            return transform.parent != null ? transform.parent.Find("IconList") : null;
        }

        private Transform FindArrowHead()
        {
            Transform found = transform.Find("ArrowHead");
            if (found != null)
                return found;

            return transform.parent != null ? transform.parent.Find("ArrowHead") : null;
        }
    }
}
