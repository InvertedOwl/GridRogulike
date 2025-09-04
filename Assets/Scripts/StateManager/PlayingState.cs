using System.Collections.Generic;
using Entities;
using Entities.Enemies;
using Grid;
using TMPro;
using Types.Actions;
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

        public GOList GoList;

        public AbstractEntity CurrentTurn => _entities[_currentTurnIndex];
        public override void Enter()
        {
            InitializeDeckAndGrid();
            SetupInitialTiles();
            SetupEntities();
            SetupUI();
            SetupPlayerHand();
            EnableTileHovers();
            UpdateNextTurnIndicators();
            RunInfo.Instance.Difficulty += 1;
        }

        private void InitializeDeckAndGrid()
        {
            Deck.Instance.ResetDeck();
            _grid = HexGridManager.Instance;
        }

        private void SetupEntities()
        {
            _entities.Add(player);
            player.positionRowCol = new Vector2Int(0, 0);
            _entities.AddRange(FindObjectsByType<TestEnemy>(FindObjectsSortMode.InstanceID));
            
            Debug.Log("Difficulty: " + RunInfo.Instance.Difficulty);
            
            // Create a list of available points
            
            List<Vector2Int> unoccupiedPoints = new List<Vector2Int>();

            foreach (Vector2Int point in HexGridManager.Instance.BoardDictionary.Keys)
            {
                bool hasEntityOn = false;
                foreach (AbstractEntity entity in _entities)
                {
                    if (entity.positionRowCol.Equals(point))
                    {
                        hasEntityOn = true;
                    }
                }

                if (!hasEntityOn)
                {
                    unoccupiedPoints.Add(point);
                }
            }
            
            foreach (var e in _entities)
            {
                e.GetComponent<LerpPosition>().targetLocation = HexGridManager.GetHexCenter(e.positionRowCol.x, e.positionRowCol.y);
                e.transform.position = e.GetComponent<LerpPosition>().targetLocation;
                
                // DEBUG
                e.health = e.initialHealth;
            }

            foreach (Vector2Int unoccupiedPoint in unoccupiedPoints)
            {
                Debug.Log("Unoccupied point: " + unoccupiedPoint);
                _entities.Add(SpawnEnemyAt("test_enemy", unoccupiedPoint));
            }

            // Punishment for not creating space for enemies. This requires a BAD tile being place
            if (unoccupiedPoints.Count == 0)
            {
                Debug.Log("No unoccupied points");
            }
            

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
            var origin = new Vector2Int(0,0);
            _grid.TryAdd(origin, "start");
            _grid.TryAdd(HexGridManager.MoveHex(origin, "n" , 1), "basic");
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
            Deck.Instance.DiscardHand();
            foreach (AbstractEntity entity in _entities)
            {
                entity.GetComponent<LerpPosition>().targetLocation += new Vector3(0, -750);
            }
            gameUI.GetComponent<LerpPosition>().targetLocation = new Vector2(0, -750);
            RunInfo.Instance.CurrentEnergy = RunInfo.Instance.MaxEnergy;

            foreach (Transform tile in HexGridManager.Instance.grid.transform)
            {
                tile.GetComponent<TileHover>().activeHover = false; 
            }
        }
        
        #region Turn System ---------------
        public void EntityEndTurn()
        {
            if (CheckForFinish() != "none") return;
        
            _currentTurnIndex += 1;
            _currentTurnIndex %= _entities.Count;
            
            if (GameStateManager.Instance.GetCurrent<PlayingState>() is { } playing && CurrentTurn is Enemy)
            {
                StartCoroutine(((Enemy)CurrentTurn).MakeTurn());
            }

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
        
            EntityEndTurn();
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