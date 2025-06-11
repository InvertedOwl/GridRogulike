using Grid;
using UnityEngine;

namespace Grid
{
    [ExecuteAlways]
    public class HexTileSnapHard : MonoBehaviour
    {
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
            {
                SnapToGrid();
            }
        }

        private void SnapToGrid()
        {

            Vector2 offset = HexGridManager.GetClosestHexCenter(transform.position);
            transform.position = new Vector2(offset.x, offset.y);
        }
    }
}