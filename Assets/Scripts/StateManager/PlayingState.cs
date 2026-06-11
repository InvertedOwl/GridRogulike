using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cards.CardEvents;
using Entities;
using Entities.Enemies;
using Grid;
using ScriptableObjects;
using Serializer;
using Types.Statuses;
using Types.Tiles;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Util;

namespace StateManager
{
    public class PlayingState : GameState
    {

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
        
        private readonly List<int> _turnOrder = new();
        private int _currentTurnIndex;
        public AbstractEntity CurrentTurn => entities[_turnOrder[_currentTurnIndex]];
        private float _autoEndReadyTime = -1f;
        private const float AutoEndDelaySeconds = 0.5f;
        private Coroutine _finishCoroutine;
        [SerializeField] private float deadEnemyCleanupScaleDuration = 0.2f;
        [SerializeField] private float disabledTileOpacity = 0.6f;
        private readonly HashSet<Vector2Int> _tilesUsedThisTurn = new();
        private readonly HashSet<Vector2Int> _tilesUsedThisCombat = new();
        private CameraMove _cameraMove;

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
                SaveData = null;
            }
            
            if (random == null)
            {
                random = RunInfo.NewRandom("playing");
            }
            
            MoveEntitiesIn();
            InitializeDeckAndGrid();
            SetupInitialTiles();
            ResetCombatTileTriggers();
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

            UpdateAutoCamera();
            TryAutoEndPlayerTurn();
        }

        private void UpdateAutoCamera()
        {
            CameraMove cameraMove = GetCameraMove();
            if (cameraMove == null)
                return;

            if (!GameplayNavSettings.autocamera)
            {
                cameraMove.ClearAutoCameraTarget();
                return;
            }

            if (TryGetCombatCenter(out Vector3 combatCenter))
            {
                cameraMove.SetAutoCameraTarget(combatCenter);
            }
            else
            {
                cameraMove.ClearAutoCameraTarget();
            }
        }

        private CameraMove GetCameraMove()
        {
            if (_cameraMove != null)
                return _cameraMove;

            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                _cameraMove = mainCamera.GetComponent<CameraMove>();

                if (_cameraMove == null)
                    _cameraMove = mainCamera.GetComponentInParent<CameraMove>();

                if (_cameraMove == null)
                    _cameraMove = mainCamera.GetComponentInChildren<CameraMove>();
            }

            if (_cameraMove == null)
                _cameraMove = FindFirstObjectByType<CameraMove>();

            return _cameraMove;
        }

        private bool TryGetCombatCenter(out Vector3 center)
        {
            center = Vector3.zero;
            int entityCount = 0;

            foreach (AbstractEntity entity in entities)
            {
                if (entity == null ||
                    (entity.entityType != EntityType.Player && entity.entityType != EntityType.Enemy) ||
                    entity.Health <= 0)
                {
                    continue;
                }

                HexGridManager.Instance._hexObjects.TryGetValue(entity.positionRowCol, out GameObject entityHex);
                Vector3 entityPosition = entityHex != null
                    ? entityHex.transform.position
                    : HexGridManager.GetHexCenter(entity.positionRowCol.x, entity.positionRowCol.y);

                center += entityPosition;
                entityCount++;
            }

            if (entityCount == 0)
                return false;

            center /= entityCount;
            return true;
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

            if (HasResolvingCard() || HasPlayableCard() || HasPendingPlayerAttack() || HasReachableMove())
            {
                ResetAutoEndTimer();
                return;
            }

            if (_autoEndReadyTime < 0f)
            {
                _autoEndReadyTime = Time.time + AutoEndDelaySeconds;
                return;
            }

            if (Time.time < _autoEndReadyTime)
                return;

            if (IsBackgroundAnimationRunning())
                return;

            ResetAutoEndTimer();
            PlayerEndTurn();
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

        private bool HasResolvingCard()
        {
            return Deck.Instance.Hand.Any(card => card != null && card.played);
        }

        private bool HasPlayableCard()
        {
            foreach (CardMonobehaviour card in Deck.Instance.Hand)
            {
                if (card == null || card.played || card.onlyDisplay)
                    continue;

                int cost = (int)(card.CostOverride > -1 ? card.CostOverride : card.Card.Cost);
                if (cost <= RunInfo.Instance.CurrentEnergy)
                    return true;
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
            entities.AddRange(FindObjectsByType<TestNonPlayerEntity>(FindObjectsSortMode.InstanceID));

            
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

            // Not enough spawn spots
            if (numNormalEnemy > 0 || numHardEnemy > 0 || numBossEnemy > 0)
            {
                Debug.Log("Not enough spawn spots");
            }
            Debug.Log("Player position: " + player.positionRowCol);
            
            Debug.Log("Player is in " + entities
                .Contains(player));
            
            foreach (AbstractEntity e in entities)
            {
                if (e is NonPlayerEntity nonPlayerEntity)
                {
                    PlanEnemyNextTurn(nonPlayerEntity, true);
                }
            }
        }

        private bool TryGetRandomEmptyHex(out Vector2Int pos)
        {
            var empties = GetOrderedBoardPositions()
                .Where(p => !entities.Any(e => e.positionRowCol == p))
                .OrderBy(_ => random.Next())
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
                GameObject enemyObject = Instantiate(enemyEntry.enemyPrefab, GoList.GetValue("board_container").transform);
                // Vector2 pos = HexGridManager.GetHexCenter(position.x, position.y);
                // Vector3 pos3 = new Vector3(pos.x, pos.y, -0.941f);
                // enemyObject.transform.position = pos3;
                AbstractEntity abstractEntity = enemyObject.GetComponent<AbstractEntity>();
                abstractEntity.positionRowCol = position;
                // enemyObject.GetComponent<AbstractEntity>().MoveEntity(position);
                encounter.Add(abstractEntity);
                
            }
            return encounter;
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
                if (entity is not NonPlayerEntity)
                    continue;

                entity.initialHealth = ScaleEnemyFloat(entity.initialHealth, multiplier);
                entity.Health = entity.initialHealth;
            }
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

        public List<MapData> maps = new List<MapData>();

        private void SetupInitialTiles()
        {
            var origin = new Vector2Int(0, 0);

            if (generateInitialMap)
            {
                foreach (Vector2Int position in HexGridManager.HexesInRadius(initialMapRadius))
                {
                    _grid.TryAdd(position, position == origin ? homeTileType : generatedTileType);
                }
            }

            foreach (MapData map in maps)
            {
                foreach (MapEntry mapEntry in map.entries)
                {
                    _grid.TryAdd(mapEntry.key, mapEntry.value);
                }
            }
            
            _grid.UpdateBoard();
        }

        private void ResetCombatTileTriggers()
        {
            _tilesUsedThisTurn.Clear();
            _tilesUsedThisCombat.Clear();
            UpdateAllTileDisableVisuals();
        }

        public void ResetTurnTileTriggers()
        {
            _tilesUsedThisTurn.Clear();
            UpdateAllTileDisableVisuals();
        }

        public void TriggerPlayerTile(Vector2Int position, AbstractEntity entity)
        {
            if (entity == null || entity.entityType != EntityType.Player)
                return;

            string tileId = HexGridManager.Instance.HexType(position);
            if (!TileData.tiles.TryGetValue(tileId, out TileEntry tile))
                return;

            if (!CanTriggerTile(position, tile))
                return;

            MarkTileTriggered(position, tile);
            CardEventPipeline.Activate(tile.landEvent.Invoke(), entity);
        }

        private bool CanTriggerTile(Vector2Int position, TileEntry tile)
        {
            return tile.triggerLimit switch
            {
                TileTriggerLimit.OncePerTurn => !_tilesUsedThisTurn.Contains(position),
                TileTriggerLimit.OncePerCombat => !_tilesUsedThisCombat.Contains(position),
                _ => true
            };
        }

        private void MarkTileTriggered(Vector2Int position, TileEntry tile)
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

        private void EnableTileHovers()
        {
            foreach (Transform tile in HexGridManager.Instance.grid.transform)
            {
                tile.GetComponent<TileHover>().activeHover = true;
            }
        }

        public void MoveEntitiesOut()
        {
            foreach (AbstractEntity entity in entities)
            {
                entity.GetComponent<LerpPosition>().targetLocation += new Vector3(0, -750);
            }
        }
        public void MoveEntitiesIn()
        {
            foreach (AbstractEntity entity in entities)
            {
                entity.GetComponent<LerpPosition>().targetLocation += new Vector3(0, 750);
            }
        }

        public override void Exit()
        {
            PlayWindowOutSound();
            SendCameraToBoardCenter();
            playingHealth.targetLocation = new Vector3(0, -600, 0);
            
            playingUI.SetScale(new Vector3(2, 2, 2));

            player.transform.SetParent(this.transform);
            
            EnvironmentManager.instance.ClearPassives();
            
            HexGridManager.Instance.UnregisterHexClickCallback(HexClickPlayerController.StaticHexClickCallback);
            
            player.Shield = 0;
            player.statusManager?.ClearStatuses();
            BattleStats.ResetStatsBattle();
            
            Debug.Log("Exiting Play State");
            Deck.Instance.DiscardHand();
            List<NonPlayerEntity> toRemove = new List<NonPlayerEntity>();
            // TurnIndicator.SendToLocation(new Vector3(0, 200, 0));

            foreach (AbstractEntity entity in entities)
            {
                if (entity is NonPlayerEntity)
                {
                    Debug.Log("Entity is an enemy!! WOOOOO");
                    toRemove.Add((NonPlayerEntity)entity);
                }
            }

            foreach (NonPlayerEntity enemy in toRemove)
            {
                entities.Remove(enemy);
                Destroy(enemy.gameObject);
            }

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
            
            HexClickPlayerController.instance.ClearPendingAttacks();
        }

        private void SendCameraToBoardCenter()
        {
            CameraMove cameraMove = GetCameraMove();
            if (cameraMove == null)
                return;

            cameraMove.SetAutoCameraTarget(Vector3.zero);
        }

        #region Turn System ---------------
        public void EntityEndTurn()
        {

            var entity = CurrentTurn;

            // Clear all previews because the actions has been done
            entity.ClearNextTurnActionPreviews();
            
            entity.EndTurn();
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

            if (entity is NonPlayerEntity)
            {
                ClearEnemyIntentPreviews();
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
            if (enemy == null || enemy.behavior == null || enemy.Health <= 0)
                return;

            enemy.behavior.NextTurn();

            if (showIntent)
            {
                ShowEnemyIntentPreview(enemy);
            }
            else
            {
                enemy.ClearIntentVisuals();
            }
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

            BattleStats.ResetStatsTurn();

            CaptureFinish();
            
            EntityEndTurn();
            
            AllowUserInput = false;
            
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
            GameStateManager.Instance.Change<TilePickState>();

            RunInfo.Instance.Money += RewardMoney;
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

        public List<AbstractEntity> GetEntities()
        {
            return entities;
        }

        public bool IsValidHex(Vector2Int coords)
        {
            if (_grid.HexType(coords) == "none") return false;
            return !EntitiesOnHex(coords, out _);
        }

        public bool MoveEntity(AbstractEntity ent, string dir, int dist)
        {
            var target = HexGridManager.MoveHex(ent.positionRowCol, dir, dist);
            if (!IsValidHex(target)) return false;

            ent.MoveEntity(target);

            if (ent.entityType == EntityType.Player)
            {
                // Activate landing events
                TriggerPlayerTile(target, ent);
                BattleStats.TilesMovedThisBattle += dist;
                BattleStats.TilesMovedThisTurn += dist;
            }

            return true;
        }
        
        public bool MoveEntity(AbstractEntity ent, Vector2Int target)
        {
            if (!IsValidHex(target)) 
                return false;

            // TODO: DEBUG
            int dist = 1;
            
            ent.MoveEntity(target);

            if (ent.entityType == EntityType.Player)
            {
                TriggerPlayerTile(target, ent);

                BattleStats.TilesMovedThisBattle += dist;
                BattleStats.TilesMovedThisTurn += dist;
            }

            return true;
        }

        public void DamageEntities(Vector2Int coords, int dmg, AbstractStatus status)
        {
            foreach (var e in entities)
                if (e.positionRowCol == coords)
                {
                    e.Damage(dmg);
                    e.ApplyStatus(status);
                }
        }
        #endregion

        public override PlayingStateSaveData CaptureSaveData()
        {
            return new PlayingStateSaveData
            {
                encounterData = encounterData,
                mapProgressLayer = MapProgressLayer,
                mapProgressLayerCount = MapProgressLayerCount
            };
        }
        
    }
}
