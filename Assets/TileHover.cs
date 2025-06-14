using StateManager;
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
    
    public bool hoverWhenNotPlaytate = true;
    
    void FixedUpdate()
    {
        if (!hoverWhenNotPlaytate && !GameStateManager.Instance.IsCurrent<PlayingState>())
        {
            if (lerpPosition)
            {
                lerpPosition.targetLocation = new Vector3(0, 0, lerpPosition.startPosition.z);
            }

            if (activateOnHover)
            {
                activateOnHover.SetActive(false);
            }
            ticksHovered = 0;

            return;
        }
        
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePos2D = new Vector2(mouseWorldPos.x, mouseWorldPos.y);

        if (GetComponent<Collider2D>().OverlapPoint(mousePos2D) && activeHover) {
            if (lerpPosition)
            {
                lerpPosition.targetLocation = new Vector3(0, -0.02f, lerpPosition.startPosition.z);
            }
            
            if (activateOnHover && ticksHovered > waitTicks)
            {
                activateOnHover.SetActive(true);
            }
            ticksHovered++;
        }
        else
        {
            if (lerpPosition)
            {
                lerpPosition.targetLocation = new Vector3(0, 0, lerpPosition.startPosition.z);
            }

            if (activateOnHover)
            {
                activateOnHover.SetActive(false);
            }
            ticksHovered = 0;
        }
    }
}
