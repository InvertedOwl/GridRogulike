using System.Collections.Generic;
using System.Linq;
using Cards.Actions;
using Entities;
using Entities.Enemies;
using Grid;
using TMPro;
using Types.Tiles;
using UnityEngine;
using Util;
namespace StateManager
{
    public class PlayingState : GameState
    {
        private readonly List<AbstractEntity> _entities = new();
        private HexGridManager _grid = null!;
        private int _currentTurnIndex;
        public Player player;
        public GameObject gameUI;
        public static int EnemiesToSpawn = 1;
        protected System.Random random = new System.Random();

        public GOList GoList;

        public AbstractEntity CurrentTurn => _entities[_currentTurnIndex];

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

            RunInfo.Instance.Difficulty += 1;
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
        }

        public void OnEntityTurnStart(AbstractEntity entity)
        {
            entity.entityGlow.Glow();
        }
        public void OnEntityTurnEnd(AbstractEntity entity)
        {
            entity.entityGlow.Unglow();
        }

        private void InitializeDeckAndGrid()
        {
            Deck.Instance.UpdateDeck();
            _grid = HexGridManager.Instance;
        }

        private void SetupEntities()
        {
            _entities.Add(player);
            _entities.AddRange(FindObjectsByType<TestEnemy>(FindObjectsSortMode.InstanceID));

            Debug.Log("Difficulty: " + RunInfo.Instance.Difficulty);

            var spawnSpots = HexGridManager.Instance.BoardDictionary.Keys
                .Where(p => !_entities.Any(e => e.positionRowCol == p))
                .OrderBy(_ => random.Next())
                .Take(EnemiesToSpawn)
                .ToList();

            foreach (var pos in spawnSpots)
            {
                _entities.Add(SpawnEnemyAt("test_enemy", pos));
            }

            if (TryGetRandomEmptyHex(out var playerStart))
            {
                player.positionRowCol = playerStart;
            }
            else
            {
                player.positionRowCol = new Vector2Int(0, 0);
                Debug.LogWarning("No empty hexes found for player; defaulting to (0,0).");
            }

            foreach (var e in _entities)
            {
                e.GetComponent<LerpPosition>().targetLocation = HexGridManager.GetHexCenter(e.positionRowCol.x, e.positionRowCol.y);
                e.transform.position = e.GetComponent<LerpPosition>().targetLocation;

                // DEBUG
                e.health = e.initialHealth;
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

        private Enemy SpawnEnemyAt(string enemyName, Vector2Int position)
        {
            GameObject enemyObject = Instantiate(GoList.GetValue(enemyName), GoList.GetValue("board_container").transform);

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
            _grid.UpdateBoard();
        }

        private void SetupUI()
        {
            gameUI.GetComponent<LerpPosition>().targetLocation = new Vector2(0, 0);
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
            Debug.Log("Exiting Play State");
            Deck.Instance.DiscardHand();
            List<Enemy> toRemove = new List<Enemy>();

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

            gameUI.GetComponent<LerpPosition>().targetLocation = new Vector2(0, -750);
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

            // Unified start for the next entity
            StartEntityTurn();
        }

        public void PlayerEndTurn()
        {
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
                if (entity is Enemy)
                {
                    List<AbstractAction> actions = ((Enemy)entity).NextTurn();

                    Debug.Log("UpdateNextTurnIndicators: " + actions.Count);

                    foreach (AbstractAction action in actions)
                    {
                        if (action is AttackAction)
                        {
                            Vector2Int posOfAttack = HexGridManager.MoveHex(entity.positionRowCol, ((AttackAction)action).Direction, ((AttackAction)action).Distance);
                            Debug.Log("Attack at: " + posOfAttack);
                            GOList list = HexGridManager.Instance.GetWorldHexObject(posOfAttack).GetComponent<GOList>();
                            list.GetValue("Particles").SetActive(true);
                            list.GetValue("Damage").SetActive(true);
                            list.GetValue("DamageText").GetComponent<TextMeshProUGUI>().text =
                                ((AttackAction)action).Amount + "";
                        }
                    }
                }
            }
        }

        public void PlayerWon()
        {
            Debug.Log("Player has finished");
            GameStateManager.Instance.Change<TilePickState>();

            // Debug money
            RunInfo.Instance.Money += 100;
        }

        public string CheckForFinish()
        {
            bool enemyWin = true;
            bool playerWin = true;
            foreach (AbstractEntity entity in _entities)
            {
                if (entity is Player && entity.health > 0)
                {
                    enemyWin = false;
                }

                Debug.Log("CheckForFinish: " + entity.health);
                if (entity is Enemy && entity.health > 0)
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
                TileData.tiles[HexGridManager.Instance.HexType(target)].landEvent.Invoke();
            }

            return true;
        }

        public void DamageEntities(Vector2Int coords, int dmg)
        {
            foreach (var e in _entities)
                if (e.positionRowCol == coords) e.Damage(dmg);
        }
        #endregion
    }
}
