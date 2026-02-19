using System;
using System.Collections.Generic;
using System.Linq;
using Grid;
using Types.Tiles;
using UnityEngine;
using Util;
using Random = System.Random;

namespace StateManager
{
    public class TilePickState : GameState
    {
        public GameObject window;
        public List<string> choices;
        private GameObject _newTile;
        private HexGridManager _grid;
        private int _chosenIndex;

        public Random tilePickRandom = RunInfo.NewRandom("tilepick".GetHashCode());
        
        public List<GameObject> tiles;
        public override void Enter()
        {
            _grid = HexGridManager.Instance;
            window.GetComponent<LerpPosition>().targetLocation = new Vector2(0, 0);
            List<string> idList = TileData.tiles.Keys.Where((t) => TileData.tiles[t].canAppearInShop).ToList();
            choices = idList.OrderBy(x => tilePickRandom.Next()).Take(3).ToList();
            for (int i = 0; i < choices.Count; i++)
            {
                HexGridManager.Instance.UpdateHexObject(TileData.tiles[choices[i]], tiles[i]);
            }
        }

        public void Choose(int index)
        {
            AreYouSure.Instance.AskConfirm((confirm) =>
            {
                if (!confirm) return;
                
                window.GetComponent<LerpPosition>().targetLocation = new Vector2(0, 730);
                // Now they have to place it
                _newTile = Instantiate(_grid.GetHexPrefab(choices[index], window.transform));
                _newTile.AddComponent<LerpPosition>();
                _newTile.GetComponent<LerpPosition>().speed = 40;
                _chosenIndex = index;
            });
            

        }

        public void Update()
        {
            if (!_newTile)
                return;

            _newTile.GetComponent<GOList>().GetValue("Display4").GetComponent<SpriteRenderer>().sortingOrder = 500;

            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0f;
            Vector2Int gridPos = HexGridManager.GetHexCoordinates(mouseWorldPos);
            // Either replacing, or adding new
            if (_grid.HexType(gridPos) != "none" || (_grid.HexType(gridPos) == "none" &&
                                                     HexGridManager.AdjacentHexes(gridPos).Any(pos => _grid.HexType(pos) != "none")))
            {
                _newTile.GetComponent<LerpPosition>().targetLocation = HexGridManager.GetHexCenter(gridPos.x, gridPos.y);

                if (Input.GetMouseButtonDown(0))
                {
                    
                    _grid.Replace(gridPos, choices[_chosenIndex]);
                    _grid.UpdateBoard();
                    Destroy(_newTile);
                    _newTile = null;
                    GameStateManager.Instance.Change<ShopState>();
                }
            }

        }

        public override void Exit()
        {
            window.GetComponent<LerpPosition>().targetLocation = new Vector2(0, 730);
        }
    }
}