using System;
using System.Collections.Generic;
using System.Linq;
using Cards.Actions;
using Cards.CardEvents;
using Entities.Enemies;
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
        public EyesFollowMouse eyesFollowMouse;
        public float initialHealth;
        public List<AbstractAction> plannedAction = new List<AbstractAction>();
        public AbstractEntityBehavior behavior;
        public GOList GoList;
        
        public List<AbstractAction> nextTurnActions = new List<AbstractAction>();

        public float _health;
        public EntityType entityType = EntityType.Enemy;

        public Image turnIndicatorIcon;
        
        // Generic random to generate specific entity randoms predictably 
        public static RandomState guidRandom = RunInfo.NewRandom("eguid".GetHashCode());
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ResetStaticsOnLoad()
        {
            guidRandom = RunInfo.NewRandom("eguid".GetHashCode());
        }
        
        public static string GenerateDeterministicId()
        {
            byte[] bytes = new byte[16];
            guidRandom.NextBytes(bytes);
            return new Guid(bytes).ToString();
        }
        
        
        // Unique entity random
        public RandomState EntityRandom = RunInfo.NewRandom(GenerateDeterministicId().GetHashCode());
        
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
            Debug.Log("Target new coords " + newCoords);
            
            positionRowCol = newCoords;
            GetComponent<LerpPosition>().targetLocation = new Vector3(0, 0.135f, -0.171f);
            GameObject currentHex = HexGridManager.Instance.GetWorldHexObject(positionRowCol);
            transform.SetParent(currentHex.transform.GetChild(3));
            
        }

        public void Update()
        {
            healthBarManager.health = Health;
            healthBarManager.initialHealth = initialHealth;
            healthBarManager.shield = Shield;
            


            FollowEyes();
        }


        public void FollowEyes()
        {
            if (eyesFollowMouse == null)
            {
                return;
            }
            bool attacking = false;
            if (plannedAction.Count > 0)
            {
                
                foreach (AbstractAction action in plannedAction)
                {
                    if (action is AttackAction attackAction)
                    {
                        Vector2Int targetHexPos = HexGridManager.MoveHex(positionRowCol, attackAction.Direction,
                            attackAction.Distance);
                        eyesFollowMouse.setTargetPosition = HexGridManager.GetHexCenter(targetHexPos.x, targetHexPos.y);
                        attacking = true;
                    }
                }
            }
            if (!attacking)
            {
                PlayingState playingState = GameStateManager.Instance.GetCurrent<PlayingState>();
                Vector2Int playerPos = playingState.player.positionRowCol;
                eyesFollowMouse.setTargetPosition = HexGridManager.GetHexCenter(playerPos.x, playerPos.y);
            }        
        }
        
        public virtual void StartTurn()
        {
            _shield = 0;
            
            
            // Play all queued actions for next turn
            foreach (AbstractAction action in nextTurnActions)
            {

                Debug.Log("Queued action for next turn " + action.GetText());
                foreach (AbstractCardEvent cardEvent in action.Activate(null))
                {

                    foreach (AbstractCardEvent modifiedEvent in ModifyEvents(
                                 new List<AbstractCardEvent> { cardEvent }))
                    {
                        
                        modifiedEvent.Activate(this);
                    }
                }
            }
            nextTurnActions.Clear();
        }
        
        public List<AbstractCardEvent> ModifyEvents(List<AbstractCardEvent> events)
        {
            List<AbstractCardEvent> modifiedEvents = new List<AbstractCardEvent>(events);

            foreach (AbstractStatus status in statusManager.statusList)
            {
                modifiedEvents = status.Modify(modifiedEvents);
            }
            
            return modifiedEvents;
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

            if (_health <= 0)
            {
                Die();
            }
            
            
            // Instantiate damage number
            GameObject newDamageNumber = Instantiate(GoList.GetValue("DamageValueParticle"), transform);
            newDamageNumber.GetComponent<TextMeshPro>().text = "" + damage;
            
        }

        
        List<string> arrowUUIDs = new List<string>();

        public virtual void HandleNextTurnActions(List<AbstractAction> actions)
        {
            foreach (string arrowUUID in arrowUUIDs)
            {
                SpriteArrowManager.Instance.DestroyArrow(arrowUUID);
            }
            arrowUUIDs.Clear();

            foreach (AbstractAction action in actions)
            {
                foreach (AbstractCardEvent abstractCardEvent in action.Activate(null))
                {
                    Vector2Int pos = new Vector2Int(-20000, -20000);

                    if (abstractCardEvent is AttackCardEvent attackCardEvent)
                    {
                        if (attackCardEvent.usePosition)
                        {
                            pos = attackCardEvent.position;
                        }
                        else
                        {
                            pos = HexGridManager.MoveHex(
                                positionRowCol,
                                attackCardEvent.direction,
                                attackCardEvent.distance
                            );
                        }
                    }

                    if (!pos.Equals(new Vector2Int(-20000, -20000)) &&
                        HexGridManager.Instance.GetAllGridPositions().Contains(pos))
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

                        // Always show arrow
                        if (abstractCardEvent is AttackCardEvent attack)
                        {
                            arrowUUIDs.Add(SpriteArrowManager.Instance.CreateArrow(positionRowCol,
                                pos, Color.red, "AttackIcon", attack.amount));
                        }
                    }
                }
            }
        }
        

        public virtual void Die()
        {
            // Rip entity :(
            Debug.Log("I have died");
            foreach (Vector2Int pos in HexGridManager.Instance._hexObjects.Keys)
            {
                HexGridManager.Instance.GetWorldHexObject(pos).GetComponent<HexPreviewHandler>().eventsOnThisHex.Remove(this);
            }


            plannedAction = new List<AbstractAction>();
            HandleNextTurnActions(plannedAction);
            if (GoList.HasValue("DeadEyes"))
            {
                eyesFollowMouse.gameObject.SetActive(false);
                GoList.GetValue("DeadEyes").SetActive(true);
            }
        }
    }
}

