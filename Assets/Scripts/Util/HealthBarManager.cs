using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarManager : MonoBehaviour
{
    
    public TextMeshPro healthText;
    public TextMeshPro shieldText;
    public TextMeshProUGUI healthTextUI;
    public TextMeshProUGUI shieldTextUI;
    public GameObject baseBar;
    public GameObject healthBar;
    public GameObject shieldBar;
    public float initialHealth;
    public float health;
    public float shield;
    
    public bool scaleBarWithMaxHealth = true;
public float uiWidthPerHealth = 1.321f;
public float worldWidthPerHealth = 0.0158992f;
public float fixedWorldReferenceHealth = 100f;

private bool _cachedBarSizes = false;

// UI cached full sizes
private float _startingUiHealthWidth;
private float _startingUiShieldWidth;

// World cached full sizes
private Vector2 _startingHealthSpriteSize;
private Vector2 _startingShieldSpriteSize;
private Vector3 _startingHealthScale;
private Vector3 _startingShieldScale;

void CacheBarSizes()
{
    if (_cachedBarSizes) return;

    // UI sizes
    if (healthBar != null && healthBar.TryGetComponent<RectTransform>(out var healthRect))
    {
        _startingUiHealthWidth = healthRect.sizeDelta.x;
    }

    if (shieldBar != null && shieldBar.TryGetComponent<RectTransform>(out var shieldRect))
    {
        _startingUiShieldWidth = shieldRect.sizeDelta.x;
    }
    else
    {
        _startingUiShieldWidth = _startingUiHealthWidth;
    }

    // World sizes/scales
    if (healthBar != null)
    {
        var healthRenderer = healthBar.GetComponent<SpriteRenderer>();
        if (healthRenderer != null)
            _startingHealthSpriteSize = healthRenderer.size;

        _startingHealthScale = healthBar.transform.localScale;
    }

    if (shieldBar != null)
    {
        var shieldRenderer = shieldBar.GetComponent<SpriteRenderer>();
        if (shieldRenderer != null)
            _startingShieldSpriteSize = shieldRenderer.size;

        _startingShieldScale = shieldBar.transform.localScale;
    }

    _cachedBarSizes = true;
}

void Update()
{
    CacheBarSizes();

    float safeMaxHealth = Mathf.Max(1f, initialHealth);
    float healthRatio = Mathf.Clamp01((float)health / safeMaxHealth);
    float shieldRatio = Mathf.Clamp01((float)shield / safeMaxHealth);

    if (healthTextUI != null)
        healthTextUI.text = health + "/" + initialHealth;
    if (shieldTextUI != null)
        shieldTextUI.text = shield.ToString();

    if (healthText != null)
        healthText.text = health + "/" + initialHealth;
    if (shieldText != null)
        shieldText.text = shield.ToString();

    // ===================== UI VERSION =====================
    if (healthBar != null && healthBar.TryGetComponent<RectTransform>(out var healthBarRect))
    {
        float fullHealthWidth = scaleBarWithMaxHealth
            ? safeMaxHealth * uiWidthPerHealth
            : _startingUiHealthWidth;

        fullHealthWidth = Mathf.Min(fullHealthWidth, _startingUiHealthWidth);

        healthBarRect.sizeDelta = new Vector2(
            healthRatio * fullHealthWidth,
            healthBarRect.sizeDelta.y
        );

        var healthImage = healthBar.GetComponent<Image>();
        if (healthImage != null)
        {
            healthImage.color = LerpLinearRGB(
                new Color(0.8980f, 0.4863f, 0.4863f),
                new Color(0.4863f, 0.8980f, 0.5333f),
                healthRatio
            );
        }

        if (shieldBar != null && shieldBar.TryGetComponent<RectTransform>(out var shieldRect))
        {
            float fullShieldWidth = scaleBarWithMaxHealth
                ? safeMaxHealth * uiWidthPerHealth
                : _startingUiShieldWidth;

            fullShieldWidth = Mathf.Min(fullShieldWidth, _startingUiShieldWidth);

            shieldRect.sizeDelta = new Vector2(
                shieldRatio * fullShieldWidth,
                shieldRect.sizeDelta.y
            );
        }

        return;
    }

    // ===================== WORLD / SPRITE VERSION =====================
    var healthSprite = healthBar != null ? healthBar.GetComponent<SpriteRenderer>() : null;
    var shieldSprite = shieldBar != null ? shieldBar.GetComponent<SpriteRenderer>() : null;
    var baseSprite = baseBar != null ? baseBar.GetComponent<SpriteRenderer>() : null;

    if (healthSprite != null)
    {
        float referenceHealth = scaleBarWithMaxHealth ? safeMaxHealth : fixedWorldReferenceHealth;

        float targetHealthWidth = scaleBarWithMaxHealth
            ? referenceHealth * worldWidthPerHealth
            : _startingHealthSpriteSize.x;

        // never exceed original full width
        targetHealthWidth = Mathf.Min(targetHealthWidth, _startingHealthSpriteSize.x);

        // keep original detected height
        float targetHealthHeight = _startingHealthSpriteSize.y;

        healthSprite.size = new Vector2(
            healthRatio * targetHealthWidth,
            targetHealthHeight
        );

        healthSprite.color = LerpLinearRGB(Color.red, Color.green, healthRatio);
    }
    
    if (shieldSprite != null)
    {
        float referenceHealth = scaleBarWithMaxHealth ? safeMaxHealth : fixedWorldReferenceHealth;

        float targetShieldWidth = scaleBarWithMaxHealth
            ? referenceHealth * worldWidthPerHealth
            : _startingShieldSpriteSize.x;

        // never exceed original full width
        targetShieldWidth = Mathf.Min(targetShieldWidth, _startingShieldSpriteSize.x);

        // keep original detected height
        float targetShieldHeight = _startingShieldSpriteSize.y;

        shieldSprite.size = new Vector2(
            shieldRatio * targetShieldWidth,
            targetShieldHeight
        );

        shieldSprite.color = new Color(0.4f, 0.7f, 1f);
    }
}
    public static Color LerpLinearRGB(Color a, Color b, float t)
    {
        Color la = a.linear;
        Color lb = b.linear;

        Color lc = Color.LerpUnclamped(la, lb, t);
        return lc.gamma;
    }
}
