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
        private readonly List<AbstractEntity> _entities = new();
        private HexGridManager _grid = null!;
        private int _currentTurnIndex;
        public Player player;
        public GameObject gameUI;
        protected System.Random random = new System.Random();
        
        public Button EndTurnButton;
        public Button RedrawButton;
        
        public MovePlayerController movePlayerController;

        public static int RewardMoney;
        public static int numNormalEnemy = 1;
        public static int numHardEnemy;
        public static int numBossEnemy;
        
        public LerpPosition cameraLerpPosition;

        public EasePosition TurnIndicator;

        public GOList GoList;

        public AbstractEntity CurrentTurn => _entities[_currentTurnIndex];
        
        public TurnIndicatorManager turnIndicatorManager;

        private static Random _entitySpawnRandom = RunInfo.NewRandom("espawn".GetHashCode());
        

        public override void Enter()
        {
            MoveEntitiesIn();
            InitializeDeckAndGrid();
            SetupInitialTiles();
            SetupEntities();
            SetupUI();
            SetupPlayerHand();
            EnableTileHovers();
            UpdateNextTurnIndicators();

            _currentTurnIndex = 0; // assuming player is added first in SetupEntities
            StartEntityTurn();
            turnIndicatorManager.UpdateTurnIndicatorList(_entities);
            TurnIndicator.SendToLocation(new Vector3(0, 0, 0));
            turnIndicatorManager.ThisEnemy(_entities);
            // RunInfo.Instance.Difficulty += 1;
            StartCoroutine(UpdateTurnIndicators());
            BattleStats.ResetStatsBattle();
            

            
            HexGridManager.Instance.RegisterHexClickCallback(MovePlayerController.StaticHexClickCallback);
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

        IEnumerator UpdateTurnIndicators()
        {
            yield return new WaitForSeconds(0.1f);
            turnIndicatorManager.ThisEnemy(_entities);
        }

        private void StartEntityTurn()
        {
            if (CheckForFinish() != "none") return;
            var entity = CurrentTurn;

            OnEntityTurnStart(entity);

            if (entity is Enemy enemy)
            {
                StartCoroutine(enemy.MakeTurn());
            }
            else if (entity is Player)
            {
            }

            UpdateNextTurnIndicators();
        }
        
        private void EndEntityTurn()
        {
            if (CheckForFinish() != "none") return;
            var entity = CurrentTurn;

            OnEntityTurnEnd(entity);
            entity.EndTurn();
        }

        public void OnEntityTurnStart(AbstractEntity entity)
        {
            entity.StartTurn();

            if (entity is Player)
            {
                StartCoroutine(EnableButtons(.26f));
            }
        }
        
        IEnumerator EnableButtons(float delay)
        {
            yield return new WaitForSeconds(delay);
            EndTurnButton.interactable = true;
            RedrawButton.interactable = true;
        }
        
        public void OnEntityTurnEnd(AbstractEntity entity)
        {
            turnIndicatorManager.NextEnemy(_entities);
            turnIndicatorManager.NextEnemy(_entities);
            
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
            _entities.AddRange(FindObjectsByType<TestEnemy>(FindObjectsSortMode.InstanceID));

            Debug.Log("Difficulty: " + RunInfo.Instance.Difficulty);
            
            _entities.RemoveAll(e => e.Health <= 0);
            


            var spawnSpots = HexGridManager.Instance.BoardDictionary.Keys
                .Where(p => !_entities.Any(e => e.positionRowCol == p))
                .OrderBy(_ => random.Next())
                .Take(numNormalEnemy + numBossEnemy + numHardEnemy)
                .ToList();

            foreach (var pos in spawnSpots)
            {
                EnemyType enemyType = EnemyType.Normal;
                if (numNormalEnemy > 0)
                {
                    enemyType = EnemyType.Normal;
                    numNormalEnemy -= 1;
                }

                if (numHardEnemy > 0)
                {
                    enemyType = EnemyType.Hard;
                    numHardEnemy -= 1;
                }

                if (numBossEnemy > 0)
                {
                    enemyType = EnemyType.Boss;
                    numBossEnemy -= 1;
                }
                
                _entities.Add(SpawnEnemyAt(RunInfo.Instance.Difficulty, pos, enemyType));
            }

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
                e.GetComponent<LerpPosition>().targetLocation = HexGridManager.GetHexCenter(e.positionRowCol.x, e.positionRowCol.y);
                e.transform.position = e.GetComponent<LerpPosition>().targetLocation;

                // DEBUG
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

        private Enemy SpawnEnemyAt(int difficulty, Vector2Int position, EnemyType enemyType)
        {
            EnemyEntry enemyEntry = GetComponent<EnemyData>().GetRandomEnemy(difficulty, enemyType, _entitySpawnRandom);
            GameObject enemyObject = Instantiate(enemyEntry.enemyPrefab, GoList.GetValue("board_container").transform);

            enemyObject.transform.position = HexGridManager.GetHexCenter(position.x, position.y);
            Enemy enemy = enemyObject.GetComponent<Enemy>();

            enemy.positionRowCol = position;

            return enemy;
        }

        private void SetupInitialTiles()
        {
            var origin = new Vector2Int(0, 0);
            _grid.TryAdd(origin, "start");
            _grid.TryAdd(HexGridManager.MoveHex(origin, "n", 1), "basic");
            _grid.TryAdd(HexGridManager.MoveHex(origin, "ne", 1), "basic");
            _grid.TryAdd(HexGridManager.MoveHex(origin, "nw", 1), "basic");
            _grid.TryAdd(HexGridManager.MoveHex(origin, "sw", 1), "basic");
            _grid.TryAdd(HexGridManager.MoveHex(origin, "se", 1), "basic");
            _grid.TryAdd(HexGridManager.MoveHex(origin, "s", 1), "basic");
            _grid.TryAdd(HexGridManager.MoveHex(origin, "s", 2), "basic");
            _grid.UpdateBoard();
        }

        private void SetupUI()
        {
            // gameUI.GetComponent<LerpPosition>().targetLocation = new Vector2(0, 0);
        }

        private void SetupPlayerHand()
        {
            Deck.Instance.DiscardHand();
            Deck.Instance.DrawHand();
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
            EnvironmentManager.instance.ClearPassives();
            
            HexGridManager.Instance.UnregisterHexClickCallback(MovePlayerController.StaticHexClickCallback);
            
            player.Shield = 0;
            BattleStats.ResetStatsBattle();
            
            Debug.Log("Exiting Play State");
            Deck.Instance.DiscardHand();
            List<Enemy> toRemove = new List<Enemy>();
            TurnIndicator.SendToLocation(new Vector3(0, 200, 0));

            foreach (AbstractEntity entity in _entities)
            {
                if (entity is Enemy)
                {
                    Debug.Log("Entity is an enemy!! WOOOOO");
                    toRemove.Add((Enemy)entity);
                }
            }

            foreach (Enemy enemy in toRemove)
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
        }

        #region Turn System ---------------
        public void EntityEndTurn()
        {
            EndEntityTurn();
            if (CheckForFinish() != "none") return;

            _currentTurnIndex = (_currentTurnIndex + 1) % _entities.Count;
            ClearDeadEnemies();

            // Unified start for the next entity
            StartEntityTurn();
        }

        public void ClearDeadEnemies()
        {
            List<AbstractEntity> toRemove = new List<AbstractEntity>();
            
            foreach (AbstractEntity entity in _entities)
            {
                if (entity.Health <= 0)
                {
                    toRemove.Add(entity);


                }
            }
            foreach (AbstractEntity enemy in toRemove)
            {
                _entities.Remove(enemy);
                Destroy(enemy.gameObject);
                
                turnIndicatorManager.UpdateTurnIndicatorList(_entities);
                turnIndicatorManager.ThisEnemy(_entities);
            }
        }

        public void PlayerEndTurn()
        {
            
            BattleStats.ResetStatsTurn();
            if (!(CurrentTurn is Player))
                return;

            if (CheckForFinish() == "player")
            {
                PlayerWon();
                return;
            }

            // Advance to next entity; StartEntityTurn() will be called inside EntityEndTurn
            EntityEndTurn();

            // Refresh player resources for the *next* time the player’s turn comes around
            RunInfo.Instance.CurrentEnergy = RunInfo.Instance.MaxEnergy;
            Deck.Instance.DiscardHand();
            Deck.Instance.DrawHand();
            RunInfo.Instance.Redraws = RunInfo.Instance.maxRedraws;
            
            EndTurnButton.interactable = false;
            RedrawButton.interactable = false;
        }

        private void UpdateNextTurnIndicators()
        {
            foreach (Vector2Int pos in HexGridManager.Instance.GetAllGridPositions())
            {
                GOList list = HexGridManager.Instance.GetWorldHexObject(pos).GetComponent<GOList>();
                list.GetValue("Particles").SetActive(false);
                list.GetValue("Damage").SetActive(false);
            }

            foreach (AbstractEntity entity in _entities)
            {
                if (entity is Enemy && !_entities[_currentTurnIndex].Equals(entity))
                {
                    List<AbstractAction> actions = ((Enemy)entity).NextTurn();

                    Debug.Log("UpdateNextTurnIndicators: " + actions.Count);

                    foreach (AbstractAction action in actions)
                    {
                        try
                        {
                            if (action is AttackAction)
                            {
                                Vector2Int posOfAttack = HexGridManager.MoveHex(entity.positionRowCol,
                                    ((AttackAction)action).Direction, ((AttackAction)action).Distance);
                                GOList list = HexGridManager.Instance.GetWorldHexObject(posOfAttack)
                                    .GetComponent<GOList>();
                                list.GetValue("Particles").SetActive(true);
                                list.GetValue("Damage").SetActive(true);
                                list.GetValue("DamageText").GetComponent<TextMeshProUGUI>().text = ((AttackAction)action).Amount + "";
                            }
                        }
                        catch
                        {
                            
                        }

                    }
                }
            }
        }

        public void PlayerWon()
        {
            Debug.Log("Player has finished");
            GameStateManager.Instance.Change<TilePickState>();

            RunInfo.Instance.Money += RewardMoney;
        }

        public string CheckForFinish()
        {
            bool enemyWin = true;
            bool playerWin = true;
            foreach (AbstractEntity entity in _entities)
            {
                if (entity is Player && entity.Health > 0)
                {
                    enemyWin = false;
                }

                Debug.Log("CheckForFinish: " + entity.Health);
                if (entity is Enemy && entity.Health > 0)
                {
                    playerWin = false;
                }
            }

            Debug.Log("CheckForFinish: " + playerWin);

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

            if (ent is Player)
            {
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

            if (ent is Player)
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
