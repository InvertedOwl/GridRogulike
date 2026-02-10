using UnityEngine;
using UnityEngine.EventSystems;

namespace Grid
{
    public class HexHoverForwarder : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public Vector2Int GridPos { get; private set; }

        private HexGridManager _manager;

        public void Init(HexGridManager manager, Vector2Int gridPos)
        {
            _manager = manager;
            GridPos = gridPos;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _manager?.NotifyHexHoverEnter(GridPos, gameObject);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _manager?.NotifyHexHoverExit(GridPos, gameObject);
        }
    }
}