using Grid;
using TMPro;
using Util;

namespace Entities
{
    using UnityEngine;

    public abstract class AbstractEntity : MonoBehaviour
    {
        public float initialHealth;
        public float health;
        public Vector2Int positionRowCol;
        public TextMeshPro healthText;
        public GameObject healthBar;
        public EntityGlow entityGlow;

        public void MoveEntity(Vector2Int newCoords)
        {
            positionRowCol = newCoords;
            GetComponent<LerpPosition>().targetLocation = HexGridManager.GetHexCenter(positionRowCol.x, positionRowCol.y);
            
        }

        public void Update()
        {
          RectTransform rectTransform = healthBar.GetComponent<RectTransform>();

          healthText.text = health + "/" + initialHealth;
          float healthRatio = health / initialHealth;

          
          // Magic number. Change later
          float fullWidth = 19.5f;

          // rectTransform.sizeDelta = new Vector2(healthRatio * fullWidth, rectTransform.sizeDelta.y);

        }

        public void StartTurn()
        {
            entityGlow.Glow();
        }
        public void EndTurn()
        {
            entityGlow.Glow();
        }

        public virtual void Damage(int damage)
        {
            health -= damage;
            health = Mathf.Clamp(health, 0, initialHealth);
            ScreenShake.Instance.Shake(0.2f, 7);
        }

        public abstract void Die();
        
    }
}

