using System.Linq;
using Grid;
using TMPro;
using Types.Statuses;
using UnityEngine.UI;
using Util;

namespace Entities
{
    using UnityEngine;

    public abstract class AbstractEntity : MonoBehaviour
    {
        public float initialHealth;
        public float health;
        public float shield;
        public Vector2Int positionRowCol;
        public TextMeshPro healthText;
        public TextMeshProUGUI healthTextUI;
        public TextMeshProUGUI shieldTextUI;
        public GameObject healthBar;
        public GameObject shieldBar;
        public ParticleSystem hurtSystem;
        public StatusManager statusManager;

        public void MoveEntity(Vector2Int newCoords)
        {
            positionRowCol = newCoords;
            GetComponent<LerpPosition>().targetLocation = HexGridManager.GetHexCenter(positionRowCol.x, positionRowCol.y);
            
        }

        public void Update()
        {
            
            // This is bad. I have added a direct check for the players health bar, BUT
            // and hear me out
            // it works rn.
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
                float fullWidth = 2f;

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
        
        public void StartTurn()
        {
        }
        public void EndTurn()
        {
            if (statusManager)
            {
                foreach (AbstractStatus abstractStatus in statusManager.statusList)
                {
                    abstractStatus.OnEndTurn();
                }
            }
            
        }

        public virtual void Damage(int damage, AbstractStatus abstractStatus)
        {
            Debug.Log("Damaged for " + damage);

            hurtSystem.Play();
            
            if (shield > 0)
            {
                shield -= damage;
                if (shield < 0)
                {
                    float overflowDamage = -shield;
                    shield = 0;
                    health -= overflowDamage;
                }
            }
            else
            {
                health -= damage;
            }

            if (abstractStatus != null)
            {
                AbstractStatus existing = statusManager.statusList.FirstOrDefault(s => s.GetType() == abstractStatus.GetType());
                if (existing != null)
                    existing.Amount += abstractStatus.Amount;
                else
                {
                    abstractStatus.Entity = this;
                    statusManager.statusList.Add(abstractStatus);
                }
            }

            health = Mathf.Clamp(health, 0, initialHealth);
            ScreenShake.Instance.Shake(0.1f, 7);
        }


        public abstract void Die();
        
    }
}

