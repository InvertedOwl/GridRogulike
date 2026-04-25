using Grid;
using UnityEngine;

namespace Util
{
    public class SpriteArrowController : MonoBehaviour
    {
        public float heightScale = 1; 
        public LineRenderer lineRenderer;
        public Transform iconContainer;
        public float endPadding = 0.75f;
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
            if (lineRenderer == null)
                lineRenderer = GetComponent<LineRenderer>() ?? GetComponentInChildren<LineRenderer>();

            if (iconContainer == null)
                iconContainer = FindIconContainer();
        }

        public void Update()
        {
            SetArrowPos(_tail, head);
        }

        public void SetColor(Color color)
        {
            if (lineRenderer == null)
                return;

            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
        }

        private void SetArrowPos(Vector2Int tail, Vector2Int head)
        {
            Vector2 tailWorldPos = HexGridManager.GetHexCenter(tail.x, tail.y);
            Vector2 headWorldPos = HexGridManager.GetHexCenter(head.x, head.y);
            Vector2 arrowWorldPos2d = (tailWorldPos + headWorldPos)/2.0f;
            Vector3 arrowWorldPos = new Vector3(arrowWorldPos2d.x, arrowWorldPos2d.y, zPosition);

            if (iconContainer != null)
            {
                iconContainer.position = arrowWorldPos;
            }

            float distance = Vector2.Distance(tailWorldPos, headWorldPos);
            float lineLength = Mathf.Max(0f, distance * heightScale - endPadding);

            if (lineRenderer == null)
                return;

            Vector2 direction = distance > 0f ? (headWorldPos - tailWorldPos) / distance : Vector2.up;
            Vector3 offset = new Vector3(direction.x, direction.y, 0f) * (lineLength / 2f);
            Vector3 center = new Vector3(arrowWorldPos2d.x, arrowWorldPos2d.y, zPosition);

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
        }

        private Transform FindIconContainer()
        {
            Transform found = transform.Find("IconList");
            if (found != null)
                return found;

            return transform.parent != null ? transform.parent.Find("IconList") : null;
        }
    }
}
