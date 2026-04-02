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

    public GameObject sideThing;

    private Collider2D col;

    void Awake()
    {
        col = GetComponent<Collider2D>();
    }

    void FixedUpdate()
    {
        // Handle game state restriction
        if (!hoverWhenNotPlaytate && !GameStateManager.Instance.IsCurrent<PlayingState>())
        {
            ResetHoverState();
            return;
        }

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePos2D = new Vector2(mouseWorldPos.x, mouseWorldPos.y);

        // 🔥 Raycast straight into screen (2D)
        RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);

        bool isTopMost = hit.collider != null && hit.collider == col;

        if (isTopMost && activeHover)
        {
            if (lerpPosition)
            {
                lerpPosition.targetLocation = new Vector3(0, -0.04f, lerpPosition.startPosition.z);
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