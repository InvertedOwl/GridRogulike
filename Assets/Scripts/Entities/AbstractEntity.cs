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

        public ParticleSystem hurtSystem;
        public StatusManager statusManager;
        public HealthBarManager healthBarManager;

        public void MoveEntity(Vector2Int newCoords)
        {
            positionRowCol = newCoords;
            GetComponent<LerpPosition>().targetLocation = HexGridManager.GetHexCenter(positionRowCol.x, positionRowCol.y);
            
        }

        public void Update()
        {
            healthBarManager.health =  health;
            healthBarManager.initialHealth = initialHealth;
            healthBarManager.shield = shield;
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

