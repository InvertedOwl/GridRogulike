using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cards;
using Cards.CardEvents;
using Entities;
using StateManager;
using TMPro;
using Types.Statuses;
using UnityEngine;
using UnityEngine.EventSystems;
using Util;

namespace Grid {
    public class HexClickPlayerController : MonoBehaviour
    {
        public bool isMoving = false;
        public bool isAttacking = false;

        public List<AttackCardEvent> ToAttack = new List<AttackCardEvent>();
        private CardMonobehaviour _pendingCard;
        private bool _pendingCardHasStarted;
        private CardMonobehaviour _pendingTargetCard;
        private TargetSelection _pendingTargetSelection = TargetSelection.Empty();
        private readonly HashSet<Vector2Int> _pendingTargetPositions = new HashSet<Vector2Int>();
        private CardMonobehaviour _pendingNonManualAttackPreviewCard;
        private readonly HashSet<Vector2Int> _pendingNonManualAttackPreviewPositions = new HashSet<Vector2Int>();
        private readonly HashSet<int> _syncedMovableParticleObjects = new HashSet<int>();
        private int _lastHexClickFrame = -1;

        public void AddToAttack(IEnumerable<AbstractCardEvent> cardEvents)
        {
            foreach (AbstractCardEvent cardEvent in cardEvents)
            {
                ToAttack.Add((AttackCardEvent) cardEvent);
            }
        }

        public void BeginCardAttack(List<AttackCardEvent> cardEvents, CardMonobehaviour card)
        {
            ClearPendingAttacks();

            if (SpriteArrowManager.Instance != null)
                SpriteArrowManager.Instance.SetEnemyPreviewArrowsVisible(false);

            ToAttack.AddRange(cardEvents);
            _pendingCard = card;
            _pendingCardHasStarted = false;
            SetupAttack();
        }

        public void PreviewAttackEvents(IEnumerable<AttackCardEvent> cardEvents)
        {
            ClearToAttackEmitters();

            if (GameStateManager.Instance == null || !GameStateManager.Instance.IsCurrent<PlayingState>())
                return;

            PlayingState playingState = GameStateManager.Instance.GetCurrent<PlayingState>();

            foreach (AttackCardEvent attackCardEvent in cardEvents)
            {
                if (!TryGetAttackPosition(attackCardEvent, playingState.player.positionRowCol, out Vector2Int attackPosition))
                    continue;

                ShowAttackPreview(attackPosition);
            }
        }

        public void BeginNonManualAttackPreview(IEnumerable<AttackCardEvent> cardEvents, CardMonobehaviour card)
        {
            _pendingNonManualAttackPreviewCard = card;
            _pendingNonManualAttackPreviewPositions.Clear();

            if (GameStateManager.Instance == null || !GameStateManager.Instance.IsCurrent<PlayingState>())
                return;

            PlayingState playingState = GameStateManager.Instance.GetCurrent<PlayingState>();

            foreach (AttackCardEvent attackCardEvent in cardEvents)
            {
                if (TryGetAttackPosition(attackCardEvent, playingState.player.positionRowCol, out Vector2Int attackPosition))
                    _pendingNonManualAttackPreviewPositions.Add(attackPosition);
            }
        }

        public void BeginCardTargeting(CardMonobehaviour card)
        {
            ClearPendingAttacks();
            ClearPendingCardTargeting(false);
            ClearToAttackEmitters();

            _pendingTargetCard = card;
            RefreshPendingCardTargets();

            if (SpriteArrowManager.Instance != null)
                SpriteArrowManager.Instance.SetEnemyPreviewArrowsVisible(false);
        }

        private void RefreshPendingCardTargets()
        {
            _pendingTargetPositions.Clear();
            _pendingTargetSelection = TargetSelection.Empty();

            if (_pendingTargetCard == null ||
                GameStateManager.Instance == null ||
                !GameStateManager.Instance.IsCurrent<PlayingState>())
            {
                return;
            }

            PlayingState playingState = GameStateManager.Instance.GetCurrent<PlayingState>();
            _pendingTargetSelection = _pendingTargetCard.ResolveAvailableTargets(false);

            foreach (Vector2Int position in _pendingTargetSelection.TargetPositions)
            {
                _pendingTargetPositions.Add(position);
                ShowAttackPreview(position);
            }
        }
        
        
        public static void StaticHexClickCallback(Vector2Int hexPosition, GameObject go)
        {
            HexClickPlayerController.instance.HexClickCallback(hexPosition);
        }

        public static void StaticHexHoverOnCallback(Vector2Int hexPosition, GameObject go)
        {
            HexClickPlayerController.instance.HexHoverOnCallback(hexPosition);
        }
        public static void StaticHexHoverOffCallback(Vector2Int hexPosition, GameObject go)
        {
            HexClickPlayerController.instance.HexHoverOffCallback(hexPosition);
        }
        
        public static HexClickPlayerController instance;

        public static void ResetStatics()
        {
            instance = null;
        }

        public void Awake ()
        {
            instance = this;
        }

        public void Update()
        {
            if (ToAttack.Count > 0 && !isAttacking)
            {
                SetupAttack();
            }

            if (ToAttack.Count == 0)
            {
                isAttacking = false;
            }
        }

        private void LateUpdate()
        {
            HandleSelectedCardMissClick();
        }

        public void SetupAttack()
        {
            if (ToAttack.Count == 0)
            {
                isAttacking = false;
                return;
            }

            PlayingState playingState = GameStateManager.Instance.GetCurrent<PlayingState>();
            
            Dictionary<Vector2Int, int> noBlockerMapFromPlayer = HexGridManager.Instance.CalculateDistanceMap(playingState.player.positionRowCol, new List<Vector2Int>());

            int numEntitiesToHit = 0;
            
            foreach (AbstractEntity entity in playingState.GetEntities())
            {
                if (!playingState.IsPlayerAttackTarget(entity))
                    continue;

                if (!noBlockerMapFromPlayer.TryGetValue(entity.positionRowCol, out int distanceFromPlayer) ||
                    distanceFromPlayer > ToAttack[0].distance ||
                    distanceFromPlayer == -1)
                {
                    continue;
                }

                EaseColor tileColor = HexGridManager.Instance.GetWorldHexObject(entity.positionRowCol)
                    .GetComponent<GOList>().GetValue("RedGlow").GetComponent<EaseColor>();
                Debug.Log("Settin red glow to 60 percent opacity");
                Debug.Log(tileColor);
                tileColor.SendToColor(new Color(tileColor.targetColor.r, tileColor.targetColor.g, tileColor.targetColor.b, 0.6f));
                numEntitiesToHit += 1;
            }

            if (numEntitiesToHit == 0)
            {
                if (_pendingCard != null)
                    ClearPendingAttacks();
                else
                    ResolveCurrentAttack();

                isAttacking = false;
                return;
            }

            isAttacking = true;
        }

        public void ClearPendingAttacks()
        {
            ClearPendingCardTargeting();
            ClearPendingNonManualAttackPreview();

            if (_pendingCard != null)
            {
                if (_pendingCardHasStarted)
                    _pendingCard.FinishManualAttackResolution();
                else
                    _pendingCard.CancelManualAttackTargeting();
            }

            ClearToAttackEmitters();
            if (SpriteArrowManager.Instance != null)
                SpriteArrowManager.Instance.DestroyArrow(arrowUUID);
            if (SpriteArrowManager.Instance != null)
                SpriteArrowManager.Instance.SetEnemyPreviewArrowsVisible(true);
            ToAttack.Clear();
            _pendingCard = null;
            _pendingCardHasStarted = false;
            isAttacking = false;
        }

        public void ClearPendingCardTargeting(bool cancelCard = true)
        {
            if (_pendingTargetCard != null && cancelCard)
                _pendingTargetCard.CancelTargeting();

            _pendingTargetCard = null;
            _pendingTargetSelection = TargetSelection.Empty();
            _pendingTargetPositions.Clear();

            if (SpriteArrowManager.Instance != null)
                SpriteArrowManager.Instance.SetEnemyPreviewArrowsVisible(true);
        }

        public void ClearPendingNonManualAttackPreview()
        {
            _pendingNonManualAttackPreviewCard = null;
            _pendingNonManualAttackPreviewPositions.Clear();
        }

        private void ResolveCurrentAttack()
        {
            if (ToAttack.Count == 0)
                return;

            ToAttack.RemoveAt(0);
        }

        private void FinishPendingCardAttack()
        {
            if (_pendingCard != null)
                _pendingCard.FinishManualAttackResolution();

            if (SpriteArrowManager.Instance != null)
                SpriteArrowManager.Instance.SetEnemyPreviewArrowsVisible(true);

            _pendingCard = null;
            _pendingCardHasStarted = false;
            isAttacking = false;
        }

        public void ClearToAttackEmitters()
        {
            if (GameStateManager.Instance == null || !GameStateManager.Instance.IsCurrent<PlayingState>())
                return;

            foreach (Vector2Int position in HexGridManager.Instance.BoardDictionary.Keys)
            {
                EaseColor tileColor = HexGridManager.Instance.GetWorldHexObject(position)
                    .GetComponent<GOList>()
                    .GetValue("RedGlow")
                    .GetComponent<EaseColor>();
                tileColor.targetColor = new Color(tileColor.targetColor.r, tileColor.targetColor.g, tileColor.targetColor.b, 0.0f);
            }
        }

        private bool TryGetAttackPosition(AttackCardEvent attackCardEvent, Vector2Int origin, out Vector2Int attackPosition)
        {
            if (!attackCardEvent.usePosition && string.IsNullOrEmpty(attackCardEvent.direction) && attackCardEvent.distance > 0)
            {
                attackPosition = Vector2Int.zero;
                return false;
            }

            attackPosition = attackCardEvent.usePosition
                ? attackCardEvent.position
                : HexGridManager.MoveHex(origin, attackCardEvent.direction, attackCardEvent.distance);

            return HexGridManager.Instance.BoardDictionary.ContainsKey(attackPosition);
        }

        private void ShowAttackPreview(Vector2Int attackPosition)
        {
            EaseColor tileColor = HexGridManager.Instance.GetWorldHexObject(attackPosition)
                .GetComponent<GOList>()
                .GetValue("RedGlow")
                .GetComponent<EaseColor>();
            tileColor.SendToColor(new Color(tileColor.targetColor.r, tileColor.targetColor.g, tileColor.targetColor.b, 0.6f));
        }

        public void UpdateMovableParticles(PlayingState playingState)
        {

            if (!GameStateManager.Instance.IsCurrent<PlayingState>())
                return;
            
            Dictionary<Vector2Int, int> currentMap =
                CalculateDistanceMap(playingState.player.positionRowCol, playingState);
            
            foreach (var key in currentMap.Keys)
            {
                if (!playingState.IsPlayerMovementBlocked &&
                    playingState.CanPlayerMove &&
                    currentMap[key] <= RunInfo.Instance.CurrentSteps &&
                    currentMap[key] > 0 &&
                    playingState.CurrentTurn.entityType == EntityType.Player)
                {
                    GOList list = HexGridManager.Instance.GetWorldHexObject(key)
                        .GetComponent<GOList>();
                    SetMovableParticlesActive(list, true);
                }
                else
                {
                    GOList list = HexGridManager.Instance.GetWorldHexObject(key)
                        .GetComponent<GOList>();
                    SetMovableParticlesActive(list, false);
                }
            }
        }

        public void ClearMovableParticles()
        {
            if (HexGridManager.Instance == null)
                return;

            ClearStepsHere();

            foreach (Vector2Int position in HexGridManager.Instance.BoardDictionary.Keys)
            {
                if (!HexGridManager.Instance._hexObjects.TryGetValue(position, out GameObject hex) || hex == null)
                    continue;

                GOList list = hex.GetComponent<GOList>();
                if (list == null || !list.HasValue("Particles"))
                    continue;

                SetMovableParticlesActive(list, false);
            }
        }

        private void SetMovableParticlesActive(GOList list, bool active)
        {
            if (list == null || !list.HasValue("Particles"))
                return;

            GameObject particles = list.GetValue("Particles");
            if (particles == null)
                return;

            int particlesId = particles.GetInstanceID();
            if (!active)
            {
                _syncedMovableParticleObjects.Remove(particlesId);
                particles.SetActive(false);
                return;
            }

            particles.SetActive(true);

            if (_syncedMovableParticleObjects.Add(particlesId))
            {
                SyncParticlesToGlobalTime(particles);
            }
        }

        private void SyncParticlesToGlobalTime(GameObject particles)
        {
            foreach (ParticleSystem particleSystem in particles.GetComponentsInChildren<ParticleSystem>(true))
            {
                ParticleSystem.MainModule main = particleSystem.main;
                float duration = Mathf.Max(main.duration, 0.01f);
                float time = Mathf.Repeat(Time.time, duration);

                particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                particleSystem.useAutoRandomSeed = false;
                particleSystem.randomSeed = 1;
                particleSystem.Simulate(time, true, true, true);
                particleSystem.Play(true);
            }
        }

        private string arrowUUID;
        
        public void HexHoverOnCallback(Vector2Int hexPosition)
        {
            ClearStepsHere();

            if (!GameStateManager.Instance.IsCurrent<PlayingState>())
                return;

            PlayingState playingState = GameStateManager.Instance.GetCurrent<PlayingState>();
            ShowStepsHere(hexPosition, playingState);

            if (_pendingTargetCard != null && _pendingTargetPositions.Contains(hexPosition))
            {
                PlayingState targetPreviewState = GameStateManager.Instance.GetCurrent<PlayingState>();
                if (CardTargetResolver.TryResolveSelectionForClick(
                        _pendingTargetCard,
                        _pendingTargetCard.Card,
                        targetPreviewState.player,
                        targetPreviewState,
                        hexPosition,
                        out TargetSelection selection))
                {
                    AttackCardEvent previewAttack = _pendingTargetCard
                        .BuildPreviewEventsForSelection(selection)
                        .OfType<AttackCardEvent>()
                        .FirstOrDefault();

                    int amount = previewAttack?.amount ?? 0;
                    arrowUUID = SpriteArrowManager.Instance.CreateArrow(
                        targetPreviewState.player.positionRowCol,
                        hexPosition,
                        Color.red,
                        "AttackIcon",
                        amount);
                }

                return;
            }

            if (!isAttacking)
                return;
        
            Dictionary<Vector2Int, int> distanceMap = HexGridManager.Instance.CalculateDistanceMap(playingState.player.positionRowCol, new List<Vector2Int>());


            bool isHoveringEntity = false;
            foreach (AbstractEntity entity in playingState.GetEntities())
            {
                if (entity.positionRowCol == hexPosition && playingState.IsPlayerAttackTarget(entity))
                {
                    isHoveringEntity = true;
                }
            }

            if (distanceMap.TryGetValue(hexPosition, out int distanceFromPlayer) &&
                distanceFromPlayer >= 0 &&
                distanceFromPlayer <= ToAttack[0].distance &&
                isHoveringEntity)
            {
                AttackCardEvent previewAttack = GetIncomingModifiedAttack(
                    ToAttack[0],
                    playingState,
                    hexPosition,
                    true);

                arrowUUID = SpriteArrowManager.Instance.CreateArrow(
                    playingState.player.positionRowCol,
                    hexPosition,
                    Color.red,
                    "AttackIcon",
                    previewAttack.amount);
            }
        }
        public void HexHoverOffCallback(Vector2Int hexPosition)
        {
            ClearStepsHere();
            SpriteArrowManager.Instance.DestroyArrow(arrowUUID);
        }

        private void ShowStepsHere(Vector2Int hexPosition, PlayingState playingState)
        {
            if (playingState == null ||
                !playingState.CanPlayerMove ||
                playingState.IsPlayerMovementBlocked)
            {
                return;
            }

            Dictionary<Vector2Int, int> distanceMap = CalculateDistanceMap(
                playingState.player.positionRowCol,
                playingState);

            if (!distanceMap.TryGetValue(hexPosition, out int steps) || steps <= 0)
                return;

            GameObject stepsHere = GetStepsHereObject(hexPosition);
            if (stepsHere == null)
                return;

            TMP_Text text = stepsHere.GetComponent<TMP_Text>() ??
                            stepsHere.GetComponentInChildren<TMP_Text>(true);
            if (text != null)
                text.text = $"<sprite name=\"footsteps\">{steps}";

            stepsHere.SetActive(true);
        }

        private void ClearStepsHere()
        {
            if (HexGridManager.Instance == null)
                return;

            foreach (Vector2Int position in HexGridManager.Instance.BoardDictionary.Keys)
            {
                GameObject stepsHere = GetStepsHereObject(position);
                if (stepsHere != null)
                    stepsHere.SetActive(false);
            }
        }

        private GameObject GetStepsHereObject(Vector2Int position)
        {
            if (HexGridManager.Instance == null ||
                !HexGridManager.Instance._hexObjects.TryGetValue(position, out GameObject hex) ||
                hex == null)
            {
                return null;
            }

            GOList list = hex.GetComponent<GOList>();
            if (list != null && list.TryGetValue("StepsHere", out GameObject registeredStepsHere))
                return registeredStepsHere;

            Transform stepsHere = hex.GetComponentsInChildren<Transform>(true)
                .FirstOrDefault(child => child.name == "StepsHere");
            return stepsHere != null ? stepsHere.gameObject : null;
        }

        public void HexClickCallback(Vector2Int hexPosition)
        {
            _lastHexClickFrame = Time.frameCount;
            Debug.Log("CLICKED " + hexPosition);
            PlayingState playingState = GameStateManager.Instance.GetCurrent<PlayingState>();
            
            if (playingState.CurrentTurn.entityType != EntityType.Player)
                return;

            if (!playingState.AllowUserInput && !isAttacking)
                return;
            
            Dictionary<Vector2Int, int> distanceMap = CalculateDistanceMap(hexPosition, playingState);
            Dictionary<Vector2Int, int> noBlockerMapFromPlayer = HexGridManager.Instance.CalculateDistanceMap(playingState.player.positionRowCol, new List<Vector2Int>());
            
            List<AbstractEntity> entitiesOnHex = new List<AbstractEntity>();
            playingState.EntitiesOnHex(hexPosition, out entitiesOnHex);

            if (TryPlayPendingNonManualAttackPreview(hexPosition, playingState))
                return;

            if (TryPlayPendingCardTarget(hexPosition, playingState))
                return;
            
            // If player is attacking, and the target is within the distance in the event
            // and the tile has an entity and that entity is not the player
            if (isAttacking)
            {
                bool validAttackTarget =
                    ToAttack.Count > 0 &&
                    noBlockerMapFromPlayer.TryGetValue(hexPosition, out int distanceFromPlayer) &&
                    distanceFromPlayer >= 0 &&
                    distanceFromPlayer <= ToAttack[0].distance &&
                    entitiesOnHex.Count > 0 &&
                    entitiesOnHex.Exists(playingState.IsPlayerAttackTarget);

                if (!validAttackTarget)
                    return;

                if (_pendingCard != null && !_pendingCardHasStarted)
                {
                    if (!_pendingCard.TryStartManualAttackPlay(out List<AttackCardEvent> attackCardEvents))
                    {
                        ClearPendingAttacks();
                        return;
                    }

                    ToAttack.Clear();
                    ToAttack.AddRange(attackCardEvents);
                    _pendingCardHasStarted = true;

                    if (ToAttack.Count == 0)
                    {
                        ClearToAttackEmitters();
                        FinishPendingCardAttack();
                        playingState.CaptureFinish();
                        return;
                    }
                }

                AttackCardEvent queuedAttack = ToAttack[0];
                AbstractEntity targetEntity = entitiesOnHex.Find(playingState.IsPlayerAttackTarget);
                ResolveManualAttackAgainstTarget(queuedAttack, targetEntity, playingState);
                
                // Reset situation
                SpriteArrowManager.Instance.DestroyArrow(arrowUUID);
                ResolveCurrentAttack();
                ClearToAttackEmitters();

                while (TryResolveNextInheritedManualAttack(queuedAttack, targetEntity, playingState))
                {
                    queuedAttack = ToAttack[0];
                    ResolveCurrentAttack();
                }

                if (ToAttack.Count > 0)
                {
                    SetupAttack();
                }
                else
                {
                    FinishPendingCardAttack();
                }

                playingState.CaptureFinish();
                return;
            }
            
            // If player exists, and has enough steps, and is not already moving
            if (!playingState.IsPlayerMovementBlocked &&
                playingState.CanPlayerMove &&
                distanceMap.ContainsKey(playingState.player.positionRowCol) &&
                distanceMap[playingState.player.positionRowCol] <= RunInfo.Instance.CurrentSteps && !isMoving)
            {
                StartCoroutine(MovePlayer(distanceMap, playingState, hexPosition));
            }
            
            playingState.CaptureFinish();
        }

        private bool TryPlayPendingCardTarget(Vector2Int hexPosition, PlayingState playingState)
        {
            if (_pendingTargetCard == null)
                return false;

            if (!_pendingTargetPositions.Contains(hexPosition))
                return true;

            CardMonobehaviour card = _pendingTargetCard;
            if (!CardTargetResolver.TryResolveSelectionForClick(
                    card,
                    card.Card,
                    playingState.player,
                    playingState,
                    hexPosition,
                    out TargetSelection selection))
            {
                return true;
            }

            ClearPendingCardTargeting(false);
            ClearToAttackEmitters();

            if (!card.TryPlayWithTargets(selection))
            {
                ClearToAttackEmitters();
                return true;
            }

            SpriteArrowManager.Instance.DestroyArrow(arrowUUID);
            UpdateMovableParticles(playingState);
            playingState.CaptureFinish();
            return true;
        }

        private void HandleSelectedCardMissClick()
        {
            if (!Input.GetMouseButtonDown(0))
                return;

            if (_lastHexClickFrame == Time.frameCount)
                return;

            if (GameStateManager.Instance == null || !GameStateManager.Instance.IsCurrent<PlayingState>())
                return;

            if (!HasSelectedHandCard())
                return;

            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            Deck.Instance.SetHandToUnused();
        }

        private bool HasSelectedHandCard()
        {
            if (Deck.Instance == null)
                return false;

            foreach (CardMonobehaviour card in Deck.Instance.Hand)
            {
                if (card != null && card.used && !card.played)
                    return true;
            }

            return false;
        }

        private bool TryResolveNextInheritedManualAttack(
            AttackCardEvent previousAttack,
            AbstractEntity target,
            PlayingState playingState)
        {
            if (ToAttack.Count == 0 || !ShouldInheritManualAttackTarget(previousAttack, ToAttack[0]))
                return false;

            ResolveManualAttackAgainstTarget(ToAttack[0], target, playingState);
            return true;
        }

        private bool ShouldInheritManualAttackTarget(AttackCardEvent previousAttack, AttackCardEvent nextAttack)
        {
            if (previousAttack == null || nextAttack == null)
                return false;

            if (ReferenceEquals(previousAttack, nextAttack))
                return true;

            return previousAttack.PreviewSourceActionIndex >= 0 &&
                   previousAttack.PreviewSourceActionIndex == nextAttack.PreviewSourceActionIndex;
        }

        private void ResolveManualAttackAgainstTarget(
            AttackCardEvent queuedAttack,
            AbstractEntity target,
            PlayingState playingState)
        {
            if (queuedAttack == null || target == null || target.Health <= 0)
                return;

            Vector2Int targetPosition = target.positionRowCol;
            AttackCardEvent attack = GetIncomingModifiedAttack(
                queuedAttack,
                playingState,
                targetPosition,
                false);

            CardEventResult result = playingState.DamageEntities(targetPosition, attack.amount, attack.status, queuedAttack);
            _pendingCard?.ActivateManualAttackFollowUps(queuedAttack, target, result);
            AttackCardEvent.PlayAttackHitFx(playingState, targetPosition);
            RangedStatus.ConsumeAfterAttack(playingState.player);

            ApplyAttackNudge(playingState.player.transform, target.transform.position);
        }

        private AttackCardEvent GetIncomingModifiedAttack(
            AttackCardEvent attack,
            PlayingState playingState,
            Vector2Int targetPosition,
            bool previewMode)
        {
            AttackCardEvent attackToModify = previewMode ? attack.Copy() : attack;
            List<AbstractCardEvent> modifiedEvents = CardEventPipeline.ApplyIncomingTileModifiersForTarget(
                new List<AbstractCardEvent> { attackToModify },
                playingState.player,
                targetPosition,
                previewMode);

            foreach (AbstractCardEvent cardEvent in modifiedEvents)
            {
                if (cardEvent is AttackCardEvent modifiedAttack)
                    return modifiedAttack;
            }

            return attackToModify;
        }

        private bool TryPlayPendingNonManualAttackPreview(Vector2Int hexPosition, PlayingState playingState)
        {
            if (_pendingNonManualAttackPreviewCard == null)
                return false;

            if (!_pendingNonManualAttackPreviewPositions.Contains(hexPosition))
                return false;

            CardMonobehaviour card = _pendingNonManualAttackPreviewCard;
            ClearPendingNonManualAttackPreview();

            if (card == null)
            {
                ClearToAttackEmitters();
                return true;
            }

            if (!card.TryPlayNonManualAttackPreview())
            {
                ClearToAttackEmitters();
                return true;
            }

            ClearToAttackEmitters();
            UpdateMovableParticles(playingState);
            playingState.CaptureFinish();
            return true;
        }

        public Dictionary<Vector2Int, int> CalculateDistanceMap(Vector2Int hexPosition, PlayingState playingState)
        {
            List<Vector2Int> blockers = new List<Vector2Int>();
            
            foreach (AbstractEntity abstractEntity in playingState.GetEntities())
            {
                if (abstractEntity.entityType == EntityType.Player)
                    continue;
                
                blockers.Add(abstractEntity.positionRowCol);
            }
            
            Dictionary<Vector2Int, int> distanceMap = HexGridManager.Instance.CalculateDistanceMap(
                hexPosition,
                blockers,
                includeElevationCosts: true);
            
            foreach (Vector2Int pos in distanceMap.Keys)
            {
                HexGridManager.Instance.GetWorldHexObject(pos).transform.GetChild(5).GetComponent<TextMeshPro>().SetText("" + distanceMap[pos]);
            }
            
            return distanceMap;
        }

        private void ApplyAttackNudge(Transform attacker, Vector3 targetWorldPosition)
        {
            Vector3 worldOffset = (targetWorldPosition - attacker.position).normalized * 0.5f;
            attacker.localPosition += attacker.parent != null
                ? attacker.parent.InverseTransformVector(worldOffset)
                : worldOffset;
        }

        IEnumerator MovePlayer(Dictionary<Vector2Int, int> distanceMap, PlayingState playingState, Vector2Int targetPosition)
        {
            playingState.AllowUserInput = false;
            isMoving = true;
            Dictionary<Vector2Int, int> currentMap = CalculateDistanceMap(targetPosition, playingState);

            // While we are walking to point
            while (currentMap[playingState.player.positionRowCol] != 0)
            {
                List<Vector2Int> positions =  HexGridManager.AdjacentHexes(playingState.player.positionRowCol);

                if (RunInfo.Instance.CurrentSteps == 0)
                {
                    break;
                }
                
                bool moved = false;
                bool tileMovedPlayer = false;
                foreach (Vector2Int pos in positions)
                {
                    Vector2Int currentPosition = playingState.player.positionRowCol;
                    int movementCost = HexGridManager.Instance.GetMovementCost(currentPosition, pos);

                    if (currentMap.ContainsKey(pos) &&
                        currentMap.TryGetValue(currentPosition, out int currentDistance) &&
                        currentMap[pos] + movementCost == currentDistance &&
                        RunInfo.Instance.CurrentSteps >= movementCost)
                    {
                        if (playingState.MoveEntity(playingState.player, pos)) 
                        {
                            yield return new WaitForSeconds(0.3f * (1/GameplayNavSettings.speed));
                            moved = true;
                            RunInfo.Instance.CurrentSteps -= movementCost;

                            tileMovedPlayer = playingState.player.positionRowCol != pos;
                            break;
                        }

                    }
                }
                
                if (!moved) 
                {
                    break;
                }

                if (tileMovedPlayer)
                {
                    break;
                }

                currentMap = CalculateDistanceMap(targetPosition, playingState);
            }
            isMoving = false;
            playingState.AllowUserInput = true;
            UpdateMovableParticles(playingState);
        }
    }
}
