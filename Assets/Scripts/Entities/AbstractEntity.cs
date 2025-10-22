using System;
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

        public float _health;
        public float Health
        {
            get => _health;
            set
            {
                if (this is Player)
                {
                    BattleStats.HealDoneThisBattle += Math.Max((int) (value-_health), 0);
                    BattleStats.HealDoneThisTurn += Math.Max((int) (value-_health), 0);
                }
                _health = value;
            }
        }
        public float _shield;

        public float Shield
        {
            get => _shield;
            set
            {
                if (this is Player)
                {
                    BattleStats.ShieldDoneThisBattle += Math.Max((int) (value-_shield), 0);
                    BattleStats.ShieldDoneThisTurn += Math.Max((int) (value-_shield), 0);
                }
                _shield = value;
            }
        }
        
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
            healthBarManager.health = Health;
            healthBarManager.initialHealth = initialHealth;
            healthBarManager.shield = Shield;
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
            
            if (Shield > 0)
            {
                Shield -= damage;
                if (Shield < 0)
                {
                    float overflowDamage = -Shield;
                    Shield = 0;
                    Health -= overflowDamage;
                }
            }
            else
            {
                Health -= damage;
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

            Health = Mathf.Clamp(Health, 0, initialHealth);
            ScreenShake.Instance.Shake(0.1f, 7);
        }


        public abstract void Die();
        
    }
}

