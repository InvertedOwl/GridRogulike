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
        private const string MoveFxKey = "ToonPunchLight";
        private const float MoveFxBackwardOffset = 0.75f;
        private Camera _previewHoverCamera;

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

            Vector2Int previousCoords = positionRowCol;
            PlayMoveFx(previousCoords, newCoords);

            positionRowCol = newCoords;
            GameObject currentHex = HexGridManager.Instance.GetWorldHexObject(positionRowCol);
            transform.SetParent(currentHex.transform.GetChild(3));

            Quaternion boardRotation = Quaternion.Euler(boardWorldEulerAngles);
            transform.rotation = boardRotation;

            LerpPosition lerpPosition = GetComponent<LerpPosition>();
            lerpPosition.targetLocation = boardLocalOffset;
            lerpPosition.targetRotation = lerpPosition.isLocal ? transform.localRotation : boardRotation;
            
        }

        private void PlayMoveFx(Vector2Int sourceCoords, Vector2Int targetCoords)
        {
            if (sourceCoords == targetCoords || FXManager.Instance == null)
                return;

            if (!TryGetHexWorldPosition(sourceCoords, out Vector3 sourcePosition) ||
                !TryGetHexWorldPosition(targetCoords, out Vector3 targetPosition))
            {
                return;
            }

            Vector3 moveDirection = targetPosition - sourcePosition;
            if (moveDirection.sqrMagnitude <= Mathf.Epsilon)
                return;

            Vector3 spawnPosition = Vector3.Lerp(sourcePosition, targetPosition, 0.5f) -
                                    (moveDirection.normalized * MoveFxBackwardOffset);
            if (FXManager.Instance.TryPlay(MoveFxKey, spawnPosition, out GameObject moveFx) &&
                moveFx != null)
            {
                moveFx.transform.rotation = Quaternion.LookRotation(moveDirection.normalized, Vector3.up);
            }
        }

        private bool TryGetHexWorldPosition(Vector2Int coords, out Vector3 position)
        {
            position = Vector3.zero;

            if (HexGridManager.Instance == null ||
                !HexGridManager.Instance._hexObjects.TryGetValue(coords, out GameObject hexObject) ||
                hexObject == null)
            {
                return false;
            }

            position = hexObject.transform.position;
            return true;
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

            UpdateEnemyPreviewArrowVisibility();


            FollowEyes();
        }

        private void UpdateEnemyPreviewArrowVisibility()
        {
            if (entityType != EntityType.Enemy ||
                arrowUUIDs.Count == 0 ||
                SpriteArrowManager.Instance == null)
            {
                return;
            }

            bool isEntityHovered = IsMouseHoveringThisEntity();
            foreach (string arrowUUID in arrowUUIDs)
            {
                bool isArrowHovered = isEntityHovered ||
                                      (_arrowTargetPositions.TryGetValue(arrowUUID, out Vector2Int targetPosition) &&
                                       IsMouseHoveringHex(targetPosition));

                SpriteArrowManager.Instance.SetEnemyPreviewArrowVisible(arrowUUID, isArrowHovered);
            }
        }

        private bool IsMouseHoveringThisEntity()
        {
            if (_previewHoverCamera == null)
                _previewHoverCamera = Camera.main;

            if (_previewHoverCamera == null)
                return false;

            Ray ray = _previewHoverCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit3D, Mathf.Infinity) &&
                hit3D.collider != null &&
                hit3D.collider.GetComponentInParent<AbstractEntity>() == this)
            {
                return true;
            }

            RaycastHit2D hit2D = Physics2D.GetRayIntersection(ray, Mathf.Infinity);
            return hit2D.collider != null &&
                   hit2D.collider.GetComponentInParent<AbstractEntity>() == this;
        }

        private bool IsMouseHoveringHex(Vector2Int hexPosition)
        {
            if (_previewHoverCamera == null)
                _previewHoverCamera = Camera.main;

            if (_previewHoverCamera == null ||
                HexGridManager.Instance == null ||
                !HexGridManager.Instance._hexObjects.TryGetValue(hexPosition, out GameObject hexObject) ||
                hexObject == null)
            {
                return false;
            }

            Ray ray = _previewHoverCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit3D, Mathf.Infinity) &&
                hit3D.collider != null &&
                (hit3D.collider.transform == hexObject.transform ||
                 hit3D.collider.transform.IsChildOf(hexObject.transform)))
            {
                return true;
            }

            RaycastHit2D hit2D = Physics2D.GetRayIntersection(ray, Mathf.Infinity);
            return hit2D.collider != null &&
                   (hit2D.collider.transform == hexObject.transform ||
                    hit2D.collider.transform.IsChildOf(hexObject.transform));
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
            HandleStartTurnShield();
            TriggerStartTurnStatuses();
            
            
            // Play all queued actions for next turn
            foreach (AbstractAction action in nextTurnActions)
            {

                Debug.Log("Queued action for next turn " + action.GetText());
                CardEventContext context = new CardEventContext();
                foreach (AbstractCardEvent modifiedEvent in ModifyEvents(action.Activate((global::CardMonobehaviour)null)))
                {
                    CardEventResult result = modifiedEvent.ActivateWithResult(this, context);
                    context.Record(result);
                }
            }
            nextTurnActions.Clear();
        }

        private void HandleStartTurnShield()
        {
            if (!TryPreserveShieldOnStartTurn())
                _shield = 0;
        }

        private bool TryPreserveShieldOnStartTurn()
        {
            if (statusManager == null)
                return false;

            foreach (AbstractStatus status in statusManager.statusList.ToList())
            {
                if (status.Amount <= 0 || !status.PreservesShieldOnStartTurn(this))
                    continue;

                status.OnShieldPreservedStartTurn();
                return true;
            }

            return false;
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
                foreach (AbstractStatus abstractStatus in statusManager.statusList.ToList())
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

            if (statusManager == null)
            {
                return;
            }

            if (abstractStatus is DizzyStatus)
            {
                FXManager.Instance.TryPlay("StunExplosion", transform.position);
            }
            if (abstractStatus is PoisonStatus)
            {
                FXManager.Instance.TryPlay("PosionExplosion", transform.position);
            }
            
            int amountAdded = abstractStatus.Amount;
            AbstractStatus existing = statusManager.statusList.FirstOrDefault(s => s.GetType() == abstractStatus.GetType());
            if (existing != null)
            {
                existing.Amount += abstractStatus.Amount;
                existing.OnApply(this, amountAdded);
            }
            else
            {
                abstractStatus.Entity = this;
                statusManager.statusList.Add(abstractStatus);
                abstractStatus.OnApply(this, amountAdded);
            }

            Deck.Instance?.MarkPlayabilityDirty();
        }
        
        public virtual void Damage(int damage, bool triggerDamageReceivedStatuses = true)
        {
            if (_isDead)
                return;

            Debug.Log("Damaged for " + damage);
            
            
            hurtSystem.Play();

            float healthBeforeDamage = Health;
            
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
            int healthDamageReceived = Mathf.Max(0, Mathf.RoundToInt(healthBeforeDamage - Health));
            if (triggerDamageReceivedStatuses && healthDamageReceived > 0)
            {
                TriggerDamageReceivedStatuses(healthDamageReceived);
            }

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

        public bool StatusesBlockMovement(int distance)
        {
            if (statusManager == null)
                return false;

            foreach (AbstractStatus status in statusManager.statusList.ToList())
            {
                if (status.BlocksMovement(this, distance))
                {
                    Deck.Instance?.MarkPlayabilityDirty();
                    return true;
                }
            }

            return false;
        }

        protected void TriggerTurnResourcesReadyStatuses()
        {
            if (statusManager == null)
                return;

            foreach (AbstractStatus status in statusManager.statusList.ToList())
            {
                status.OnTurnResourcesReady();
            }
        }

        private void TriggerStartTurnStatuses()
        {
            if (statusManager == null)
                return;

            foreach (AbstractStatus status in statusManager.statusList.ToList())
            {
                status.OnStartTurn();
            }
        }

        private void TriggerDamageReceivedStatuses(int damage)
        {
            if (statusManager == null)
                return;

            foreach (AbstractStatus status in statusManager.statusList.ToList())
            {
                status.OnDamageReceived(damage);
            }
        }

        private void TriggerDeathStatuses()
        {
            if (statusManager == null)
                return;

            foreach (AbstractStatus status in statusManager.statusList.ToList())
            {
                status.OnDeath();
            }
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
        private readonly Dictionary<string, Vector2Int> _arrowTargetPositions = new Dictionary<string, Vector2Int>();
        private readonly Dictionary<int, List<string>> _arrowUUIDsByActionIndex = new Dictionary<int, List<string>>();

        public virtual void ClearNextTurnActionPreviews()
        {
            foreach (string arrowUUID in arrowUUIDs)
            {
                SpriteArrowManager.Instance.DestroyArrow(arrowUUID);
            }

            arrowUUIDs.Clear();
            _arrowTargetPositions.Clear();
            _arrowUUIDsByActionIndex.Clear();

            foreach (Vector2Int pos in HexGridManager.Instance._hexObjects.Keys)
            {
                HexGridManager.Instance.GetWorldHexObject(pos).GetComponent<HexPreviewHandler>().RemoveEventsForEntity(this);
            }
        }

        public virtual void ClearNextTurnActionPreviewForAction(AbstractAction action)
        {
            if (action == null || plannedAction == null)
                return;

            int actionIndex = plannedAction.IndexOf(action);
            if (actionIndex < 0)
                return;

            ClearNextTurnActionPreviewForActionIndex(actionIndex);
        }

        private void ClearNextTurnActionPreviewForActionIndex(int actionIndex)
        {
            if (_arrowUUIDsByActionIndex.TryGetValue(actionIndex, out List<string> actionArrowUUIDs))
            {
                foreach (string arrowUUID in actionArrowUUIDs)
                {
                    SpriteArrowManager.Instance.DestroyArrow(arrowUUID);
                    arrowUUIDs.Remove(arrowUUID);
                    _arrowTargetPositions.Remove(arrowUUID);
                }

                _arrowUUIDsByActionIndex.Remove(actionIndex);
            }

            foreach (Vector2Int pos in HexGridManager.Instance._hexObjects.Keys)
            {
                HexGridManager.Instance.GetWorldHexObject(pos)
                    .GetComponent<HexPreviewHandler>()
                    .RemoveEventsForEntityAction(this, actionIndex);
            }
        }

        public virtual void HandleNextTurnActions(List<AbstractAction> actions)
        {
            ClearNextTurnActionPreviews();

            List<AbstractStatus> plannedSelfStatuses = new List<AbstractStatus>();

            for (int actionIndex = 0; actionIndex < actions.Count; actionIndex++)
            {
                AbstractAction action = actions[actionIndex];
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
                    if (abstractCardEvent.PreviewSourceActionIndex < 0)
                        abstractCardEvent.PreviewSourceActionIndex = actionIndex;

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
                            string arrowUUID = SpriteArrowManager.Instance.CreateArrow(positionRowCol,
                                pos, Color.red, "AttackIcon", attack.amount, enemyPreview: true);

                            arrowUUIDs.Add(arrowUUID);
                            _arrowTargetPositions[arrowUUID] = pos;
                            if (!_arrowUUIDsByActionIndex.TryGetValue(actionIndex, out List<string> actionArrowUUIDs))
                            {
                                actionArrowUUIDs = new List<string>();
                                _arrowUUIDsByActionIndex[actionIndex] = actionArrowUUIDs;
                            }

                            actionArrowUUIDs.Add(arrowUUID);
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
            TriggerDeathStatuses();

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

