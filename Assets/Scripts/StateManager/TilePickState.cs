using System;
using System.Collections.Generic;
using System.Linq;
using Grid;
using Types.Tiles;
using UnityEngine;
using Util;

namespace StateManager
{
    public class TilePickState : GameState
    {
        public GameObject window;
        public List<string> choices;
        private GameObject _newTile;
        private HexGridManager _grid;
        private int _chosenIndex;

        public RandomState tilePickRandom = RunInfo.NewRandom("tilepick".GetHashCode());

        public List<GameObject> tiles;

        [Header("3D Placement")]
        [SerializeField] private float boardZ = 0f;          // Board lies on XY plane at this Z
        [SerializeField] private float previewDepthOffset = -0.05f; // Slight offset toward camera if needed

        public override void Enter()
        {
            _grid = HexGridManager.Instance;
            window.GetComponent<LerpPosition>().targetLocation = new Vector2(0, 0);

            List<string> idList = TileData.tiles.Keys
                .Where(t => TileData.tiles[t].canAppearInShop)
                .ToList();

            choices = idList
                .OrderBy(x => tilePickRandom.Next())
                .Take(3)
                .ToList();

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

                // Don't parent world object to UI in 3D
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

            GOList goList = _newTile.GetComponent<GOList>();
            if (goList != null)
            {
                GameObject display = goList.GetValue("Display4");
                if (display != null)
                {
                    SpriteRenderer sr = display.GetComponent<SpriteRenderer>();
                    if (sr != null)
                        sr.sortingOrder = 500;
                }
            }

            if (!TryGetMouseBoardPosition(out Vector3 mouseWorldPos))
                return;

            Vector2Int gridPos = HexGridManager.GetHexCoordinates(mouseWorldPos);

            bool canPlace =
                _grid.HexType(gridPos) != "none" ||
                (_grid.HexType(gridPos) == "none" &&
                 HexGridManager.AdjacentHexes(gridPos).Any(pos => _grid.HexType(pos) != "none"));

            if (canPlace)
            {
                Vector3 target = HexGridManager.GetHexCenter(gridPos.x, gridPos.y);

                // Keep preview slightly in front of board, if needed
                target.z += previewDepthOffset;

                _newTile.GetComponent<LerpPosition>().targetLocation = target;

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

        private bool TryGetMouseBoardPosition(out Vector3 worldPos)
        {
            worldPos = Vector3.zero;

            Camera cam = Camera.main;
            if (cam == null)
                return false;

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            // XY board at fixed Z
            Plane boardPlane = new Plane(Vector3.forward, new Vector3(0f, 0f, boardZ));

            if (boardPlane.Raycast(ray, out float enter))
            {
                worldPos = ray.GetPoint(enter);
                worldPos.z = boardZ;
                return true;
            }

            return false;
        }

        public override void Exit()
        {
            window.GetComponent<LerpPosition>().targetLocation = new Vector2(0, 730);
        }
    }
}