using TMPro;
using UnityEngine;

public class HealthBarManager : MonoBehaviour
{
    [Header("World Text")]
    public TMP_Text healthText;
    public TMP_Text shieldText;

    [Header("World Bars")]
    public GameObject baseBar;
    public GameObject healthBar;
    public GameObject shieldBar;

    [Header("Stats")]
    public float initialHealth = 100f;
    public float health = 100f;
    public float shield = 0f;

    [Header("Bar Scaling")]
    public bool scaleBarWithMaxHealth = true;
    public float worldWidthPerHealth = 0.0158992f;
    public float fixedWorldReferenceHealth = 100f;

    private bool _cachedBarSizes = false;
    private bool _cachedText = false;
    private Vector2 _startingHealthSpriteSize;
    private Vector2 _startingShieldSpriteSize;

    private void CacheText()
    {
        if (_cachedText) return;

        TMP_Text[] textComponents = GetComponentsInChildren<TMP_Text>(true);

        if (healthText == null)
            healthText = FindTextComponent(textComponents, "HealthText");

        if (healthText == null && textComponents.Length == 1)
            healthText = textComponents[0];

        if (shieldText == null)
            shieldText = FindTextComponent(textComponents, "ShieldText");

        _cachedText = true;
    }

    private TMP_Text FindTextComponent(TMP_Text[] textComponents, string objectName)
    {
        string normalizedObjectName = NormalizeTextObjectName(objectName);

        foreach (TMP_Text textComponent in textComponents)
        {
            if (NormalizeTextObjectName(textComponent.gameObject.name) == normalizedObjectName)
                return textComponent;
        }

        return null;
    }

    private string NormalizeTextObjectName(string objectName)
    {
        return objectName
            .Replace(" ", "")
            .Replace("_", "")
            .Replace("-", "")
            .ToLowerInvariant();
    }

    public void SetValues(float currentHealth, float maxHealth, float currentShield)
    {
        health = currentHealth;
        initialHealth = maxHealth;
        shield = currentShield;
        Refresh();
    }

    private void CacheBarSizes()
    {
        if (_cachedBarSizes) return;

        if (healthBar != null)
        {
            SpriteRenderer healthRenderer = healthBar.GetComponent<SpriteRenderer>();
            if (healthRenderer != null)
            {
                _startingHealthSpriteSize = healthRenderer.size;
            }
        }

        if (shieldBar != null)
        {
            SpriteRenderer shieldRenderer = shieldBar.GetComponent<SpriteRenderer>();
            if (shieldRenderer != null)
            {
                _startingShieldSpriteSize = shieldRenderer.size;
            }
        }
        else
        {
            _startingShieldSpriteSize = _startingHealthSpriteSize;
        }

        _cachedBarSizes = true;
    }

    private void Update()
    {
        Refresh();
    }

    private void Refresh()
    {
        CacheText();
        CacheBarSizes();

        float safeMaxHealth = Mathf.Max(1f, initialHealth);
        float healthRatio = Mathf.Clamp01(health / safeMaxHealth);
        float shieldRatio = Mathf.Clamp01(shield / safeMaxHealth);

        if (healthText != null)
            healthText.SetText("{0}/{1}", Mathf.CeilToInt(Mathf.Max(0f, health)), Mathf.CeilToInt(safeMaxHealth));

        if (shieldText != null)
            shieldText.SetText("{0}", Mathf.CeilToInt(Mathf.Max(0f, shield)));

        SpriteRenderer healthSprite = healthBar != null ? healthBar.GetComponent<SpriteRenderer>() : null;
        SpriteRenderer shieldSprite = shieldBar != null ? shieldBar.GetComponent<SpriteRenderer>() : null;

        if (healthSprite != null)
        {
            float fullHealthWidth = scaleBarWithMaxHealth
                ? safeMaxHealth * worldWidthPerHealth
                : _startingHealthSpriteSize.x;

            fullHealthWidth = Mathf.Min(fullHealthWidth, _startingHealthSpriteSize.x);

            healthSprite.size = new Vector2(
                healthRatio * fullHealthWidth,
                _startingHealthSpriteSize.y
            );

            healthSprite.color = LerpLinearRGB(
                new Color(0.8980f, 0.4863f, 0.4863f),
                new Color(0.4863f, 0.8980f, 0.5333f),
                healthRatio
            );
        }

        if (shieldSprite != null)
        {
            float fullShieldWidth = scaleBarWithMaxHealth
                ? safeMaxHealth * worldWidthPerHealth
                : _startingShieldSpriteSize.x;

            fullShieldWidth = Mathf.Min(fullShieldWidth, _startingShieldSpriteSize.x);

            shieldSprite.size = new Vector2(
                shieldRatio * fullShieldWidth,
                _startingShieldSpriteSize.y
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
