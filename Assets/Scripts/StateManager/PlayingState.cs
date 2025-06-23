using System.Collections.Generic;
using Entities;
using Entities.Enemies;
using Grid;
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

        public AbstractEntity CurrentTurn => _entities[_currentTurnIndex];
        public override void Enter()
        {
            Deck.Instance.ResetDeck();
            _grid = HexGridManager.Instance;

            _entities.Add(player);
            _entities.AddRange(FindObjectsByType<TestEnemy>(FindObjectsSortMode.InstanceID));

            foreach (var e in _entities)
            {
                e.GetComponent<LerpPosition>().targetLocation = HexGridManager.GetHexCenter(e.positionRowCol.x, e.positionRowCol.y);
                e.transform.position = e.GetComponent<LerpPosition>().targetLocation;
                
                // DEBUG
                e.health = e.initialHealth;
            }

            var origin = new Vector2Int(0,0);
            _grid.TryAdd(origin, "start");
            _grid.TryAdd(HexGridManager.MoveHex(origin, "n" , 1), "basic");

            _grid.UpdateBoard();
            gameUI.GetComponent<LerpPosition>().targetLocation = new Vector2(0, 0);
            Deck.Instance.DiscardHand();
            Deck.Instance.DrawHand();
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
            RunInfo.Instance.CurrentEnergy = RunInfo.Instance.maxEnergy;

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
        
            // DEBUG
            if (CurrentTurn is Enemy)
            {
                ((Enemy) CurrentTurn).MakeTurn();
            }
        }
        public void PlayerEndTurn()
        {
            if (CurrentTurn is Player)
            {
                if (CheckForFinish() == "player")
                {
                    PlayerWon();
                    return;
                }
            
                EntityEndTurn();
                RunInfo.Instance.CurrentEnergy = RunInfo.Instance.maxEnergy;
                Deck.Instance.DiscardHand();
                Deck.Instance.DrawHand();
                RunInfo.Instance.Redraws = RunInfo.Instance.maxRedraws;
            }
        }

        public void PlayerWon()
        {
            Debug.Log("Player has finished");
            GameStateManager.Instance.Change<TilePickState>();
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

                if (entity is Enemy && entity.health > 0)
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