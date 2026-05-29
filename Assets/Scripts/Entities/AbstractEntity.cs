using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cards.Actions;
using Cards.CardEvents;
using Entities.Enemies;
using Grid;
using Spine.Unity;
using StateManager;
using TMPro;
using Types.Statuses;
using Types.Tiles;
using UnityEngine.AdaptivePerformance.Provider;
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
        public SkeletonAnimation skeletonAnimation;
        private Coroutine _blinkCoroutine;
        private bool _isDead;
        
        public List<AbstractAction> nextTurnActions = new List<AbstractAction>();

        public float _health;
        public EntityType entityType = EntityType.Enemy;
        private const float PlayerDamageShakeMagnitude = 0.08f;
        private const float EnemyDamageShakeMagnitude = 0.035f;
        private const float PlayerDamageShakeDamping = 8f;
        private const float EnemyDamageShakeDamping = 10f;

        public Image turnIndicatorIcon;
        
        // Generic random to generate specific entity randoms predictably 
        public static RandomState guidRandom = RunInfo.NewRandom("eguid");
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ResetStaticsOnLoad()
        {
            ResetStatics();
        }

        public static void ResetStatics()
        {
            guidRandom = RunInfo.NewRandom("eguid");
        }
        
        public static string GenerateDeterministicId()
        {
            byte[] bytes = new byte[16];
            guidRandom.NextBytes(bytes);
            return new Guid(bytes).ToString();
        }
        
        
        // Unique entity random
        public RandomState EntityRandom = RunInfo.NewRandom(GenerateDeterministicId());
        
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
        [SerializeField] private Vector3 boardWorldEulerAngles = new Vector3(-70f, 0f, 0f);
        private Vector3 boardLocalOffset = new Vector3(0.121f, 0.18f, -0.171f);

        public void MoveEntity(Vector2Int newCoords)
        {
            Debug.Log("Target new coords " + newCoords);
            
            positionRowCol = newCoords;
            GameObject currentHex = HexGridManager.Instance.GetWorldHexObject(positionRowCol);
            transform.SetParent(currentHex.transform.GetChild(3));

            Quaternion boardRotation = Quaternion.Euler(boardWorldEulerAngles);
            transform.rotation = boardRotation;

            LerpPosition lerpPosition = GetComponent<LerpPosition>();
            lerpPosition.targetLocation = boardLocalOffset;
            lerpPosition.targetRotation = lerpPosition.isLocal ? transform.localRotation : boardRotation;
            
        }

        public void Start()
        {
            if (skeletonAnimation != null)
            {

                _blinkCoroutine = StartCoroutine(Blink());

            }
        }
        public IEnumerator Blink()
        {
            while (!_isDead)
            {
                yield return new WaitForSeconds(Random.Range(1.5f, 4.5f));

                if (_isDead || skeletonAnimation == null)
                    yield break;

                if (TrySetAnimation(1, "blink", false))
                {
                    skeletonAnimation.AnimationState.AddEmptyAnimation(1, 0.1f, 0f);
                }
            }
        }

        public void Update()
        {
            if (healthBarManager != null)
                healthBarManager.SetValues(Health, initialHealth, Shield);
            


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
                foreach (AbstractCardEvent modifiedEvent in ModifyEvents(action.Activate(null)))
                {
                    modifiedEvent.Activate(this);
                }
            }
            nextTurnActions.Clear();
        }
        
        public List<AbstractCardEvent> ModifyEvents(List<AbstractCardEvent> events, bool previewMode = false)
        {
            return CardEventPipeline.Apply(events, this, previewMode: previewMode);
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

        public virtual void ApplyStatus(AbstractStatus abstractStatus)
        {
            if (abstractStatus == null)
            {
                return;
            }
            
            Debug.Log("Type of status applied " + abstractStatus.GetType());
            
            AbstractStatus existing = statusManager.statusList.FirstOrDefault(s => s.GetType() == abstractStatus.GetType());
            if (existing != null)
                existing.Amount += abstractStatus.Amount;
            else
            {
                abstractStatus.Entity = this;
                statusManager.statusList.Add(abstractStatus);
            }
        }
        
        public virtual void Damage(int damage)
        {
            if (_isDead)
                return;

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

            Health = Mathf.Clamp(Health, 0, initialHealth);
            ShakeOnDamage();

            // Instantiate damage number
            GameObject newDamageNumber = Instantiate(GoList.GetValue("DamageValueParticle"), transform);
            newDamageNumber.GetComponent<TextMeshPro>().text = "" + damage;

            if (_health <= 0)
            {
                Die();
                return;
            }

            PlayHurtAnimation();
            
        }

        private void PlayHurtAnimation()
        {
            if (skeletonAnimation == null)
                return;

            if (TrySetAnimation(0, "hurt", false))
            {
                TryAddAnimation(0, "idle", true, 0f);
            }
        }

        private void ShakeOnDamage()
        {
            if (ScreenShake.Instance == null)
                return;

            if (entityType == EntityType.Player)
            {
                ScreenShake.Instance.Shake(PlayerDamageShakeMagnitude, PlayerDamageShakeDamping);
                return;
            }

            ScreenShake.Instance.Shake(EnemyDamageShakeMagnitude, EnemyDamageShakeDamping);
        }

        
        List<string> arrowUUIDs = new List<string>();

        public virtual void HandleNextTurnActions(List<AbstractAction> actions)
        {
            foreach (string arrowUUID in arrowUUIDs)
            {
                SpriteArrowManager.Instance.DestroyArrow(arrowUUID);
            }
            arrowUUIDs.Clear();

            List<AbstractStatus> plannedSelfStatuses = new List<AbstractStatus>();

            foreach (AbstractAction action in actions)
            {
                List<AbstractCardEvent> modifiedEvents = ModifyEvents(
                    action.Activate(null, previewMode: true),
                    previewMode: true
                );

                foreach (AbstractStatus plannedSelfStatus in plannedSelfStatuses)
                {
                    modifiedEvents = plannedSelfStatus.Modify(modifiedEvents, previewMode: true);
                }

                foreach (AbstractCardEvent abstractCardEvent in modifiedEvents)
                {
                    if (abstractCardEvent is ApplyStatusToEntityCardEvent applyStatusEvent &&
                        applyStatusEvent.target == this &&
                        applyStatusEvent.status != null)
                    {
                        plannedSelfStatuses.Add(applyStatusEvent.status);
                    }

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

                        hexHandler.AddPreviewEvent(this, abstractCardEvent);

                        // Always show arrow
                        if (abstractCardEvent is AttackCardEvent attack)
                        {
                            arrowUUIDs.Add(SpriteArrowManager.Instance.CreateArrow(positionRowCol,
                                pos, Color.red, "AttackIcon", attack.amount, enemyPreview: true));
                        }
                    }
                }
            }
        }
        

        public virtual void Die()
        {
            if (_isDead)
                return;

            _isDead = true;

            // Rip entity :(
            Debug.Log("I have died");

            if (_blinkCoroutine != null)
            {
                StopCoroutine(_blinkCoroutine);
                _blinkCoroutine = null;
            }

            if (skeletonAnimation != null)
            {
                skeletonAnimation.AnimationState.ClearTrack(1);

                if (HasSkeletonAnimation("die"))
                {
                    skeletonAnimation.AnimationState.ClearTrack(0);
                    TrySetAnimation(0, "die", false);
                }
            }

            foreach (Vector2Int pos in HexGridManager.Instance._hexObjects.Keys)
            {
                HexGridManager.Instance.GetWorldHexObject(pos).GetComponent<HexPreviewHandler>().RemoveEventsForEntity(this);
            }

            plannedAction = new List<AbstractAction>();
            HandleNextTurnActions(plannedAction);
            if (GoList.HasValue("DeadEyes"))
            {
                eyesFollowMouse.gameObject.SetActive(false);
                GoList.GetValue("DeadEyes").SetActive(true);
            }
        }

        protected bool TrySetAnimation(int trackIndex, string animationName, bool loop)
        {
            if (!HasSkeletonAnimation(animationName))
                return false;

            skeletonAnimation.AnimationState.SetAnimation(trackIndex, animationName, loop);
            return true;
        }

        protected bool TryAddAnimation(int trackIndex, string animationName, bool loop, float delay)
        {
            if (!HasSkeletonAnimation(animationName))
                return false;

            skeletonAnimation.AnimationState.AddAnimation(trackIndex, animationName, loop, delay);
            return true;
        }

        protected bool HasSkeletonAnimation(string animationName)
        {
            return skeletonAnimation != null &&
                   skeletonAnimation.Skeleton?.Data?.FindAnimation(animationName) != null;
        }
    }
}

