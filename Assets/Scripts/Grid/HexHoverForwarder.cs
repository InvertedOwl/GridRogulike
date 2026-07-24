using UnityEngine;
using UnityEngine.EventSystems;
using Util;

namespace Grid
{
    public class HexHoverForwarder : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public Vector2Int GridPos { get; private set; }

        private HexGridManager _manager;
        private bool _isPointerOver;

        public void Init(HexGridManager manager, Vector2Int gridPos)
        {
            _manager = manager;
            GridPos = gridPos;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (HoverStateInvalidator.IsSuppressed)
                return;

            _isPointerOver = true;
            _manager?.NotifyHexHoverEnter(GridPos, gameObject);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!_isPointerOver)
                return;

            _isPointerOver = false;
            _manager?.NotifyHexHoverExit(GridPos, gameObject);
        }

        public void ForcePointerExit()
        {
            if (!_isPointerOver)
                return;

            _isPointerOver = false;
            _manager?.NotifyHexHoverExit(GridPos, gameObject);
        }
    }
}
