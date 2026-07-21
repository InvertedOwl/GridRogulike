using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cards.Actions;
using Cards.CardEvents;
using Entities;
using Entities.Enemies;
using Grid;
using Passives;
using ScriptableObjects;
using Serializer;
using Types.CardRestrictions;
using Types.Passives;
using Types.Statuses;
using Types.Tiles;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Util;

namespace StateManager
{
    public class PlayingState : GameState
    {
        public enum TurnPhase
        {
            None,
            Move,
            Card,
            Enemy
        }

        private bool _allowUserInput = true;
        

        public bool AllowUserInput
        {
            get => _allowUserInput;
            set
            {
                bool changed = _allowUserInput != value;
                _allowUserInput = value;
                Deck.Instance.SetInactive(!value, false);
                EndTurnButton.interactable = value;
                RedrawButton.interactable = value;

                Deck.Instance.UpdatePlayability();

                foreach (Vector2Int hex in HexGridManager.Instance.BoardDictionary.Keys)
                {
                    HexGridManager.Instance.GetWorldHexObject(hex).GetComponent<HexPreviewHandler>().DisablePreview =
                        !value;
                }   
            }
        }

        public readonly List<AbstractEntity> entities = new();
        private HexGridManager _grid = null!;
        public Player player;
        public GameObject gameUI;
        public EnemiesData enemiesData;
        protected RandomState random;
        
        public void Start()
        {
            random = RunInfo.NewRandom("playing");
        }
        
        public Button EndTurnButton;
        public Button RedrawButton;
        
        public static int RewardMoney;
        public static EncounterData encounterData;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ResetStaticsOnLoad()
        {
            ResetStatics();
        }

        public static void ResetStatics()
        {
            RewardMoney = 0;
            encounterData = null;
            MapProgressLayer = 0;
            MapProgressLayerCount = 1;
        }

        public static int MapProgressLayer;
        public static int MapProgressLayerCount = 1;
        
        public EasePosition TurnIndicator;

        public GOList GoList;

        
        public TurnIndicatorManager turnIndicatorManager;

        [Header("Turn Phase Events")]
        public UnityEvent MovePhaseActivatedEvent;
        public UnityEvent CardPhaseActivatedEvent;
        public UnityEvent EnemyTurnPhaseActivatedEvent;
        
        private readonly List<int> _turnOrder = new();
        private int _currentTurnIndex;
        public AbstractEntity CurrentTurn => entities[_turnOrder[_currentTurnIndex]];
        public TurnPhase CurrentTurnPhase { get; private set; } = TurnPhase.None;
        public bool IsMovePhaseActive => CurrentTurnPhase == TurnPhase.Move;
        public bool IsCardPhaseActive => CurrentTurnPhase == TurnPhase.Card;
        public bool IsEnemyTurnPhaseActive => CurrentTurnPhase == TurnPhase.Enemy;
        public bool CanPlayerMove => IsMovePhaseActive && AllowUserInput && IsPlayerTurnActive();
        public bool CanPlayerPlayCards => IsCardPhaseActive && AllowUserInput && IsPlayerTurnActive();
        private float _autoEndReadyTime = -1f;
        private const float AutoEndDelaySeconds = 0.5f;
        private Coroutine _finishCoroutine;
        [SerializeField] private float deadEnemyCleanupScaleDuration = 0.2f;
        [SerializeField] private float disabledTileOpacity = 0.6f;
        private readonly HashSet<Vector2Int> _tilesUsedThisTurn = new();
        private readonly HashSet<Vector2Int> _tilesUsedThisCombat = new();
        private readonly HashSet<NonPlayerEntity> _enemiesNeedingIntentRefresh = new();
        private readonly Dictionary<AbstractEntity, Vector2Int> _enemyPlanningPositions = new();
        private readonly List<ICardPlayRestriction> _temporaryCardPlayRestrictions = new();
        private readonly List<ICardPlayRestriction> _combatCardPlayRestrictions = new();
        private int _cardRestrictionVersion;
        private bool _playerMovementBlockedThisTurn;
        private bool _playerMovementBlockedThisCombat;
        private RunInfo _subscribedRunInfo;
        private readonly Dictionary<Vector2Int, TileCountdownRuntimeState> _tileCountdownStates = new();
        private List<TileCountdownSaveData> _loadedTileCountdownStates;
        public int PlayerMovesThisTurn { get; private set; }

        private class TileCountdownRuntimeState
        {
            public int turnsRemaining;
            public bool exploded;
            public bool iconCleared;
        }

        [Header("Phases")] 
        [SerializeField] private EasePosition YourTurn;
        [SerializeField] private EasePosition EnemyTurn;
        [SerializeField] private EasePosition MovePhase;
        [SerializeField] private EasePosition CardPhase;

        [Header("Enemy Scaling")]
        [Min(0)]
        [SerializeField] private int enemyScalingStartLayer = 1;
        [Min(0f)]
        [SerializeField] private float enemyHealthScalePerMapLayer = 0.15f;
        [Min(0f)]
        [SerializeField] private float enemyDamageScalePerMapLayer = 0.1f;

        public EaseScale playingUI;
        public EasePosition playingHealth;
        
        public override void Enter()
        {
            PlayWindowInSound();
            playingHealth.targetLocation = new Vector3(0, 0, 0);
            Debug.Log("Save is  " + SaveData);
            if (SaveData != null)
            {
                PlayingStateSaveData saveData = (PlayingStateSaveData) SaveData;
                encounterData = saveData.encounterData;
                MapProgressLayer = saveData.mapProgressLayer;
                MapProgressLayerCount = Mathf.Max(1, saveData.mapProgressLayerCount);
                _loadedTileCountdownStates = saveData.tileCountdownStates;
                SaveData = null;
            }
            
            if (random == null)
            {
                random = RunInfo.NewRandom("playing");
            }
            
            MoveEntitiesIn();
            InitializeDeckAndGrid();
            SetupInitialTiles();
            RestoreOrInitializeTileCountdownStates();
            ResetCombatTileTriggers();
            SpawnEncounterEnvironmentPassives();
            RunInfo.Instance.CurrentSteps = 0;
            SubscribeToRunInfoEvents();
            _enemyPlanningPositions.Clear();
            SetupEntities();
            RebuildTurnOrder();
            SetupUI();
            _currentTurnIndex = -1;
            EnableTileHovers();
            UpdateNextTurnAttacks();
            SetupPlayerHand();

            playingUI.SetScale(Vector3.one);
            
            
            turnIndicatorManager.Rebuild(entities, _turnOrder);
            StartCoroutine(WaitFrame());
            StartEntityTurn();
            // TurnIndicator.SendToLocation(new Vector3(0, 0, 0));
            BattleStats.ResetStatsBattle();
            
            HexGridManager.Instance.RegisterHexClickCallback(HexClickPlayerController.StaticHexClickCallback);
            HexGridManager.Instance.RegisterHexHoverEnterCallback(HexClickPlayerController.StaticHexHoverOnCallback);
            HexGridManager.Instance.RegisterHexHoverExitCallback(HexClickPlayerController.StaticHexHoverOffCallback);

            
            foreach (var e in entities)
            {
                StartCoroutine(WaitFrameMove(e));
                

                if (e.entityType != EntityType.Player)
                    e.Health = e.initialHealth;
            }

        }

        IEnumerator WaitFrameMove(AbstractEntity e)
        {
            yield return new WaitForEndOfFrame();
            e.MoveEntity(e.positionRowCol);
        }

        IEnumerator WaitFrame()
        {
            yield return new WaitForFixedUpdate();
            turnIndicatorManager.SetCurrentTurn(_turnOrder, entities, 0);
        }
        
        // TODO: Add speed stuff so you can have more turns more often
        private void RebuildTurnOrder()
        {
            _turnOrder.Clear();

            for (int i = 0; i < entities.Count; i++)
                _turnOrder.Add(i);
        }

        public void Update()
        {
            if (!GameStateManager.Instance.IsCurrent<PlayingState>())
                return;

            RefreshMovedEnemyIntents();

            if (CheckForFinish() != "none")
            {
                CaptureFinish();
                return;
            }

            TryAutoEndPlayerTurn();
        }

        private void TryAutoEndPlayerTurn()
        {
            if (!GameplayNavSettings.endturn)
            {
                ResetAutoEndTimer();
                return;
            }

            if (!AllowUserInput)
            {
                ResetAutoEndTimer();
                return;
            }

            if (!IsPlayerTurnActive())
            {
                ResetAutoEndTimer();
                return;
            }

            if (Deck.Instance == null || HexClickPlayerController.instance == null)
            {
                ResetAutoEndTimer();
                return;
            }

            if (IsMovePhaseActive)
            {
                if (HasReachableMove())
                {
                    ResetAutoEndTimer();
                    return;
                }

                ResetAutoEndTimer();
                ActivateCardPhase();
                return;
            }

            if (!IsCardPhaseActive)
            {
                ResetAutoEndTimer();
                return;
            }

            if (HasResolvingCard() || HasPlayableCard() || HasPendingPlayerAttack())
            {
                ResetAutoEndTimer();
                return;
            }

            if (!TryWaitForAutoEndDelay())
                return;

            PlayerEndTurn();
        }

        private bool TryWaitForAutoEndDelay()
        {
            if (_autoEndReadyTime < 0f)
            {
                _autoEndReadyTime = Time.time + AutoEndDelaySeconds;
                return false;
            }

            if (Time.time < _autoEndReadyTime)
                return false;

            if (IsBackgroundAnimationRunning())
                return false;

            ResetAutoEndTimer();
            return true;
        }

        private void ResetAutoEndTimer()
        {
            _autoEndReadyTime = -1f;
        }

        private bool IsBackgroundAnimationRunning()
        {
            return SpawnBG.instance != null && SpawnBG.instance.IsColorAnimationRunning;
        }

        private bool IsPlayerTurnActive()
        {
            if (_turnOrder.Count == 0 || _currentTurnIndex < 0 || _currentTurnIndex >= _turnOrder.Count)
                return false;

            int entityIndex = _turnOrder[_currentTurnIndex];
            return entityIndex >= 0 &&
                   entityIndex < entities.Count &&
                   entities[entityIndex].entityType == EntityType.Player;
        }

        public int GetTurnPhaseSignature()
        {
            return (int)CurrentTurnPhase;
        }

        public void ActivateMovePhase()
        {
            if (!IsPlayerTurnActive())
                return;

            SetTurnPhase(TurnPhase.Move);
        }

        public void ActivateCardPhase()
        {
            if (!IsPlayerTurnActive())
                return;

            SetTurnPhase(TurnPhase.Card);
        }

        private void ActivateEnemyTurnPhase()
        {
            SetTurnPhase(TurnPhase.Enemy, force: true);
        }

        private void SetTurnPhase(TurnPhase turnPhase, bool force = false)
        {
            if (!force && CurrentTurnPhase == turnPhase)
                return;

            CurrentTurnPhase = turnPhase;
            ResetAutoEndTimer();
            RefreshPhaseInputState();

            switch (turnPhase)
            {
                case TurnPhase.Move:
                    OnMovePhaseActivated();
                    MovePhaseActivatedEvent?.Invoke();
                    break;
                case TurnPhase.Card:
                    OnCardPhaseActivated();
                    CardPhaseActivatedEvent?.Invoke();
                    break;
                case TurnPhase.Enemy:
                    OnEnemyTurnPhaseActivated();
                    EnemyTurnPhaseActivatedEvent?.Invoke();
                    break;
            }
        }

        private void RefreshPhaseInputState()
        {
            if (CurrentTurnPhase == TurnPhase.Move)
            {
                Deck.Instance?.SetHandToUnused();
                HexClickPlayerController.instance?.UpdateMovableParticles(this);
            }
            else
            {
                HexClickPlayerController.instance?.ClearMovableParticles();
            }

            Deck.Instance?.MarkPlayabilityDirty();
            Deck.Instance?.UpdatePlayability();
        }

        public void OnAllPhasesEnd()
        {
            YourTurn.targetLocation = new Vector2(0, 50);
            MovePhase.targetLocation = new Vector2(0, 0);
            CardPhase.targetLocation = new Vector2(0, 0);
            EnemyTurn.targetLocation = new Vector2(0, 50);
        }
        
        public virtual void OnMovePhaseActivated()
        {
            YourTurn.targetLocation = new Vector2(0, 0);
            MovePhase.targetLocation = new Vector2(0, -14);
            CardPhase.targetLocation = new Vector2(0, 0);
            EnemyTurn.targetLocation = new Vector2(0, 50);
        }

        public virtual void OnCardPhaseActivated()
        {
            YourTurn.targetLocation = new Vector2(0, 0);
            MovePhase.targetLocation = new Vector2(0, 0);
            CardPhase.targetLocation = new Vector2(0, -14);
            EnemyTurn.targetLocation = new Vector2(0, 50);
        }

        public virtual void OnEnemyTurnPhaseActivated()
        {
            YourTurn.targetLocation = new Vector2(0, 50);
            MovePhase.targetLocation = new Vector2(0, 0);
            CardPhase.targetLocation = new Vector2(0, 0);
            EnemyTurn.targetLocation = new Vector2(0, 0);
        }

        private void QueueEnemyIntentRefreshAfterMove(AbstractEntity movedEntity)
        {
            if (movedEntity is not NonPlayerEntity enemy)
                return;

            enemy.ClearIntentVisuals();

            if (enemy.Health > 0 && IsPlayerTurnActive())
            {
                _enemiesNeedingIntentRefresh.Add(enemy);
            }
        }

        private void RefreshMovedEnemyIntents()
        {
            if (_enemiesNeedingIntentRefresh.Count == 0)
                return;

            if (!IsPlayerTurnActive())
            {
                _enemiesNeedingIntentRefresh.Clear();
                return;
            }

            foreach (NonPlayerEntity enemy in _enemiesNeedingIntentRefresh)
            {
                if (enemy != null && enemy.Health > 0 && entities.Contains(enemy))
                {
                    PlanEnemyNextTurn(enemy, true);
                }
            }

            _enemiesNeedingIntentRefresh.Clear();
        }

        private bool HasResolvingCard()
        {
            return Deck.Instance.Hand.Any(card => card != null && card.played);
        }

        private bool HasPlayableCard()
        {
            if (!CanPlayerPlayCards)
                return false;

            foreach (CardMonobehaviour card in Deck.Instance.Hand)
            {
                if (card == null || card.played || card.onlyDisplay)
                    continue;

                int cost = (int)(card.CostOverride > -1 ? card.CostOverride : card.Card.Cost);
                if (cost <= RunInfo.Instance.CurrentEnergy &&
                    card.CanPlayByRules(out _) &&
                    card.HasPlayableTarget())
                {
                    return true;
                }
            }

            return false;
        }

        private bool HasPendingPlayerAttack()
        {
            HexClickPlayerController controller = HexClickPlayerController.instance;
            return controller.ToAttack.Count > 0 || controller.isAttacking;
        }

        private bool HasReachableMove()
        {
            if (!CanPlayerMove)
                return false;

            if (IsPlayerMovementBlocked)
                return false;

            if (RunInfo.Instance.CurrentSteps <= 0)
                return false;

            List<Vector2Int> blockers = entities
                .Where(entity => entity != null && entity.entityType != EntityType.Player)
                .Select(entity => entity.positionRowCol)
                .ToList();

            Dictionary<Vector2Int, int> distanceMap =
                HexGridManager.Instance.CalculateDistanceMap(player.positionRowCol, blockers);

            return distanceMap.Values.Any(distance =>
                distance > 0 && distance <= RunInfo.Instance.CurrentSteps);
        }
        
        
        private void InitializeDeckAndGrid()
        {
            Deck.Instance.UpdateDeck();
            _grid = HexGridManager.Instance;
        }

        private void SetupEntities()
        {
            entities.Clear();
            entities.Add(player);

            
            entities.ForEach(e =>
            {
                if (e.Health <= 0)
                {
                    e.Die();
                }
            });
            
            entities.RemoveAll(e => e.Health <= 0);
            
            int numNormalEnemy = 0;
            int numHardEnemy = 0;
            int numBossEnemy = 0;

            foreach (string enemyEntry in encounterData.enemies)
            {
                if (enemiesData.Get(enemyEntry).HasValue)
                {
                    EnemiesData.EnemyEntry enemy = enemiesData.Get(enemyEntry).Value;
                    if (enemy.enemyType == EnemyType.Normal)
                        numNormalEnemy += 1;
                    if (enemy.enemyType == EnemyType.Hard)
                        numHardEnemy += 1;
                    if (enemy.enemyType == EnemyType.Boss)
                        numBossEnemy += 1;
                }
                else
                {
                    Debug.Log("Could not fine enemy: " + enemyEntry);
                }
                    

            }

            Debug.Log(random);
            player.positionRowCol = new Vector2Int(0, 0);
            
            var spawnSpots = GetOrderedBoardPositions()
                .Where(p => !entities.Any(e => e.positionRowCol == p))
                .OrderBy(_ => random.Next())
                .ToList();
            
            
            entities.AddRange(SpawnEncounter(spawnSpots));
            ApplyEnemyHealthScaling();
            SpawnActivePassiveEntities();

            // Not enough spawn spots
            if (numNormalEnemy > 0 || numHardEnemy > 0 || numBossEnemy > 0)
            {
                Debug.Log("Not enough spawn spots");
            }
            Debug.Log("Player position: " + player.positionRowCol);
            
            Debug.Log("Player is in " + entities
                .Contains(player));
            
            _enemyPlanningPositions.Clear();
            foreach (AbstractEntity e in entities)
            {
                if (e is NonPlayerEntity nonPlayerEntity)
                {
                    PlanEnemyNextTurn(nonPlayerEntity, true);
                }
            }
        }

        private bool TryGetRandomEmptyHex(RandomState randomState, out Vector2Int pos)
        {
            RandomState positionRandom = randomState ?? random ?? RunInfo.NewRandom("playing");
            var empties = GetOrderedBoardPositions()
                .Where(p => !entities.Any(e => e.positionRowCol == p))
                .OrderBy(_ => positionRandom.Next())
                .ToList();

            if (empties.Count == 0)
            {
                pos = default;
                return false; 
            }

            pos = empties[0];
            return true;
        }

        private List<Vector2Int> GetOrderedBoardPositions()
        {
            return HexGridManager.Instance.BoardDictionary.Keys
                .OrderBy(p => p.x)
                .ThenBy(p => p.y)
                .ToList();
        }

        private List<AbstractEntity> SpawnEncounter(List<Vector2Int> positions)
        {
            List<AbstractEntity> encounter = new List<AbstractEntity>();
            Debug.Log("Positions count " + positions.Count + " enemies: " + encounterData.enemies.Count);

            if (positions.Count < encounterData.enemies.Count)
            {
                throw new Exception("Not enough spawn spots"); // TODO: Handle this
            }
            
            for (int i = 0; i < encounterData.enemies.Count; i++)
            {
                string enemy = encounterData.enemies[i];
                Vector2Int position = positions[i];
                Debug.Log("enemy: " + enemy);
                EnemiesData.EnemyEntry enemyEntry = enemiesData.Get(enemy).Value;
                AbstractEntity abstractEntity = SpawnEntity(enemyEntry, position, false);
                if (abstractEntity != null)
                    encounter.Add(abstractEntity);
                
            }
            return encounter;
        }

        private AbstractEntity SpawnEntity(
            EnemiesData.EnemyEntry enemyEntry,
            Vector2Int position,
            bool placeOnBoardImmediately)
        {
            GameObject enemyObject = Instantiate(enemyEntry.enemyPrefab, GoList.GetValue("board_container").transform);
            AbstractEntity abstractEntity = enemyObject.GetComponent<AbstractEntity>();
            if (abstractEntity == null)
            {
                Debug.LogError($"Spawned entity prefab '{enemyEntry.enemyName}' does not have an AbstractEntity.");
                Destroy(enemyObject);
                return null;
            }

            abstractEntity.positionRowCol = position;
            abstractEntity.Health = abstractEntity.initialHealth;
            if (placeOnBoardImmediately)
                abstractEntity.MoveEntity(position);

            return abstractEntity;
        }

        private int EnemyScalingSteps()
        {
            return Mathf.Max(0, MapProgressLayer - enemyScalingStartLayer + 1);
        }

        private float EnemyHealthMultiplier()
        {
            return 1f + EnemyScalingSteps() * enemyHealthScalePerMapLayer;
        }

        private float EnemyDamageMultiplier()
        {
            return 1f + EnemyScalingSteps() * enemyDamageScalePerMapLayer;
        }

        private void ApplyEnemyHealthScaling()
        {
            float multiplier = EnemyHealthMultiplier();
            if (Mathf.Approximately(multiplier, 1f))
                return;

            foreach (AbstractEntity entity in entities)
            {
                ApplyEnemyHealthScaling(entity, multiplier);
            }
        }

        private void ApplyEnemyHealthScaling(AbstractEntity entity)
        {
            ApplyEnemyHealthScaling(entity, EnemyHealthMultiplier());
        }

        private void ApplyEnemyHealthScaling(AbstractEntity entity, float multiplier)
        {
            if (entity is not NonPlayerEntity || Mathf.Approximately(multiplier, 1f))
                return;

            entity.initialHealth = ScaleEnemyFloat(entity.initialHealth, multiplier);
            entity.Health = entity.initialHealth;
        }

        public List<AbstractCardEvent> ApplyEnemyDamageScaling(List<AbstractCardEvent> events)
        {
            float multiplier = EnemyDamageMultiplier();
            if (Mathf.Approximately(multiplier, 1f))
                return events;

            foreach (AbstractCardEvent cardEvent in events)
            {
                if (cardEvent is AttackCardEvent attackCardEvent)
                {
                    attackCardEvent.amount = ScaleEnemyInt(attackCardEvent.amount, multiplier);
                }
            }

            return events;
        }

        private float ScaleEnemyFloat(float amount, float multiplier)
        {
            if (amount <= 0f)
                return amount;

            return Mathf.Max(1f, Mathf.Round(amount * multiplier));
        }

        private int ScaleEnemyInt(int amount, float multiplier)
        {
            if (amount <= 0)
                return amount;

            return Mathf.Max(1, Mathf.RoundToInt(amount * multiplier));
        }
        
        [Header("Generated Initial Map")]
        [SerializeField] private bool generateInitialMap = true;
        [SerializeField] private int initialMapRadius = 2;
        [SerializeField] private string homeTileType = "start";
        [SerializeField] private string generatedTileType = "basic";
        [SerializeField, Min(0.001f)] private float tileHeightNoiseScale = 0.35f;

        public List<MapData> maps = new List<MapData>();

        private void SetupInitialTiles()
        {
            var origin = new Vector2Int(0, 0);
            RandomState noiseSeed = RunInfo.NewRandom("tile-height-noise");
            RandomState noiseOffsetRandom = new RandomState(noiseSeed.seed);
            Vector2 noiseOffset = new Vector2(
                (float)noiseOffsetRandom.NextDouble() * 10000f,
                (float)noiseOffsetRandom.NextDouble() * 10000f);

            if (generateInitialMap)
            {
                foreach (Vector2Int position in HexGridManager.HexesInRadius(initialMapRadius))
                {
                    TryAddTileWithNoiseHeight(
                        position,
                        position == origin ? homeTileType : generatedTileType,
                        noiseOffset);
                }
            }

            foreach (MapData map in maps)
            {
                foreach (MapEntry mapEntry in map.entries)
                {
                    TryAddTileWithNoiseHeight(mapEntry.key, mapEntry.value, noiseOffset);
                }
            }
            
            _grid.UpdateBoard();
        }

        private void TryAddTileWithNoiseHeight(
            Vector2Int position,
            string tileType,
            Vector2 noiseOffset)
        {
            if (_grid.BoardDictionary.ContainsKey(position))
                return;

            Vector2 center = HexGridManager.GetHexCenter(position.x, position.y);
            float noise = Mathf.PerlinNoise(
                center.x * tileHeightNoiseScale + noiseOffset.x,
                center.y * tileHeightNoiseScale + noiseOffset.y);
            int height = Mathf.Min(2, Mathf.FloorToInt(noise * 3f));

            _grid.TryAdd(position, tileType, height);
        }

        private void RestoreOrInitializeTileCountdownStates()
        {
            _tileCountdownStates.Clear();

            if (_loadedTileCountdownStates != null)
            {
                foreach (TileCountdownSaveData saveData in _loadedTileCountdownStates)
                {
                    if (!TryGetTileCountdownEffect(saveData.position, out _))
                        continue;

                    _tileCountdownStates[saveData.position] = new TileCountdownRuntimeState
                    {
                        turnsRemaining = saveData.turnsRemaining,
                        exploded = saveData.exploded,
                        iconCleared = saveData.iconCleared
                    };
                }

                _loadedTileCountdownStates = null;
            }

            SynchronizeTileCountdownStates();
            ApplyAllTileCountdownIcons();
        }

        private void SynchronizeTileCountdownStates()
        {
            if (HexGridManager.Instance == null)
                return;

            List<Vector2Int> stalePositions = new List<Vector2Int>();
            foreach (Vector2Int position in _tileCountdownStates.Keys)
            {
                if (!TryGetTileCountdownEffect(position, out _))
                    stalePositions.Add(position);
            }

            foreach (Vector2Int stalePosition in stalePositions)
            {
                _tileCountdownStates.Remove(stalePosition);
            }

            foreach (KeyValuePair<Vector2Int, string> boardTile in HexGridManager.Instance.BoardDictionary)
            {
                if (!TileData.tiles.TryGetValue(boardTile.Value, out TileEntry tile) || tile.countdownEffect == null)
                    continue;

                if (!_tileCountdownStates.ContainsKey(boardTile.Key))
                {
                    _tileCountdownStates[boardTile.Key] = new TileCountdownRuntimeState
                    {
                        turnsRemaining = tile.countdownEffect.startTurns
                    };
                }
            }
        }

        private void ApplyAllTileCountdownIcons()
        {
            foreach (Vector2Int position in _tileCountdownStates.Keys.ToList())
            {
                ApplyTileCountdownIcon(position);
            }
        }

        private void ApplyTileCountdownIcon(Vector2Int position)
        {
            if (!TryGetTileCountdownEffect(position, out TileCountdownEffect countdownEffect) ||
                !_tileCountdownStates.TryGetValue(position, out TileCountdownRuntimeState state))
            {
                return;
            }

            string icon = state.iconCleared
                ? countdownEffect.inactiveIcon
                : state.exploded
                    ? countdownEffect.explosionIcon
                    : countdownEffect.IconForTurnsRemaining(state.turnsRemaining);

            HexGridManager.Instance.SetHexIcon(position, icon);
        }

        private bool TryGetTileCountdownEffect(Vector2Int position, out TileCountdownEffect countdownEffect)
        {
            countdownEffect = null;

            if (HexGridManager.Instance == null ||
                !HexGridManager.Instance.BoardDictionary.TryGetValue(position, out string tileId) ||
                !TileData.tiles.TryGetValue(tileId, out TileEntry tile))
            {
                return false;
            }

            countdownEffect = tile.countdownEffect;
            return countdownEffect != null;
        }

        private void TickTileCountdowns()
        {
            SynchronizeTileCountdownStates();

            foreach (Vector2Int position in _tileCountdownStates.Keys.ToList())
            {
                if (!TryGetTileCountdownEffect(position, out TileCountdownEffect countdownEffect) ||
                    !_tileCountdownStates.TryGetValue(position, out TileCountdownRuntimeState state))
                {
                    continue;
                }

                if (state.iconCleared)
                    continue;

                if (state.exploded)
                {
                    state.iconCleared = true;
                    ApplyTileCountdownIcon(position);
                    continue;
                }

                state.turnsRemaining--;

                if (state.turnsRemaining <= 0)
                {
                    state.turnsRemaining = 0;
                    state.exploded = true;
                    PlayTileCountdownExplosionFx(position);
                    DamageEntities(position, countdownEffect.explosionDamage, null);
                }

                ApplyTileCountdownIcon(position);
            }
        }

        private void PlayTileCountdownExplosionFx(Vector2Int position)
        {
            if (FXManager.Instance == null)
                return;
            
            Debug.Log("Playing countdown explosive effect");
            Vector3 spawnPosition = HexGridManager.GetHexCenter(position.x, position.y);
            if (HexGridManager.Instance != null &&
                HexGridManager.Instance._hexObjects.TryGetValue(position, out GameObject hexObject) &&
                hexObject != null)
            {
                spawnPosition = hexObject.transform.position;
            }

            FXManager.Instance.TryPlay("NukeExplosionFire", spawnPosition);
        }

        private List<TileCountdownSaveData> CaptureTileCountdownStates()
        {
            SynchronizeTileCountdownStates();

            List<TileCountdownSaveData> saveData = new List<TileCountdownSaveData>();
            foreach (KeyValuePair<Vector2Int, TileCountdownRuntimeState> kvp in _tileCountdownStates)
            {
                saveData.Add(new TileCountdownSaveData
                {
                    position = kvp.Key,
                    turnsRemaining = kvp.Value.turnsRemaining,
                    exploded = kvp.Value.exploded,
                    iconCleared = kvp.Value.iconCleared
                });
            }

            return saveData;
        }

        private void ResetCombatTileTriggers()
        {
            _tilesUsedThisTurn.Clear();
            _tilesUsedThisCombat.Clear();
            _temporaryCardPlayRestrictions.Clear();
            _combatCardPlayRestrictions.Clear();
            _playerMovementBlockedThisTurn = false;
            _playerMovementBlockedThisCombat = false;
            PlayerMovesThisTurn = 0;
            _cardRestrictionVersion++;
            UpdateAllTileDisableVisuals();
        }

        public void ResetTurnTileTriggers()
        {
            _tilesUsedThisTurn.Clear();
            _temporaryCardPlayRestrictions.Clear();
            _playerMovementBlockedThisTurn = false;
            PlayerMovesThisTurn = 0;
            _cardRestrictionVersion++;
            UpdateAllTileDisableVisuals();
        }

        public bool IsPlayerMovementBlockedThisTurn => _playerMovementBlockedThisTurn;
        public bool IsPlayerMovementBlockedThisCombat => _playerMovementBlockedThisCombat;
        public bool IsPlayerMovementBlocked => _playerMovementBlockedThisTurn || _playerMovementBlockedThisCombat;

        public void BlockPlayerMovementForTurn()
        {
            _playerMovementBlockedThisTurn = true;
            AddTemporaryCardPlayRestriction(
                new GeneratedEventCardPlayRestriction(
                    RestrictedCardEventKind.Movement,
                    "Cannot move this turn."));
            RefreshPlayerMovementBlocked();
        }

        public void BlockPlayerMovementForCombat()
        {
            _playerMovementBlockedThisCombat = true;
            AddCombatCardPlayRestriction(
                new GeneratedEventCardPlayRestriction(
                    RestrictedCardEventKind.Movement,
                    "Cannot move this combat."));
            RefreshPlayerMovementBlocked();
        }

        private void RefreshPlayerMovementBlocked()
        {
            if (RunInfo.Instance != null)
                RunInfo.Instance.CurrentSteps = 0;

            HexClickPlayerController.instance?.UpdateMovableParticles(this);
            Deck.Instance?.MarkPlayabilityDirty();
        }

        public void AddTemporaryCardPlayRestriction(ICardPlayRestriction restriction)
        {
            AddCardPlayRestriction(restriction, _temporaryCardPlayRestrictions);
        }

        public void AddCombatCardPlayRestriction(ICardPlayRestriction restriction)
        {
            AddCardPlayRestriction(restriction, _combatCardPlayRestrictions);
        }

        private void AddCardPlayRestriction(
            ICardPlayRestriction restriction,
            List<ICardPlayRestriction> restrictionList)
        {
            if (restriction == null)
                return;

            restrictionList.Add(restriction);
            _cardRestrictionVersion++;
            Deck.Instance?.MarkPlayabilityDirty();
        }

        public List<ICardPlayRestriction> GetActiveCardPlayRestrictions()
        {
            List<ICardPlayRestriction> restrictions = new List<ICardPlayRestriction>();

            if (player?.statusManager != null)
            {
                foreach (AbstractStatus status in player.statusManager.statusList)
                {
                    if (status is ICardPlayRestriction restriction)
                        restrictions.Add(restriction);
                }
            }

            restrictions.AddRange(_temporaryCardPlayRestrictions);
            restrictions.AddRange(_combatCardPlayRestrictions);
            return restrictions;
        }

        public int GetCardPlayRestrictionSignature()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + PlayerMovesThisTurn;
                hash = hash * 31 + _cardRestrictionVersion;

                if (player?.statusManager != null)
                {
                    foreach (AbstractStatus status in player.statusManager.statusList)
                    {
                        if (status is not ICardPlayRestriction)
                            continue;

                        hash = hash * 31 + status.GetType().GetHashCode();
                        hash = hash * 31 + status.Amount;
                    }
                }

                return hash;
            }
        }

        public void TriggerPlayerTileLand(Vector2Int position, AbstractEntity entity)
        {
            if (entity == null || entity.entityType != EntityType.Player)
                return;

            string tileId = HexGridManager.Instance.HexType(position);
            if (!TileData.tiles.TryGetValue(tileId, out TileEntry tile))
                return;

            if (!CanTriggerTile(position, tile))
                return;

            if (!tile.triggerEvents.Keys.Contains(TriggerEventTime.Land))
                return;
            
            MarkTileTriggered(position, tile);
            TileContext context = new TileContext(position, tileId, entity, false);
            CardEventPipeline.Activate(tile.triggerEvents[TriggerEventTime.Land].Invoke(context), entity);
        }
        
        public void TriggerPlayerTileStart(Vector2Int position, AbstractEntity entity)
        {
            if (entity == null || entity.entityType != EntityType.Player)
                return;

            string tileId = HexGridManager.Instance.HexType(position);
            if (!TileData.tiles.TryGetValue(tileId, out TileEntry tile))
                return;

            if (!CanTriggerTile(position, tile))
                return;

            if (!tile.triggerEvents.Keys.Contains(TriggerEventTime.StartTurn))
                return;
            
            MarkTileTriggered(position, tile);
            TileContext context = new TileContext(position, tileId, entity, false);
            CardEventPipeline.Activate(tile.triggerEvents[TriggerEventTime.StartTurn].Invoke(context), entity);
        }
        
        public void TriggerPlayerTileEnd(Vector2Int position, AbstractEntity entity)
        {
            if (entity == null || entity.entityType != EntityType.Player)
                return;

            string tileId = HexGridManager.Instance.HexType(position);
            if (!TileData.tiles.TryGetValue(tileId, out TileEntry tile))
                return;

            if (!CanTriggerTile(position, tile))
                return;

            if (!tile.triggerEvents.Keys.Contains(TriggerEventTime.EndTurn))
                return;
            
            MarkTileTriggered(position, tile);
            TileContext context = new TileContext(position, tileId, entity, false);
            CardEventPipeline.Activate(tile.triggerEvents[TriggerEventTime.EndTurn].Invoke(context), entity);
        }

        public bool CanTriggerTile(Vector2Int position, TileEntry tile)
        {
            return tile.triggerLimit switch
            {
                TileTriggerLimit.OncePerTurn => !_tilesUsedThisTurn.Contains(position),
                TileTriggerLimit.OncePerCombat => !_tilesUsedThisCombat.Contains(position),
                _ => true
            };
        }

        public void MarkTileTriggered(Vector2Int position, TileEntry tile)
        {
            if (tile.triggerLimit == TileTriggerLimit.OncePerTurn)
                _tilesUsedThisTurn.Add(position);
            else if (tile.triggerLimit == TileTriggerLimit.OncePerCombat)
                _tilesUsedThisCombat.Add(position);
            else
                return;

            UpdateTileDisableVisual(position);
        }

        private void UpdateAllTileDisableVisuals()
        {
            if (HexGridManager.Instance == null)
                return;

            foreach (Vector2Int position in HexGridManager.Instance.BoardDictionary.Keys)
            {
                UpdateTileDisableVisual(position);
            }
        }

        private void UpdateTileDisableVisual(Vector2Int position)
        {
            if (HexGridManager.Instance == null ||
                !HexGridManager.Instance.BoardDictionary.ContainsKey(position) ||
                !HexGridManager.Instance._hexObjects.TryGetValue(position, out GameObject hexObject) ||
                hexObject == null)
            {
                return;
            }

            GOList goList = hexObject.GetComponent<GOList>();
            if (goList == null)
                goList = hexObject.GetComponentInChildren<GOList>(true);

            if (goList == null || !goList.HasValue("Disable"))
                return;

            GameObject disableObject = goList.GetValue("Disable");
            if (disableObject == null)
                return;

            EaseColor easeColor = disableObject.GetComponent<EaseColor>();
            if (easeColor == null)
                easeColor = disableObject.GetComponentInChildren<EaseColor>(true);

            if (easeColor == null)
                return;

            disableObject.SetActive(true);
            Color color = easeColor.targetColor;
            color.a = IsTileDisabled(position) ? disabledTileOpacity : 0f;
            easeColor.SendToColor(color);
        }

        private bool IsTileDisabled(Vector2Int position)
        {
            string tileId = HexGridManager.Instance.HexType(position);
            if (!TileData.tiles.TryGetValue(tileId, out TileEntry tile))
                return false;

            return tile.triggerLimit switch
            {
                TileTriggerLimit.OncePerTurn => _tilesUsedThisTurn.Contains(position),
                TileTriggerLimit.OncePerCombat => _tilesUsedThisCombat.Contains(position),
                _ => false
            };
        }

        private void SetupUI()
        {
            // gameUI.GetComponent<LerpPosition>().targetLocation = new Vector2(0, 0);
        }

        private void SetupPlayerHand()
        {
            Deck.Instance.DiscardHand();
        }

        private void SpawnEncounterEnvironmentPassives()
        {
            if (encounterData?.environmentPassives == null || EnvironmentManager.instance == null)
                return;

            foreach (string passiveName in encounterData.environmentPassives)
            {
                if (string.IsNullOrWhiteSpace(passiveName))
                    continue;

                if (!PassiveData.TryGetPassiveEntry(passiveName, out var passiveEntry))
                {
                    Debug.LogError($"Encounter references unknown environment passive: {passiveName}");
                    continue;
                }

                AddEnvironmentPassive(passiveEntry, false);
            }
        }

        public void AddEnvironmentPassive(PassiveEntry entry)
        {
            AddEnvironmentPassive(entry, true);
        }

        private void AddEnvironmentPassive(PassiveEntry entry, bool spawnEntities)
        {
            if (entry == null || EnvironmentManager.instance == null)
                return;

            EnvironmentManager.instance.AddPassive(entry);

            if (!spawnEntities)
                return;

            if (SpawnPassiveEntities(entry))
                RefreshTurnOrderAfterEntitySpawn();
        }

        private void SpawnActivePassiveEntities()
        {
            if (EnvironmentManager.instance == null)
                return;

            foreach (PassiveEntry entry in EnvironmentManager.instance.GetPassiveEntries())
            {
                SpawnPassiveEntities(entry);
            }
        }

        private bool SpawnPassiveEntities(PassiveEntry entry)
        {
            if (entry?.EntitySpawns == null || entry.EntitySpawns.Count == 0)
                return false;

            bool spawnedAny = false;
            RandomState spawnRandom = RunInfo.NewRandom($"passive_entity_spawn:{entry.Name}");

            foreach (PassiveEntitySpawn entitySpawn in entry.EntitySpawns)
            {
                if (entitySpawn == null || entitySpawn.Count <= 0)
                    continue;

                for (int i = 0; i < entitySpawn.Count; i++)
                {
                    if (TrySpawnPassiveEntity(entitySpawn.EntityName, spawnRandom, out AbstractEntity spawnedEntity))
                    {
                        ApplyEnemyHealthScaling(spawnedEntity);
                        spawnedAny = true;
                    }
                }
            }

            return spawnedAny;
        }

        private bool TrySpawnPassiveEntity(string entityName, RandomState spawnRandom, out AbstractEntity spawnedEntity)
        {
            spawnedEntity = null;

            if (string.IsNullOrWhiteSpace(entityName))
                return false;

            if (enemiesData == null)
            {
                Debug.LogError($"Cannot spawn passive entity '{entityName}' because enemiesData is not set.");
                return false;
            }

            EnemiesData.EnemyEntry? enemyEntry = enemiesData.Get(entityName);
            if (!enemyEntry.HasValue)
            {
                Debug.LogError($"Passive references unknown entity '{entityName}'.");
                return false;
            }

            if (!TryGetRandomEmptyHex(spawnRandom, out Vector2Int position))
            {
                Debug.LogWarning($"No empty hex available to spawn passive entity '{entityName}'.");
                return false;
            }

            spawnedEntity = SpawnEntity(enemyEntry.Value, position, true);
            if (spawnedEntity != null)
                entities.Add(spawnedEntity);

            return spawnedEntity != null;
        }

        private void RefreshTurnOrderAfterEntitySpawn()
        {
            RebuildTurnOrder();
            _currentTurnIndex = Mathf.Clamp(_currentTurnIndex, -1, _turnOrder.Count - 1);
            turnIndicatorManager.Rebuild(entities, _turnOrder);

            if (_currentTurnIndex >= 0)
                turnIndicatorManager.SetCurrentTurn(_turnOrder, entities, _currentTurnIndex);

            _enemyPlanningPositions.Clear();
            foreach (AbstractEntity entity in entities)
            {
                if (entity is NonPlayerEntity enemy && enemy.Health > 0)
                {
                    PlanEnemyNextTurn(enemy, IsPlayerTurnActive());
                }
            }
        }

        private void EnableTileHovers()
        {
            foreach (Transform tile in HexGridManager.Instance.grid.transform)
            {
                tile.GetComponent<TileHover>().activeHover = true;
            }
        }

        public void MoveEntitiesOut()
        {
            PruneDestroyedEntities();

            foreach (AbstractEntity entity in entities)
            {
                if (entity == null)
                    continue;

                LerpPosition lerpPosition = entity.GetComponent<LerpPosition>();
                if (lerpPosition == null)
                    continue;

                lerpPosition.targetLocation += new Vector3(0, -750);
            }
        }
        public void MoveEntitiesIn()
        {
            PruneDestroyedEntities();

            foreach (AbstractEntity entity in entities)
            {
                if (entity == null)
                    continue;

                LerpPosition lerpPosition = entity.GetComponent<LerpPosition>();
                if (lerpPosition == null)
                    continue;

                lerpPosition.targetLocation += new Vector3(0, 750);
            }
        }

        private void PruneDestroyedEntities()
        {
            entities.RemoveAll(entity => entity == null);
        }

        public override void Exit()
        {
            UnsubscribeFromRunInfoEvents();
            PlayWindowOutSound();
            playingHealth.targetLocation = new Vector3(0, -600, 0);
            
            playingUI.SetScale(new Vector3(2, 2, 2));
            OnAllPhasesEnd();
            player.transform.SetParent(this.transform);
            
            EnvironmentManager.instance.ClearPassives();
            
            HexGridManager.Instance.UnregisterHexClickCallback(HexClickPlayerController.StaticHexClickCallback);
            
            player.Shield = 0;
            player.statusManager?.ClearStatuses();
            BattleStats.ResetStatsBattle();
            
            Debug.Log("Exiting Play State");
            Deck.Instance.DiscardHand();
            PruneDestroyedEntities();
            List<AbstractEntity> toRemove = new List<AbstractEntity>();
            // TurnIndicator.SendToLocation(new Vector3(0, 200, 0));

            foreach (AbstractEntity entity in entities)
            {
                if (entity != null && entity != player)
                {
                    toRemove.Add(entity);
                }
            }

            foreach (AbstractEntity entity in toRemove)
            {
                entities.Remove(entity);
                if (entity != null)
                    Destroy(entity.gameObject);
            }

            PruneDestroyedEntities();

            // gameUI.GetComponent<LerpPosition>().targetLocation = new Vector2(0, -750);
            RunInfo.Instance.CurrentEnergy = RunInfo.Instance.MaxEnergy;

            foreach (Transform tile in HexGridManager.Instance.grid.transform)
            {
                tile.GetComponent<TileHover>().activeHover = false;
            }
            MoveEntitiesOut();
            
            foreach (Vector2Int pos in HexGridManager.Instance._hexObjects.Keys)
            {
                HexGridManager.Instance.GetWorldHexObject(pos).GetComponent<HexPreviewHandler>().ClearPreviewEvents();
            }
            
            HexClickPlayerController.instance?.ClearMovableParticles();
            HexClickPlayerController.instance?.ClearPendingAttacks();
        }

        private void SubscribeToRunInfoEvents()
        {
            UnsubscribeFromRunInfoEvents();

            _subscribedRunInfo = RunInfo.Instance;
            if (_subscribedRunInfo != null)
            {
                _subscribedRunInfo.CurrentStepsChanged += HandleCurrentStepsChanged;
            }
        }

        private void UnsubscribeFromRunInfoEvents()
        {
            if (_subscribedRunInfo != null)
            {
                _subscribedRunInfo.CurrentStepsChanged -= HandleCurrentStepsChanged;
                _subscribedRunInfo = null;
            }
        }

        private void HandleCurrentStepsChanged(int previousSteps, int currentSteps)
        {
            if (!IsMovePhaseActive || !AllowUserInput)
                return;

            if (HexClickPlayerController.instance != null && !HexClickPlayerController.instance.isMoving)
            {
                HexClickPlayerController.instance.UpdateMovableParticles(this);
            }
        }

        #region Turn System ---------------
        public void EntityEndTurn()
        {

            var entity = CurrentTurn;

            // Clear all previews because the actions has been done
            entity.ClearNextTurnActionPreviews();
            
            entity.EndTurn();
            if (entity.entityType == EntityType.Player)
            {
                TickTileCountdowns();
                _playerMovementBlockedThisTurn = false;
            }

            Debug.Log("Entity end turn");
            if (CheckForFinish() != "none")
            {
                CaptureFinish();
                return;
            };
            HexClickPlayerController.instance.UpdateMovableParticles(this);

            ClearDeadEnemies();

            if (entity is NonPlayerEntity nonPlayerEntity)
            {
                PlanEnemyNextTurn(nonPlayerEntity, false);
            }
            

            // Unified start for the next entity
            StartEntityTurn();
        }
        
        private void StartEntityTurn()
        {
            if (CheckForFinish() != "none") return;

            if (_turnOrder.Count == 0)
            {
                Debug.LogWarning("No turn order entries exist.");
                return;
            }

            int attempts = 0;

            do
            {
                _currentTurnIndex = (_currentTurnIndex + 1) % _turnOrder.Count;
                attempts++;

                if (attempts > _turnOrder.Count)
                {
                    Debug.LogWarning("All entities in turn order are Neutral (or invalid).");
                    return;
                }

                int entIndex = _turnOrder[_currentTurnIndex];

                if (entIndex < 0 || entIndex >= entities.Count)
                    continue;

            } while (entities[_turnOrder[_currentTurnIndex]].entityType == EntityType.Neutral);

            var entity = CurrentTurn;
            turnIndicatorManager.SetCurrentTurn(_turnOrder, entities, _currentTurnIndex);

            if (entity.entityType == EntityType.Player)
            {
                ActivateMovePhase();
            }
            else
            {
                ActivateEnemyTurnPhase();
            }

            entity.StartTurn();

            if (entity.entityType == EntityType.Player)
            {
                ShowEnemyIntentPreviews();
            }

            if (entity is NonPlayerEntity enemy)
                StartCoroutine(MakeEnemyTurn(enemy));
        }

        private void PlanEnemyNextTurn(NonPlayerEntity enemy, bool showIntent)
        {
            if (enemy == null)
                return;

            if (enemy.behavior == null || enemy.Health <= 0)
            {
                _enemyPlanningPositions.Remove(enemy);
                return;
            }

            PruneEnemyPlanningPositions();
            enemy.behavior.NextTurn(_enemyPlanningPositions);
            _enemyPlanningPositions[enemy] = GetPlannedFinalPosition(enemy);

            if (showIntent)
            {
                ShowEnemyIntentPreview(enemy);
            }
            else
            {
                enemy.ClearIntentVisuals();
            }
        }

        private void PruneEnemyPlanningPositions()
        {
            List<AbstractEntity> toRemove = new List<AbstractEntity>();
            foreach (AbstractEntity entity in _enemyPlanningPositions.Keys)
            {
                if (entity == null || entity.Health <= 0 || !entities.Contains(entity))
                    toRemove.Add(entity);
            }

            foreach (AbstractEntity entity in toRemove)
            {
                _enemyPlanningPositions.Remove(entity);
            }
        }

        private Vector2Int GetPlannedFinalPosition(NonPlayerEntity enemy)
        {
            if (enemy == null)
                return Vector2Int.zero;

            Vector2Int finalPosition = enemy.positionRowCol;
            if (enemy.plannedAction == null)
                return finalPosition;

            foreach (AbstractAction action in enemy.plannedAction)
            {
                if (action is MoveAction moveAction)
                {
                    finalPosition = HexGridManager.MoveHex(
                        finalPosition,
                        moveAction.Direction,
                        moveAction.Distance);
                }
            }

            return finalPosition;
        }

        private void ShowEnemyIntentPreviews()
        {
            foreach (AbstractEntity entity in entities)
            {
                if (entity is NonPlayerEntity enemy && enemy.Health > 0)
                {
                    ShowEnemyIntentPreview(enemy);
                }
            }
        }

        private void ShowEnemyIntentPreview(NonPlayerEntity enemy)
        {
            enemy.HandleNextTurnActions(enemy.plannedAction);
            enemy.SetIntent();
        }

        private void ClearEnemyIntentPreviews()
        {
            foreach (AbstractEntity entity in entities)
            {
                if (entity is NonPlayerEntity enemy)
                {
                    enemy.ClearIntentVisuals();
                }
            }
        }

        
        private IEnumerator MakeEnemyTurn(NonPlayerEntity nonPlayerEntity)
        {
            if (nonPlayerEntity.behavior == null)
            {
                EntityEndTurn();
                yield break;
            }
            yield return new WaitForSeconds(0.25f * (1/GameplayNavSettings.speed));
            yield return nonPlayerEntity.behavior.MakeTurn();
            yield return new WaitForSeconds(0.25f * (1/GameplayNavSettings.speed));
            EntityEndTurn();
        }

        public void ClearDeadEnemies()
        {
            bool removedAny = false;

            for (int i = entities.Count - 1; i >= 0; i--)
            {
                var entity = entities[i];
                if (entity == null)
                {
                    removedAny = true;
                    entities.RemoveAt(i);
                    continue;
                }

                if (entity.Health > 0) continue;

                removedAny = true;
                entity.Die();
                entities.RemoveAt(i);
                ScaleDownAndDestroy(entity.gameObject);
            }

            if (removedAny)
            {
                RebuildTurnOrder();

                if (_turnOrder.Count == 0)
                {
                    _currentTurnIndex = -1;
                    turnIndicatorManager.Rebuild(entities, _turnOrder);
                    return;
                }

                // Keep progression sane: next StartEntityTurn() increments, so keep it within [-1 .. Count-1]
                _currentTurnIndex = Mathf.Clamp(_currentTurnIndex, -1, _turnOrder.Count - 1);

                turnIndicatorManager.Rebuild(entities, _turnOrder);
                turnIndicatorManager.SetCurrentTurn(_turnOrder, entities, _currentTurnIndex);
            }
            else
            {
                // still rebuild UI if you want, but not necessary for turn logic
                // turnIndicatorManager.Rebuild(_entities);
            }
        }

        private void ScaleDownAndDestroy(GameObject entityObject)
        {
            if (entityObject == null)
                return;

            LerpPosition lerpPosition = entityObject.GetComponent<LerpPosition>();
            if (lerpPosition != null)
            {
                lerpPosition.targetScale = Vector3.zero;
            }

            EaseScale easeScale = entityObject.GetComponent<EaseScale>();
            if (easeScale == null)
            {
                Destroy(entityObject);
                return;
            }

            easeScale.durationSeconds = Mathf.Max(0.01f, deadEnemyCleanupScaleDuration / GameplayNavSettings.speed);
            easeScale.SetScale(Vector3.zero, () =>
            {
                if (entityObject != null)
                    Destroy(entityObject);
            });
        }

        public void PlayerEndTurn()
        {
            if (CurrentTurn.entityType != EntityType.Player)
                return;

            if (!AllowUserInput)
                return;

            if (IsMovePhaseActive)
            {
                ActivateCardPhase();
                return;
            }

            if (!IsCardPhaseActive)
                return;

            BattleStats.ResetStatsTurn();

            CaptureFinish();
            
            EntityEndTurn();
            
            AllowUserInput = false;
            
        }

        public void PlayerEndPhase()
        {
            PlayerEndTurn();
        }

        public void CaptureFinish()
        {
            if (_finishCoroutine != null)
                return;

            string finish = CheckForFinish();
            if (finish == "player")
            {
                _finishCoroutine = StartCoroutine(PlayerWonAfterDeaths());
            }
            else if (finish == "enemy")
            {
                EntityWon();
            }
        }

        private IEnumerator PlayerWonAfterDeaths()
        {
            AllowUserInput = false;

            float waitSeconds = GetLongestPendingDeathAnimation();
            if (waitSeconds > 0f)
                yield return new WaitForSeconds(waitSeconds);

            _finishCoroutine = null;
            PlayerWon();
        }

        private float GetLongestPendingDeathAnimation()
        {
            float longestDuration = 0f;

            foreach (AbstractEntity entity in entities)
            {
                if (entity == null ||
                    entity.entityType != EntityType.Enemy ||
                    entity.Health > 0 ||
                    entity.skeletonAnimation == null)
                {
                    continue;
                }

                Spine.Animation deathAnimation =
                    entity.skeletonAnimation.Skeleton?.Data?.FindAnimation("die");

                if (deathAnimation != null)
                    longestDuration = Mathf.Max(longestDuration, deathAnimation.Duration);
            }

            return longestDuration;
        }

        private void UpdateNextTurnAttacks()
        {
            foreach (Vector2Int pos in HexGridManager.Instance.GetAllGridPositions())
            {
                GOList list = HexGridManager.Instance.GetWorldHexObject(pos).GetComponent<GOList>();
                list.GetValue("Particles").SetActive(false);
                list.GetValue("Damage").SetActive(false);
            }

        }

        public void PlayerWon()
        {
            Debug.Log("Player has finished");
            RunInfo.Instance.Money += RewardMoney;

            if (IsFinalMapLayer())
            {
                if (GameStateManager.Instance.GetState<GameFinishState>() != null)
                {
                    GameStateManager.Instance.Change<GameFinishState>();
                }
                else
                {
                    Debug.LogWarning("GameFinishState is not registered on the GameStateManager.");
                }

                return;
            }

            GameStateManager.Instance.Change<ShopState>();
        }

        private bool IsFinalMapLayer()
        {
            return MapProgressLayerCount > 0 &&
                   MapProgressLayer >= MapProgressLayerCount - 1;
        }

        public void EntityWon()
        {
            Debug.Log("Aww bummer you fuckin dork you lost");

            if (GameStateManager.Instance.GetState<GameOverState>() != null)
            {
                GameStateManager.Instance.Change<GameOverState>();
            }
            else
            {
                Debug.LogWarning("GameOverState is not registered on the GameStateManager.");
            }
        }

        public string CheckForFinish()
        {
            bool enemyWin = true;
            bool playerWin = true;
            foreach (AbstractEntity entity in entities)
            {
                if (entity.entityType == EntityType.Player && entity.Health > 0)
                {
                    enemyWin = false;
                }

                if (entity.entityType == EntityType.Enemy && entity.Health > 0)
                {
                    playerWin = false;
                }
            }

            if (enemyWin) return "enemy";
            if (playerWin) return "player";
            return "none";
        }
        #endregion

        #region Entity Manager ---------------
        public bool EntitiesOnHex(Vector2Int coords, out List<AbstractEntity> list)
        {
            list = entities.FindAll(e => e.positionRowCol == coords);
            return list.Count > 0;
        }

        public List<AbstractEntity> GetAdjacentEntities(Vector2Int center)
        {
            List<AbstractEntity> adjacentEntities = new List<AbstractEntity>();

            foreach (Vector2Int adjacentPosition in HexGridManager.AdjacentHexes(center))
            {
                if (EntitiesOnHex(adjacentPosition, out List<AbstractEntity> entitiesOnHex))
                    adjacentEntities.AddRange(entitiesOnHex);
            }

            return adjacentEntities;
        }

        public List<AbstractEntity> GetAdjacentEnemies(Vector2Int center)
        {
            return GetAdjacentEntities(center)
                .Where(entity => entity != null && entity.entityType == EntityType.Enemy)
                .ToList();
        }

        public List<AbstractEntity> GetEntities()
        {
            return entities;
        }

        public bool IsPlayerAttackTarget(AbstractEntity entity)
        {
            return entity != null &&
                   entity.Health > 0 &&
                   (entity.entityType == EntityType.Enemy || entity.entityType == EntityType.Neutral);
        }

        public bool IsValidHex(Vector2Int coords)
        {
            if (_grid.HexType(coords) == "none") return false;
            return !EntitiesOnHex(coords, out _);
        }

        public bool MoveEntity(AbstractEntity ent, string dir, int dist)
        {
            if (ent == null)
                return false;

            var target = HexGridManager.MoveHex(ent.positionRowCol, dir, dist);
            if (!IsValidHex(target)) return false;
            if (IsMovementBlocked(ent, Mathf.Max(1, dist))) return false;

            ent.MoveEntity(target);
            QueueEnemyIntentRefreshAfterMove(ent);

            if (ent.entityType == EntityType.Player)
            {
                PlayerMovesThisTurn += Mathf.Max(1, dist);
                TriggerPlayerTileLand(target, ent);
                BattleStats.TilesMovedThisBattle += dist;
                BattleStats.TilesMovedThisTurn += dist;
                Deck.Instance?.MarkPlayabilityDirty();
            }

            return true;
        }

        private bool IsMovementBlocked(AbstractEntity ent, int distance)
        {
            if (ent == null)
                return true;

            if (ent.entityType == EntityType.Player && IsPlayerMovementBlocked)
                return true;

            return ent.StatusesBlockMovement(Mathf.Max(1, distance));
        }

        public bool SwapEntities(AbstractEntity first, AbstractEntity second)
        {
            if (first == null ||
                second == null ||
                first == second ||
                first.Health <= 0 ||
                second.Health <= 0)
            {
                return false;
            }

            Vector2Int firstPosition = first.positionRowCol;
            Vector2Int secondPosition = second.positionRowCol;
            if (firstPosition == secondPosition)
                return false;

            if (!IsBoardHex(firstPosition) || !IsBoardHex(secondPosition))
                return false;

            if (HasOtherLiveEntityOnHex(firstPosition, first, second) ||
                HasOtherLiveEntityOnHex(secondPosition, first, second))
            {
                return false;
            }

            if (IsMovementBlocked(first, 1) || IsMovementBlocked(second, 1))
                return false;

            first.MoveEntity(secondPosition);
            second.MoveEntity(firstPosition);

            QueueEnemyIntentRefreshAfterMove(first);
            QueueEnemyIntentRefreshAfterMove(second);

            RecordPlayerSwapMove(first, secondPosition);
            RecordPlayerSwapMove(second, firstPosition);
            Deck.Instance?.MarkPlayabilityDirty();

            return true;
        }

        private bool IsBoardHex(Vector2Int coords)
        {
            return _grid != null && _grid.HexType(coords) != "none";
        }

        private bool HasOtherLiveEntityOnHex(Vector2Int coords, AbstractEntity first, AbstractEntity second)
        {
            if (!EntitiesOnHex(coords, out List<AbstractEntity> entitiesOnHex))
                return false;

            foreach (AbstractEntity entity in entitiesOnHex)
            {
                if (entity != null &&
                    entity.Health > 0 &&
                    entity != first &&
                    entity != second)
                {
                    return true;
                }
            }

            return false;
        }

        private void RecordPlayerSwapMove(AbstractEntity entity, Vector2Int target)
        {
            if (entity == null || entity.entityType != EntityType.Player)
                return;

            PlayerMovesThisTurn += 1;
            TriggerPlayerTileLand(target, entity);
            BattleStats.TilesMovedThisBattle += 1;
            BattleStats.TilesMovedThisTurn += 1;
        }
        
        public bool MoveEntity(AbstractEntity ent, Vector2Int target)
        {
            if (ent == null)
                return false;

            if (!IsValidHex(target)) 
                return false;

            // TODO: DEBUG
            int dist = 1;
            if (IsMovementBlocked(ent, dist)) return false;
            
            ent.MoveEntity(target);
            QueueEnemyIntentRefreshAfterMove(ent);

            if (ent.entityType == EntityType.Player)
            {
                PlayerMovesThisTurn += Mathf.Max(1, dist);
                TriggerPlayerTileLand(target, ent);

                BattleStats.TilesMovedThisBattle += dist;
                BattleStats.TilesMovedThisTurn += dist;
                Deck.Instance?.MarkPlayabilityDirty();
            }

            return true;
        }

        public CardEventResult DamageEntities(
            Vector2Int coords,
            int dmg,
            AbstractStatus status,
            AbstractCardEvent sourceEvent = null)
        {
            CardEventResult result = new CardEventResult(sourceEvent);

            foreach (var e in entities)
            {
                if (e.positionRowCol == coords)
                {
                    float healthBeforeDamage = e.Health;
                    e.Damage(dmg);

                    if (healthBeforeDamage > 0)
                    {
                        result.DamagedEntities.Add(e);
                    }

                    if (healthBeforeDamage > 0 && e.Health <= 0)
                    {
                        result.DefeatedEntities.Add(e);
                    }

                    e.ApplyStatus(status);
                    StatusApplicationFx.TryPlay(status, e);
                }
            }

            return result;
        }
        #endregion

        public override PlayingStateSaveData CaptureSaveData()
        {
            return new PlayingStateSaveData
            {
                encounterData = encounterData,
                mapProgressLayer = MapProgressLayer,
                mapProgressLayerCount = MapProgressLayerCount,
                tileCountdownStates = CaptureTileCountdownStates()
            };
        }
        
    }
}
