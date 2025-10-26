using System;
using Grid;
using UnityEditor;
using UnityEngine;

namespace Util
{
    public class SpriteArrowController : MonoBehaviour
    {
        public float heightScale = 1; 
        public LerpHeight lerpHeight;
        public LerpPosition lerpPosition;
        public LerpPosition lerpIcon;

        public Vector2Int Tail
        {
            get {return _tail;}
            set
            {
                lerpPosition.targetLocation = HexGridManager.GetHexCenter(value.x, value.y);
                transform.localPosition = HexGridManager.GetHexCenter(value.x, value.y);
                _tail = value;
                SetArrowPos(Tail, head);
            }
        }
        private Vector2Int _tail;
        public Vector2Int head;

        public void Update()
        {
            SetArrowPos(_tail, head);
        }

        private void SetArrowPos(Vector2Int tail, Vector2Int head)
        {
            Vector2 tailWorldPos = HexGridManager.GetHexCenter(tail.x, tail.y);
            Vector2 headWorldPos = HexGridManager.GetHexCenter(head.x, head.y);
            Vector2 arrowWorldPos = (tailWorldPos + headWorldPos)/2.0f;
            float width = headWorldPos.x - tailWorldPos.x;
            float height = headWorldPos.y - tailWorldPos.y;
            float arrowAngle = Mathf.Atan2(height, width);
            
            lerpPosition.targetLocation = arrowWorldPos;
            transform.localPosition = arrowWorldPos;
            lerpPosition.targetRotation = Quaternion.Euler(new Vector3(0, 0, Mathf.Rad2Deg * arrowAngle - 90));
            lerpIcon.targetLocation = arrowWorldPos;
            lerpIcon.transform.localPosition = arrowWorldPos;
            lerpHeight.targetHeight = (float) Math.Sqrt((height * height) + (width * width)) * heightScale;
        }
    }
}
