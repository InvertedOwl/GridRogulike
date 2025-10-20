using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarManager : MonoBehaviour
{
    
    public TextMeshPro healthText;
    public TextMeshProUGUI healthTextUI;
    public TextMeshProUGUI shieldTextUI;
    public GameObject healthBar;
    public GameObject shieldBar;
    public float initialHealth;
    public float health;
    public float shield;

    void Update()
    {
        if (healthBar.TryGetComponent<RectTransform>(out var healthBarRect))
        {
            float healthRatio = health / initialHealth;
            float fullWidth = 132.1f;
                
            float shieldRatio = shield / initialHealth;

            healthBarRect.sizeDelta = new Vector2(healthRatio * fullWidth, healthBarRect.sizeDelta.y);
            healthBar.GetComponent<Image>().color = LerpLinearRGB(Color.red, Color.green, healthRatio);
            RectTransform shieldBarRect = shieldBar.GetComponent<RectTransform>();
            shieldBarRect.sizeDelta =  new Vector2(shieldRatio * fullWidth, shieldBarRect.sizeDelta.y);
            healthTextUI.text = health + "/" + initialHealth;
            shieldTextUI.text = shield + "";
                
        }
        else
        {
            healthText.text = health + "/" + initialHealth;
            Transform healthtransform = healthBar.transform;
            float healthRatio = health / initialHealth;
            float fullWidth = 0.0158992f;

            healthtransform.localScale = new Vector2(healthRatio * fullWidth, healthtransform.localScale.y);
            healthBar.GetComponent<SpriteRenderer>().color = LerpLinearRGB(Color.red, Color.green, healthRatio);
                
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
