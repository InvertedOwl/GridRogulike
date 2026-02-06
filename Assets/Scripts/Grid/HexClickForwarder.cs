using UnityEngine;
using UnityEngine.EventSystems;

namespace Grid
{
    public class HexClickForwarder : MonoBehaviour, IPointerClickHandler
    {
        public Vector2Int GridPos { get; private set; }

        private HexGridManager _manager;

        public void Init(HexGridManager manager, Vector2Int gridPos)
        {
            _manager = manager;
            GridPos = gridPos;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            _manager?.NotifyHexClicked(GridPos, gameObject);
        }
    }
}