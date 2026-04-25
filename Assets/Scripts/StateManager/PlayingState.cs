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
                Deck.Instance.SetInactive(!value);
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
            RewardMoney = 0;
            encounterData = null;
        }
        
        public MoveWithMouse cameraLerpPosition;

        public EasePosition TurnIndicator;

        public GOList GoList;

        
        public TurnIndicatorManager turnIndicatorManager;
        
        private readonly List<int> _turnOrder = new();
        private int _currentTurnIndex;
        public AbstractEntity CurrentTurn => entities[_turnOrder[_currentTurnIndex]];

        public EaseScale playingUI;
        public EasePosition playingHealth;
        
        [SerializeField] private LerpCameraSize cameraSizeLerp;
        [SerializeField] private float minViewSize = 5f;
        [SerializeField] private float viewPadding = 2f;
        [SerializeField] private float zoomSpeed = 3f;
        
        public override void Enter()
        {
            playingHealth.targetLocation = new Vector3(0, 0, 0);
            Debug.Log("Save is  " + SaveData);
            if (SaveData != null)
            {
                encounterData = ((PlayingStateSaveData) SaveData).encounterData;
                SaveData = null;
            }
            
            if (random == null)
            {
                random = RunInfo.NewRandom("playing");
            }
            
            MoveEntitiesIn();
            InitializeDeckAndGrid();
            SetupInitialTiles();
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

            Vector3 playerPos = player.transform.position;

            cameraLerpPosition.offset = new Vector3(playerPos.x/2, (playerPos.y - 1f)/2, -10);
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
            
            var spawnSpots = HexGridManager.Instance.BoardDictionary.Keys
                .Where(p => !entities.Any(e => e.positionRowCol == p))
                .OrderBy(_ => random.Next())
                .ToList();
            
            
            entities.AddRange(SpawnEncounter(spawnSpots));

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
                    nonPlayerEntity.HandleNextTurnActions(nonPlayerEntity.behavior.NextTurn());
                    nonPlayerEntity.SetIntent();
                }
            }
        }

        private bool TryGetRandomEmptyHex(out Vector2Int pos)
        {
            var allHexes = HexGridManager.Instance.BoardDictionary.Keys;

            var empties = allHexes.Where(p => !entities.Any(e => e.positionRowCol == p)).OrderBy(_ => random.Next()).ToList();

            if (empties.Count == 0)
            {
                pos = default;
                return false; 
            }

            pos = empties[0];
            return true;
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
                enemyObject.transform.eulerAngles = new Vector3(-70, 0, 0);
                AbstractEntity abstractEntity = enemyObject.GetComponent<AbstractEntity>();
                abstractEntity.positionRowCol = position;
                // enemyObject.GetComponent<AbstractEntity>().MoveEntity(position);
                encounter.Add(abstractEntity);
                
            }
            return encounter;
        }
        
        public List<MapData> maps = new List<MapData>();

        private void SetupInitialTiles()
        {
            var origin = new Vector2Int(0, 0);
            // _grid.TryAdd(origin, "start");

            foreach (MapData map in maps)
            {
                foreach (MapEntry mapEntry in map.entries)
                {
                    _grid.TryAdd(mapEntry.key, mapEntry.value);
                }
            }
            
            _grid.UpdateBoard();
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
            playingHealth.targetLocation = new Vector3(0, -600, 0);
            
            cameraSizeLerp.targetHeight = 4;
            playingUI.SetScale(new Vector3(2, 2, 2));

            player.transform.SetParent(this.transform);
            
            EnvironmentManager.instance.ClearPassives();
            
            HexGridManager.Instance.UnregisterHexClickCallback(HexClickPlayerController.StaticHexClickCallback);
            
            player.Shield = 0;
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
                HexGridManager.Instance.GetWorldHexObject(pos).GetComponent<HexPreviewHandler>().eventsOnThisHex.Clear();
            }
            
            HexClickPlayerController.instance.ToAttack.Clear();
        }

        #region Turn System ---------------
        public void EntityEndTurn()
        {

            var entity = CurrentTurn;

            // Clear all previews because the actions has been done
            foreach (Vector2Int hex in HexGridManager.Instance.BoardDictionary.Keys)
            {
                HexGridManager.Instance.GetWorldHexObject(hex).GetComponent<HexPreviewHandler>().eventsOnThisHex.Remove(entity);
            }
            
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
                nonPlayerEntity.HandleNextTurnActions(nonPlayerEntity.behavior.NextTurn());
                nonPlayerEntity.SetIntent();
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

            if (entity is NonPlayerEntity nonPlayerEntity)
            {
                nonPlayerEntity.RemoveIntent();
            }

            entity.StartTurn();

            if (entity is NonPlayerEntity enemy)
                StartCoroutine(MakeEnemyTurn(enemy));
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
                Destroy(entity.gameObject);
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
            string finish = CheckForFinish();
            if (finish == "player")
            {
                PlayerWon();
            }
            else if (finish == "enemy")
            {
                EntityWon();
            }
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
                foreach (AbstractCardEvent cardEvent in TileData.tiles[HexGridManager.Instance.HexType(target)].landEvent.Invoke())
                {
                    cardEvent.Activate(ent);
                }
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
                foreach (AbstractCardEvent cardEvent in 
                         TileData.tiles[HexGridManager.Instance.HexType(target)]
                             .landEvent.Invoke())
                {
                    cardEvent.Activate(ent);
                }

                BattleStats.TilesMovedThisBattle += dist;
                BattleStats.TilesMovedThisTurn += dist;
            }

            return true;
        }

        public void DamageEntities(Vector2Int coords, int dmg, AbstractStatus status)
        {
            foreach (var e in entities)
                if (e.positionRowCol == coords) e.Damage(dmg, status);
        }
        #endregion

        public override PlayingStateSaveData CaptureSaveData()
        {
            return new PlayingStateSaveData
            {
                encounterData = encounterData
            };
        }
        
    }
}
