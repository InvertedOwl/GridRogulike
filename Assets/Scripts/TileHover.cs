using StateManager;
using UnityEngine;
using Util;

public class TileHover : MonoBehaviour
{
    public LerpPosition lerpPosition;
    public GameObject activateOnHover;
    public int waitTicks = 20;

    private int ticksHovered = 0;
    public bool activeHover = true;

    public bool hoverWhenNotPlaytate = true;

    public bool ignoreOcclusion = false;

    public GameObject sideThing;

    private Collider col;
    private Camera mainCam;

    void Awake()
    {
        col = GetComponent<Collider>();
        mainCam = Camera.main;
    }

    void FixedUpdate()
    {
        if (mainCam == null)
        {
            mainCam = Camera.main;
            if (mainCam == null) return;
        }

        // Handle game state restriction
        if (!hoverWhenNotPlaytate && !GameStateManager.Instance.IsCurrent<PlayingState>())
        {
            ResetHoverState();
            return;
        }

        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
        bool isHovering = false;

        if (ignoreOcclusion)
        {
            // Check whether the ray intersects this collider, regardless of what is in front
            isHovering = col.Raycast(ray, out _, Mathf.Infinity);
        }
        else
        {
            // Check whether this object is the first thing hit by the ray
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
            {
                isHovering = hit.collider == col;
            }
        }

        if (isHovering && activeHover)
        {
            if (lerpPosition)
            {
                lerpPosition.targetLocation = new Vector3(0, -0.07f, lerpPosition.startPosition.z);
            }

            if (activateOnHover && ticksHovered > waitTicks)
            {
                activateOnHover.SetActive(true);
            }

            if (sideThing)
            {
                sideThing.SetActive(false);
            }

            ticksHovered++;
        }
        else
        {
            ResetHoverState();
        }
    }

    private void ResetHoverState()
    {
        if (lerpPosition)
        {
            lerpPosition.targetLocation = new Vector3(0, 0, lerpPosition.startPosition.z);
        }

        if (activateOnHover)
        {
            activateOnHover.SetActive(false);
        }

        if (sideThing)
        {
            sideThing.SetActive(true);
        }

        ticksHovered = 0;
    }
}