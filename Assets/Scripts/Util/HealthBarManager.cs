using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HealthBarManager : MonoBehaviour
{
    [Header("References")]
    public Transform blockContainer; // parent object for the blocks
    public TextMeshProUGUI healthTextUI;
    public TextMeshProUGUI shieldTextUI;
    public TextMeshPro healthText; // for world-space

    [Header("Block Settings")]
    public GameObject blockPrefab; // small square sprite or UI image
    public float hpPerBlock = 25f;
    public float blockWidth = 5f; // width of each block in local units
    public float blockSpacing = 1f; // gap between blocks
    public Color healthColor = new Color(0.95f, 0.95f, 0.95f);    // white-ish like OW
    public Color shieldColor = new Color(0.3f, 0.65f, 1f);         // blue
    public Color armorColor = new Color(1f, 0.75f, 0.25f);         // yellow/orange
    public Color emptyColor = new Color(0.15f, 0.15f, 0.15f, 0.6f); // dark/transparent

    [Header("Stats")]
    public float initialHealth = 200f;
    public float health = 200f;
    public float shield = 0f;

    private List<GameObject> healthBlocks = new List<GameObject>();
    private List<GameObject> shieldBlocks = new List<GameObject>();
    private bool isUI; // auto-detect canvas vs sprite mode

    void Start()
    {
        isUI = blockPrefab.GetComponent<Image>() != null;
        StartCoroutine(WaitFrame());
    }

    IEnumerator WaitFrame()
    {
        yield return new WaitForFixedUpdate();
        Initialize();
    }

    public void Initialize()
    {

        RebuildBar();
    }

    /// <summary>
    /// Call this whenever max health or shield capacity changes.
    /// Destroys old blocks and spawns new ones.
    /// </summary>
    public void RebuildBar()
    {
        int healthBlockCount = Mathf.CeilToInt(initialHealth / hpPerBlock);
        Debug.Log($"RebuildBar called — healthBlockCount: {healthBlockCount}, shield: {shield}");

        foreach (var b in healthBlocks) Destroy(b);
        foreach (var b in shieldBlocks) Destroy(b);
        healthBlocks.Clear();
        shieldBlocks.Clear();

        for (int i = 0; i < healthBlockCount; i++)
        {
            Debug.Log($"Spawning health block {i}");
            GameObject block = Instantiate(blockPrefab, blockContainer);
            Debug.Log($"Spawned: {block.name}, parent: {block.transform.parent?.name}");
            PositionBlock(block, i);
            healthBlocks.Add(block);
        }

        Debug.Log($"Final count — healthBlocks: {healthBlocks.Count}, container children: {blockContainer.childCount}");
        UpdateVisuals();
    }

    void PositionBlock(GameObject block, int index)
    {
        float offset = index * (blockWidth + blockSpacing);
        
        if (isUI)
        {
            RectTransform rt = block.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0.5f);
            rt.anchorMax = new Vector2(0, 0.5f);
            rt.pivot = new Vector2(0, 0.5f);
            rt.anchoredPosition = new Vector2(offset, 0);
            rt.sizeDelta = new Vector2(blockWidth, rt.sizeDelta.y);
        }
        else
        {
            block.transform.localPosition = new Vector3(offset, 0, 0);
            block.transform.localScale = new Vector3(blockWidth, block.transform.localScale.y, 1);
        }
    }

    void Update()
    {
        UpdateVisuals();
    }

    void UpdateVisuals()
    {
        // Update health blocks
        for (int i = 0; i < healthBlocks.Count; i++)
        {
            float blockMinHP = i * hpPerBlock;
            float blockMaxHP = blockMinHP + hpPerBlock;

            Color color;
            float fill;

            if (health >= blockMaxHP)
            {
                // Fully filled
                color = healthColor;
                fill = 1f;
            }
            else if (health > blockMinHP)
            {
                // Partially filled — you could use Image.fillAmount for UI
                color = healthColor;
                fill = (health - blockMinHP) / hpPerBlock;
            }
            else
            {
                // Empty
                color = emptyColor;
                fill = 0f;
            }

            ApplyBlockVisual(healthBlocks[i], color, fill);
        }

        // Update shield blocks
        for (int i = 0; i < shieldBlocks.Count; i++)
        {
            float blockMinHP = i * hpPerBlock;
            float blockMaxHP = blockMinHP + hpPerBlock;

            Color color = shield >= blockMaxHP ? shieldColor :
                          shield > blockMinHP ? shieldColor : emptyColor;
            float fill = shield >= blockMaxHP ? 1f :
                         shield > blockMinHP ? (shield - blockMinHP) / hpPerBlock : 0f;

            ApplyBlockVisual(shieldBlocks[i], color, fill);
        }

        // Update text
        if (healthTextUI != null) healthTextUI.text = $"{health}/{initialHealth}";
        if (shieldTextUI != null) shieldTextUI.text = shield > 0 ? $"+{shield}" : "";
        if (healthText != null) healthText.text = $"{health}/{initialHealth}";
    }

    void ApplyBlockVisual(GameObject block, Color color, float fill)
    {
        if (isUI)
        {
            Image img = block.GetComponent<Image>();
            img.color = color;
            img.fillAmount = fill; // set Image type to "Filled" in inspector
        }
        else
        {
            SpriteRenderer sr = block.GetComponent<SpriteRenderer>();
            // For sprites, lerp between empty and target color based on fill
            sr.color = fill > 0 ? Color.Lerp(emptyColor, color, fill) : emptyColor;
        }
    }
}