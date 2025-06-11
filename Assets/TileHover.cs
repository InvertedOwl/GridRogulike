using UnityEngine;
using UnityEngine.Serialization;
using Util;

public class TileHover : MonoBehaviour
{
    public LerpPosition lerpPosition;
    public GameObject activateOnHover;
    public int waitTicks = 20;
    
    private int ticksHovered = 0;
    public bool activeHover = true;
    
    void FixedUpdate() {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePos2D = new Vector2(mouseWorldPos.x, mouseWorldPos.y);

        if (GetComponent<Collider2D>().OverlapPoint(mousePos2D) && activeHover) {
            lerpPosition.targetLocation = new Vector2(0, -0.02f);
            if (activateOnHover && ticksHovered > waitTicks)
            {
                activateOnHover.SetActive(true);
            }
            ticksHovered++;
        }
        else
        {
            lerpPosition.targetLocation = new Vector2(0, 0);
            if (activateOnHover)
            {
                activateOnHover.SetActive(false);
            }
            ticksHovered = 0;
        }
    }
}
