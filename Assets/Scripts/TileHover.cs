using StateManager;
using UnityEngine;
using Util;

public class TileHover : MonoBehaviour
{
    public LerpPosition lerpPosition;
    public GameObject activateOnHover;
    public int waitTicks = 20;
    public float hoverYOffset = -0.07f;

    private int ticksHovered = 0;
    public bool activeHover = true;

    public bool hoverWhenNotPlaytate = true;
    public bool ignoreOcclusion = false;

    public GameObject sideThing;

    private Collider col3D;
    private Collider2D col2D;
    private Camera mainCam;

    void Awake()
    {
        col3D = GetComponent<Collider>();
        col2D = GetComponent<Collider2D>();
        mainCam = Camera.main;
    }

    void FixedUpdate()
    {
        if (mainCam == null)
        {
            mainCam = Camera.main;
            if (mainCam == null) return;
        }

        if (!hoverWhenNotPlaytate && !GameStateManager.Instance.IsCurrent<PlayingState>())
        {
            ResetHoverState();
            return;
        }

        bool isHovering = IsMouseHovering();

        if (isHovering && activeHover)
        {
            if (lerpPosition)
            {
                lerpPosition.targetLocation = new Vector3(0, hoverYOffset, lerpPosition.startPosition.z);
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

    private bool IsMouseHovering()
    {
        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);

        // 3D collider support
        if (col3D != null)
        {
            if (ignoreOcclusion)
            {
                return col3D.Raycast(ray, out _, Mathf.Infinity);
            }
            else
            {
                if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
                {
                    return hit.collider == col3D;
                }
            }

            return false;
        }

        // 2D collider support
        if (col2D != null)
        {
            Vector2 mouseWorldPoint = mainCam.ScreenToWorldPoint(Input.mousePosition);

            if (ignoreOcclusion)
            {
                return col2D.OverlapPoint(mouseWorldPoint);
            }
            else
            {
                RaycastHit2D hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity);
                return hit.collider == col2D;
            }
        }

        return false;
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