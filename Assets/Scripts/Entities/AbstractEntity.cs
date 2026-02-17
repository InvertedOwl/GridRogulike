using System;
using System.Collections.Generic;
using System.Linq;
using Cards.Actions;
using Cards.CardEvents;
using Grid;
using StateManager;
using TMPro;
using Types.Statuses;
using Types.Tiles;
using UnityEngine.UI;
using Util;

namespace Entities
{
    using UnityEngine;

    public abstract class AbstractEntity : MonoBehaviour
    {
        public float initialHealth;

        public float _health;
        public EntityType entityType = EntityType.Enemy;
        
        
        // Generic random to generate specific entity randoms predictably 
        public static System.Random guidRandom = RunInfo.NewRandom("eguid".GetHashCode());
        public static string GenerateDeterministicId()
        {
            byte[] bytes = new byte[16];
            guidRandom.NextBytes(bytes);
            return new Guid(bytes).ToString();
        }
        
        // Unique entity random
        protected System.Random _entityRandom = RunInfo.NewRandom(GenerateDeterministicId().GetHashCode());
        
        public Dictionary<Vector2Int, int> CalculateDistanceMap(Vector2Int hexPosition, PlayingState playingState, AbstractEntity nonblock = null)
        {
            List<Vector2Int> blockers = new List<Vector2Int>();
            
            foreach (AbstractEntity abstractEntity in playingState.GetEntities())
            {
                if (abstractEntity == this)
                    continue;

                if (nonblock != null && abstractEntity == nonblock)
                    continue;
                
                blockers.Add(abstractEntity.positionRowCol);
            }
            
            Dictionary<Vector2Int, int> distanceMap = HexGridManager.Instance.CalculateDistanceMap(hexPosition, blockers);
            
            return distanceMap;
        }
        
        public float Health
        {
            get => _health;
            set
            {
                if (entityType == EntityType.Player)
                {
                    BattleStats.HealDoneThisBattle += Math.Max((int) (value-_health), 0);
                    BattleStats.HealDoneThisTurn += Math.Max((int) (value-_health), 0);
                }
                _health = Math.Min(value, initialHealth);
            }
        }
        private float _shield;

        public float Shield
        {
            get => _shield;
            set
            {
                if (entityType == EntityType.Player)
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
            GetComponent<LerpPosition>().targetLocation = Vector3.zero;
            GameObject currentHex = HexGridManager.Instance.GetWorldHexObject(positionRowCol);
            transform.SetParent(currentHex.transform.GetChild(3));
            
        }

        public void Update()
        {
            healthBarManager.health = Health;
            healthBarManager.initialHealth = initialHealth;
            healthBarManager.shield = Shield;
        }


        
        public virtual void StartTurn()
        {
            _shield = 0;
            Debug.Log("Start Turn: " + this.name);
            
            Debug.Log("Shield: " + Shield);
            
            // Activate tile events
            TileEntry tile = TileData.tiles[HexGridManager.Instance.HexType(positionRowCol)];
            foreach (AbstractCardEvent cardEvent in tile.landEvent.Invoke())
            {
                cardEvent.Activate(this);
            }
            
        }
        public virtual void EndTurn()
        {
            Debug.Log("End Turn: " + this.name);
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

        
        List<string> arrowUUIDs = new List<string>();

        public virtual void HandleNextTurnActions(List<AbstractAction> actions)
        {
            foreach (string arrowUUID in arrowUUIDs)
            {
                SpriteArrowManager.Instance.DestroyArrow(arrowUUID);
            }
            
            
            foreach (AbstractAction action in actions)
            {
                foreach (AbstractCardEvent abstractCardEvent in action.Activate(null))
                {
                    Vector2Int pos = new Vector2Int(-20000, -20000);
                    if (abstractCardEvent is AttackCardEvent)
                    {
                        if (((AttackCardEvent)abstractCardEvent).usePosition)
                        {
                            pos = ((AttackCardEvent)abstractCardEvent).position;
                        }
                        else
                        {
                            pos = HexGridManager.MoveHex(positionRowCol,
                                ((AttackCardEvent)abstractCardEvent).direction,
                                ((AttackCardEvent)abstractCardEvent).distance);
                        }
                    }

                    if (!pos.Equals(new Vector2Int(-20000, -20000)))
                    {
                        GameObject hex = HexGridManager.Instance.GetWorldHexObject(pos);
                        HexPreviewHandler hexHandler = hex.GetComponent<HexPreviewHandler>();
                        if (hexHandler.eventsOnThisHex.ContainsKey(this))
                        {
                            hexHandler.eventsOnThisHex[this].Add(abstractCardEvent);    
                        }
                        else
                        {
                            hexHandler.eventsOnThisHex[this] = new List<AbstractCardEvent> { abstractCardEvent };
                        }
                    }
                }
            }
        }

        public void Die()
        {
            // Rip entity :(
            foreach (Vector2Int pos in HexGridManager.Instance._hexObjects.Keys)
            {
                HexGridManager.Instance.GetWorldHexObject(pos).GetComponent<HexPreviewHandler>().eventsOnThisHex.Remove(this);
            }

        }
        
    }
}

