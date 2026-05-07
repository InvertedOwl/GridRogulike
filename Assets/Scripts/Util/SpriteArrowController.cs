using Grid;
using UnityEngine;

namespace Util
{
    public class SpriteArrowController : MonoBehaviour
    {
        public float heightScale = 1; 
        public LineRenderer lineRenderer;
        public LineRenderer borderLineRenderer;
        public Transform iconContainer;
        public Transform arrowHead;
        public SpriteRenderer arrowHeadRenderer;
        public Transform arrowHeadBorder;
        public SpriteRenderer arrowHeadBorderRenderer;
        public bool createBorderIfMissing = true;
        public Color borderColor = Color.black;
        public float lineBorderWidthMultiplier = 1.75f;
        public float arrowHeadBorderScaleMultiplier = 1.25f;
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

        public Transform IconContainer
        {
            get
            {
                CacheReferences();
                return iconContainer;
            }
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

            SetLinePosition(borderLineRenderer, center, offset);
            SetLinePosition(lineRenderer, center, offset);

            SetArrowHeadPosition(lineEnd, direction);
            SetBorderColor();
            SetArrowHeadColor();
        }

        private void CacheReferences()
        {
            if (lineRenderer == null)
                lineRenderer = GetComponent<LineRenderer>() ?? GetComponentInChildren<LineRenderer>();

            if (borderLineRenderer == null)
                borderLineRenderer = FindBorderLineRenderer();

            if (iconContainer == null)
                iconContainer = FindIconContainer();

            if (arrowHead == null)
                arrowHead = FindArrowHead();

            if (arrowHeadRenderer == null && arrowHead != null)
                arrowHeadRenderer = arrowHead.GetComponent<SpriteRenderer>();

            if (arrowHeadBorder == null)
                arrowHeadBorder = FindArrowHeadBorder();

            if (arrowHeadBorderRenderer == null && arrowHeadBorder != null)
                arrowHeadBorderRenderer = arrowHeadBorder.GetComponent<SpriteRenderer>();

            if (createBorderIfMissing)
            {
                EnsureBorderLineRenderer();
                EnsureArrowHeadBorder();
            }
        }

        private void SetLinePosition(LineRenderer targetLineRenderer, Vector3 center, Vector3 offset)
        {
            if (targetLineRenderer == null)
                return;

            targetLineRenderer.positionCount = 2;
            if (targetLineRenderer.useWorldSpace)
            {
                targetLineRenderer.SetPosition(0, center - offset);
                targetLineRenderer.SetPosition(1, center + offset);
            }
            else
            {
                targetLineRenderer.transform.position = center;
                targetLineRenderer.SetPosition(0, -offset);
                targetLineRenderer.SetPosition(1, offset);
            }
        }

        private void SetArrowHeadPosition(Vector3 lineEnd, Vector2 direction)
        {
            if (arrowHead == null)
                return;

            Vector3 direction3 = new Vector3(direction.x, direction.y, 0f);
            arrowHead.position = lineEnd + direction3 * arrowHeadOffset;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            arrowHead.rotation = Quaternion.Euler(0f, 0f, angle + arrowHeadAngleOffset);

            if (arrowHeadBorder == null)
                return;

            arrowHeadBorder.position = arrowHead.position;
            arrowHeadBorder.rotation = arrowHead.rotation;
            arrowHeadBorder.localScale = arrowHead.localScale * arrowHeadBorderScaleMultiplier;
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

        private void SetBorderColor()
        {
            if (borderLineRenderer != null)
            {
                borderLineRenderer.startColor = borderColor;
                borderLineRenderer.endColor = borderColor;
            }

            if (arrowHeadBorderRenderer != null)
                arrowHeadBorderRenderer.color = borderColor;
        }

        private LineRenderer FindBorderLineRenderer()
        {
            Transform found = transform.Find("LineBorder");
            if (found == null && transform.parent != null)
                found = transform.parent.Find("LineBorder");

            return found != null ? found.GetComponent<LineRenderer>() : null;
        }

        private void EnsureBorderLineRenderer()
        {
            if (borderLineRenderer != null || lineRenderer == null)
                return;

            GameObject borderLine = new GameObject("LineBorder");
            borderLine.transform.SetParent(lineRenderer.transform.parent, false);
            borderLine.transform.SetSiblingIndex(lineRenderer.transform.GetSiblingIndex());

            borderLineRenderer = borderLine.AddComponent<LineRenderer>();
            borderLineRenderer.sharedMaterial = lineRenderer.sharedMaterial;
            borderLineRenderer.useWorldSpace = lineRenderer.useWorldSpace;
            borderLineRenderer.widthCurve = lineRenderer.widthCurve;
            borderLineRenderer.widthMultiplier = lineRenderer.widthMultiplier * lineBorderWidthMultiplier;
            borderLineRenderer.numCornerVertices = lineRenderer.numCornerVertices;
            borderLineRenderer.numCapVertices = lineRenderer.numCapVertices;
            borderLineRenderer.alignment = lineRenderer.alignment;
            borderLineRenderer.textureMode = lineRenderer.textureMode;
            borderLineRenderer.textureScale = lineRenderer.textureScale;
            borderLineRenderer.sortingLayerID = lineRenderer.sortingLayerID;
            borderLineRenderer.sortingOrder = lineRenderer.sortingOrder - 1;
            SetBorderColor();
        }

        private void EnsureArrowHeadBorder()
        {
            if (arrowHeadBorderRenderer != null || arrowHeadRenderer == null || arrowHead == null)
                return;

            GameObject borderHead = new GameObject("ArrowHeadBorder");
            borderHead.transform.SetParent(arrowHead.parent, false);
            borderHead.transform.SetSiblingIndex(arrowHead.GetSiblingIndex());

            arrowHeadBorder = borderHead.transform;
            arrowHeadBorderRenderer = borderHead.AddComponent<SpriteRenderer>();
            arrowHeadBorderRenderer.sprite = arrowHeadRenderer.sprite;
            arrowHeadBorderRenderer.sharedMaterial = arrowHeadRenderer.sharedMaterial;
            arrowHeadBorderRenderer.sortingLayerID = arrowHeadRenderer.sortingLayerID;
            arrowHeadBorderRenderer.sortingOrder = arrowHeadRenderer.sortingOrder - 1;
            SetBorderColor();
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

        private Transform FindArrowHeadBorder()
        {
            Transform found = transform.Find("ArrowHeadBorder");
            if (found != null)
                return found;

            return transform.parent != null ? transform.parent.Find("ArrowHeadBorder") : null;
        }
    }
}
