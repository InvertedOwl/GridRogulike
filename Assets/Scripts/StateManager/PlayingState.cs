using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cards.Actions;
using Cards.CardEvents;
using Entities;
using Entities.Enemies;
using Grid;
using TMPro;
using Types.Statuses;
using Types.Tiles;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions.EasingCore;
using Util;
using Random = System.Random;

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

                if (changed)
                {
                    Deck.Instance.UpdatePlayability();
                }

                foreach (Vector2Int hex in HexGridManager.Instance.BoardDictionary.Keys)
                {
                    HexGridManager.Instance.GetWorldHexObject(hex).GetComponent<HexPreviewHandler>().DisablePreview =
                        !value;
                }   
            }
        }

        private readonly List<AbstractEntity> _entities = new();
        private HexGridManager _grid = null!;
        public Player player;
        public GameObject gameUI;
        protected Random random;
        
        public void Awake()
        {
            random = RunInfo.NewRandom("playing".GetHashCode());
        }
        
        public Button EndTurnButton;
        public Button RedrawButton;
        
        public static int RewardMoney;
        public static EncounterData encounterData;
        
        public LerpPosition cameraLerpPosition;

        public EasePosition TurnIndicator;

        public GOList GoList;

        
        public TurnIndicatorManager turnIndicatorManager;

        private static Random _entitySpawnRandom = RunInfo.NewRandom("espawn".GetHashCode());
        
        private readonly List<int> _turnOrder = new();
        private int _currentTurnIndex;
        public AbstractEntity CurrentTurn => _entities[_turnOrder[_currentTurnIndex]];
        
        public override void Enter()
        {
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



            // Start all NonPlayer turns
            foreach (AbstractEntity e in _entities)
            {
                if (e is NonPlayerEntity)
                {
                    ((NonPlayerEntity)e).HandleNextTurnActions(((NonPlayerEntity)e).NextTurn());
                }
            }
            
            turnIndicatorManager.Rebuild(_entities, _turnOrder);
            StartCoroutine(WaitFrame());
            StartEntityTurn();
            TurnIndicator.SendToLocation(new Vector3(0, 0, 0));
            BattleStats.ResetStatsBattle();
            
            HexGridManager.Instance.RegisterHexClickCallback(HexClickPlayerController.StaticHexClickCallback);
            HexGridManager.Instance.RegisterHexHoverEnterCallback(HexClickPlayerController.StaticHexHoverOnCallback);
            HexGridManager.Instance.RegisterHexHoverExitCallback(HexClickPlayerController.StaticHexHoverOffCallback);

        }

        IEnumerator WaitFrame()
        {
            yield return new WaitForFixedUpdate();
            turnIndicatorManager.SetCurrentTurn(_turnOrder, _entities, 0);
        }
        
        // TODO: Add speed stuff so you can have more turns more often
        private void RebuildTurnOrder()
        {
            _turnOrder.Clear();

            for (int i = 0; i < _entities.Count; i++)
                _turnOrder.Add(i);
        }

        public void Update()
        {
            if (!GameStateManager.Instance.IsCurrent<PlayingState>())
                return;
            
            Vector3 averagePos = Vector3.zero;

            foreach (var entity in _entities)
            {
                averagePos += entity.transform.position;
            }
            averagePos /= _entities.Count;
            
            cameraLerpPosition.targetLocation = new Vector3(averagePos.x, averagePos.y -1, -10); // -1 y for adjusted center. It's made up
        }
        
        
        private void InitializeDeckAndGrid()
        {
            Deck.Instance.UpdateDeck();
            _grid = HexGridManager.Instance;
        }

        private void SetupEntities()
        {
            _entities.Clear();
            _entities.Add(player);
            _entities.AddRange(FindObjectsByType<TestNonPlayerEntity>(FindObjectsSortMode.InstanceID));

            Debug.Log("Difficulty: " + RunInfo.Instance.Difficulty);
            
            _entities.ForEach(e =>
            {
                if (e.Health <= 0)
                {
                    e.Die();
                }
            });
            
            _entities.RemoveAll(e => e.Health <= 0);
            
            int numNormalEnemy = 0;
            int numHardEnemy = 0;
            int numBossEnemy = 0;

            foreach (EnemyEntry enemyEntry in encounterData.enemies)
            {
                if (enemyEntry.enemyType == EnemyType.Normal)
                    numNormalEnemy += 1;
                if (enemyEntry.enemyType == EnemyType.Hard)
                    numHardEnemy += 1;
                if (enemyEntry.enemyType == EnemyType.Boss)
                    numBossEnemy += 1;
            }

            var spawnSpots = HexGridManager.Instance.BoardDictionary.Keys
                .Where(p => !_entities.Any(e => e.positionRowCol == p))
                .OrderBy(_ => random.Next())
                .Take(numNormalEnemy + numBossEnemy + numHardEnemy)
                .ToList();

            // foreach (var pos in spawnSpots)
            // {
                // EnemyType enemyType = EnemyType.Normal;
                // if (numNormalEnemy > 0)
                // {
                //     enemyType = EnemyType.Normal;
                //     numNormalEnemy -= 1;
                // }
                //
                // if (numHardEnemy > 0)
                // {
                //     enemyType = EnemyType.Hard;
                //     numHardEnemy -= 1;
                // }
                //
                // if (numBossEnemy > 0)
                // {
                //     enemyType = EnemyType.Boss;
                //     numBossEnemy -= 1;
                // }
                
            // }

            
            _entities.AddRange(SpawnEncounter(spawnSpots));

            // Not enough spawn spots
            if (numNormalEnemy > 0 || numHardEnemy > 0 || numBossEnemy > 0)
            {
                Debug.Log("Not enough spawn spots");
            }
            if (TryGetRandomEmptyHex(out var playerStart))
            {
                player.positionRowCol = playerStart;
            }

            else
            {
                player.positionRowCol = new Vector2Int(0, 0);
                Debug.LogError("No empty hexes found for player; defaulting to (0,0).");
            }

            foreach (var e in _entities)
            {
                e.MoveEntity(e.positionRowCol);
                

                if (e.entityType != EntityType.Player)
                    e.Health = e.initialHealth;
            }
        }

        private bool TryGetRandomEmptyHex(out Vector2Int pos)
        {
            var allHexes = HexGridManager.Instance.BoardDictionary.Keys;

            var empties = allHexes.Where(p => !_entities.Any(e => e.positionRowCol == p)).OrderBy(_ => random.Next()).ToList();

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

            if (positions.Count < encounterData.enemies.Count)
            {
                throw new Exception("Not enough spawn spots"); // TODO: Handle this
            }
            
            for (int i = 0; i < encounterData.enemies.Count; i++)
            {
                EnemyEntry enemy = encounterData.enemies[i];
                Vector2Int position = positions[i];
                GameObject enemyObject = Instantiate(enemy.enemyPrefab, GoList.GetValue("board_container").transform);
                enemyObject.transform.position = HexGridManager.GetHexCenter(position.x, position.y);
                AbstractEntity abstractEntity = enemyObject.GetComponent<AbstractEntity>();
                abstractEntity.positionRowCol = position;
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
            foreach (AbstractEntity entity in _entities)
            {
                entity.GetComponent<LerpPosition>().targetLocation += new Vector3(0, -750);
            }
        }
        public void MoveEntitiesIn()
        {
            foreach (AbstractEntity entity in _entities)
            {
                entity.GetComponent<LerpPosition>().targetLocation += new Vector3(0, 750);
            }
        }

        public override void Exit()
        {
            player.transform.SetParent(this.transform);
            
            EnvironmentManager.instance.ClearPassives();
            
            HexGridManager.Instance.UnregisterHexClickCallback(HexClickPlayerController.StaticHexClickCallback);
            
            player.Shield = 0;
            BattleStats.ResetStatsBattle();
            
            Debug.Log("Exiting Play State");
            Deck.Instance.DiscardHand();
            List<NonPlayerEntity> toRemove = new List<NonPlayerEntity>();
            TurnIndicator.SendToLocation(new Vector3(0, 200, 0));

            foreach (AbstractEntity entity in _entities)
            {
                if (entity is NonPlayerEntity)
                {
                    Debug.Log("Entity is an enemy!! WOOOOO");
                    toRemove.Add((NonPlayerEntity)entity);
                }
            }

            foreach (NonPlayerEntity enemy in toRemove)
            {
                _entities.Remove(enemy);
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

            if (entity is NonPlayerEntity)
            {
                List<AbstractAction> actions = ((NonPlayerEntity)entity).NextTurn();
                ((NonPlayerEntity)entity).HandleNextTurnActions(actions);
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

                if (entIndex < 0 || entIndex >= _entities.Count)
                    continue;

            } while (_entities[_turnOrder[_currentTurnIndex]].entityType == EntityType.Neutral);

            var entity = CurrentTurn;
            turnIndicatorManager.SetCurrentTurn(_turnOrder, _entities, _currentTurnIndex);

            entity.StartTurn();

            if (entity is NonPlayerEntity enemy)
                StartCoroutine(MakeEnemyTurn(enemy));
        }

        
        private IEnumerator MakeEnemyTurn(NonPlayerEntity nonPlayerEntity)
        {
            yield return new WaitForSeconds(0.25f);
            yield return nonPlayerEntity.MakeTurn();
            yield return new WaitForSeconds(0.25f);
            EntityEndTurn();
        }

        public void ClearDeadEnemies()
        {
            bool removedAny = false;

            for (int i = _entities.Count - 1; i >= 0; i--)
            {
                var entity = _entities[i];
                if (entity.Health > 0) continue;

                removedAny = true;
                entity.Die();
                _entities.RemoveAt(i);
                Destroy(entity.gameObject);
            }

            if (removedAny)
            {
                RebuildTurnOrder();

                if (_turnOrder.Count == 0)
                {
                    _currentTurnIndex = -1;
                    turnIndicatorManager.Rebuild(_entities, _turnOrder);
                    return;
                }

                // Keep progression sane: next StartEntityTurn() increments, so keep it within [-1 .. Count-1]
                _currentTurnIndex = Mathf.Clamp(_currentTurnIndex, -1, _turnOrder.Count - 1);

                turnIndicatorManager.Rebuild(_entities, _turnOrder);
                turnIndicatorManager.SetCurrentTurn(_turnOrder, _entities, _currentTurnIndex);
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
            foreach (AbstractEntity entity in _entities)
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
            list = _entities.FindAll(e => e.positionRowCol == coords);
            return list.Count > 0;
        }

        public List<AbstractEntity> GetEntities()
        {
            return _entities;
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
            foreach (var e in _entities)
                if (e.positionRowCol == coords) e.Damage(dmg, status);
        }
        #endregion
    }
}
