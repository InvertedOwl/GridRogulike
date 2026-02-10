using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Types.Tiles;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Util;
using UnityEngine.Events;

namespace Grid
{
    [Serializable]
    public class HexClickedEvent : UnityEvent<int, int> { }

    [Serializable]
    public class HexHoveredEvent : UnityEvent<int, int> { }

    public class HexGridManager : MonoBehaviour
    {
        private static float _hexWidth = 1f;

        public GameObject hexPrefab;
        private Dictionary<Vector2Int, string> _boardDictionary = new Dictionary<Vector2Int, string>();
        private Dictionary<Vector2Int, GameObject> _hexObjects = new Dictionary<Vector2Int, GameObject>();
        public Transform grid;
        public static HexGridManager Instance;

        private readonly List<Action<Vector2Int, GameObject>> _hexClickedCallbacks = new();

        private readonly List<Action<Vector2Int, GameObject>> _hexHoverEnterCallbacks = new();
        private readonly List<Action<Vector2Int, GameObject>> _hexHoverExitCallbacks = new();

        [Header("Hex Click Events (Inspector)")]
        public HexClickedEvent onHexClicked;

        [Header("Hex Hover Events (Inspector)")]
        public HexHoveredEvent onHexHoverEnter;
        public HexHoveredEvent onHexHoverExit;

        void Awake()
        {
            Instance = this;
        }

        public void Update()
        {
            Vector3 mousePos = Input.mousePosition;

            Vector2 viewportPos = new Vector2(
                mousePos.x / Screen.width,
                mousePos.y / Screen.height
            );

            Vector2 normalizedPos = new Vector2(
                viewportPos.x * 2f - 1f,
                viewportPos.y * 2f - 1f
            ) * 0.05f;

            Camera.main.transform.parent.GetComponent<LerpPosition>().targetLocation =
                new Vector3(normalizedPos.x, normalizedPos.y, -10);
        }

        public void TryAdd(Vector2Int current, string type)
        {
            if (!_boardDictionary.ContainsKey(current))
            {
                _boardDictionary.Add(current, type);
            }
        }

        public void UpdateBoard()
        {
            if (grid != null)
            {
                var children = new List<GameObject>();
                foreach (Transform child in grid)
                    children.Add(child.gameObject);

                for (int i = 0; i < children.Count; i++)
                {
#if UNITY_EDITOR
                    if (!Application.isPlaying)
                        DestroyImmediate(children[i]);
                    else
                        Destroy(children[i]);
#else
                    Destroy(children[i]);
#endif
                }
            }

            _hexObjects.Clear();

            foreach (var kvp in _boardDictionary)
            {
                var pos = kvp.Key;
                GameObject newHex = GetHexPrefab(HexType(pos), grid);
                newHex.transform.localPosition = GetHexCenter(pos.x, pos.y);
                _hexObjects[pos] = newHex;

                SpriteRenderer displayRenderer = newHex.transform.GetChild(3).GetComponent<SpriteRenderer>();
                displayRenderer.sortingOrder = (int)(newHex.transform.position.y * -3);

                AttachClickForwarder(newHex, pos);

                // NEW: attach hover forwarder too
                AttachHoverForwarder(newHex, pos);
            }
        }

        private void AttachClickForwarder(GameObject hexObj, Vector2Int gridPos)
        {
            var forwarder = hexObj.GetComponent<HexClickForwarder>();
            if (forwarder == null)
                forwarder = hexObj.AddComponent<HexClickForwarder>();

            forwarder.Init(this, gridPos);
        }

        // NEW
        private void AttachHoverForwarder(GameObject hexObj, Vector2Int gridPos)
        {
            var forwarder = hexObj.GetComponent<HexHoverForwarder>();
            if (forwarder == null)
                forwarder = hexObj.AddComponent<HexHoverForwarder>();

            forwarder.Init(this, gridPos);
        }

        internal void NotifyHexClicked(Vector2Int pos, GameObject hexObj)
        {
            onHexClicked?.Invoke(pos.x, pos.y);

            for (int i = 0; i < _hexClickedCallbacks.Count; i++)
            {
                try { _hexClickedCallbacks[i]?.Invoke(pos, hexObj); }
                catch (Exception e) { Debug.LogException(e); }
            }
        }

        // NEW
        internal void NotifyHexHoverEnter(Vector2Int pos, GameObject hexObj)
        {
            onHexHoverEnter?.Invoke(pos.x, pos.y);

            for (int i = 0; i < _hexHoverEnterCallbacks.Count; i++)
            {
                try { _hexHoverEnterCallbacks[i]?.Invoke(pos, hexObj); }
                catch (Exception e) { Debug.LogException(e); }
            }
        }

        // NEW
        internal void NotifyHexHoverExit(Vector2Int pos, GameObject hexObj)
        {
            onHexHoverExit?.Invoke(pos.x, pos.y);

            for (int i = 0; i < _hexHoverExitCallbacks.Count; i++)
            {
                try { _hexHoverExitCallbacks[i]?.Invoke(pos, hexObj); }
                catch (Exception e) { Debug.LogException(e); }
            }
        }

        public void RegisterHexClickCallback(Action<Vector2Int, GameObject> callback)
        {
            if (callback == null) return;
            if (!_hexClickedCallbacks.Contains(callback))
                _hexClickedCallbacks.Add(callback);
        }

        public void UnregisterHexClickCallback(Action<Vector2Int, GameObject> callback)
        {
            if (callback == null) return;
            _hexClickedCallbacks.Remove(callback);
        }

        public void ClearHexClickCallbacks()
        {
            _hexClickedCallbacks.Clear();
        }

        // NEW: hover callback registration
        public void RegisterHexHoverEnterCallback(Action<Vector2Int, GameObject> callback)
        {
            if (callback == null) return;
            if (!_hexHoverEnterCallbacks.Contains(callback))
                _hexHoverEnterCallbacks.Add(callback);
        }

        public void UnregisterHexHoverEnterCallback(Action<Vector2Int, GameObject> callback)
        {
            if (callback == null) return;
            _hexHoverEnterCallbacks.Remove(callback);
        }

        public void RegisterHexHoverExitCallback(Action<Vector2Int, GameObject> callback)
        {
            if (callback == null) return;
            if (!_hexHoverExitCallbacks.Contains(callback))
                _hexHoverExitCallbacks.Add(callback);
        }

        public void UnregisterHexHoverExitCallback(Action<Vector2Int, GameObject> callback)
        {
            if (callback == null) return;
            _hexHoverExitCallbacks.Remove(callback);
        }

        public void ClearHexHoverCallbacks()
        {
            _hexHoverEnterCallbacks.Clear();
            _hexHoverExitCallbacks.Clear();
        }
        
        public Dictionary<Vector2Int, int> CalculateDistanceMap(Vector2Int start, List<Vector2Int> blockers)
        {
            Dictionary<Vector2Int, int> distances = new Dictionary<Vector2Int, int>();

            if (!_boardDictionary.ContainsKey(start))
                return distances;

            Queue<Vector2Int> queue = new Queue<Vector2Int>();

            distances[start] = 0;
            queue.Enqueue(start);

            while (queue.Count > 0)
            {
                Vector2Int current = queue.Dequeue();
                int currentDist = distances[current];

                foreach (Vector2Int neighbor in AdjacentHexes(current))
                {
                    if (!_boardDictionary.ContainsKey(neighbor))
                        continue;

                    if (blockers.Contains(neighbor))
                    {
                        distances[neighbor] = -1;
                        continue;
                    }

                    if (distances.ContainsKey(neighbor))
                        continue;

                    distances[neighbor] = currentDist + 1;
                    queue.Enqueue(neighbor);
                }
            }

            return distances;
        }

        public GameObject GetHexPrefab(string id, Transform parent)
        {
            GameObject newTile = Instantiate(hexPrefab, parent);
            newTile.GetComponent<TileHover>().hoverWhenNotPlaytate = false;
            UpdateHexObject(TileData.tiles[id], newTile);

            return newTile;
        }

        public List<Vector2Int> GetAllGridPositions()
        {
            return _boardDictionary.Keys.ToArray().ToList();
        }

        public GameObject GetWorldHexObject(Vector2Int position)
        {
            return _hexObjects[position];
        }

        public Dictionary<Vector2Int, string> BoardDictionary
        {
            get => _boardDictionary;
        }

        public void UpdateHexObject(TileEntry entry, GameObject tile)
        {
            Color color = entry.color;

            Color darker = new Color(color.r * .6f, color.g * .6f, color.b * .6f);

            GOList goList = tile.GetComponentInChildren<GOList>();

            goList.GetValue("Display1").GetComponent<SpriteRenderer>().color = darker;
            goList.GetValue("Display2").GetComponent<SpriteRenderer>().color = darker;
            goList.GetValue("Display3").GetComponent<SpriteRenderer>().color = darker;
            goList.GetValue("Display4").GetComponent<SpriteRenderer>().color = color;

            goList.GetValue("Title").GetComponent<TextMeshProUGUI>().text = entry.name;
            goList.GetValue("Description").GetComponent<TextMeshProUGUI>().text = entry.description;
            goList.GetValue("HoverBG1").GetComponent<Image>().color = color;
            goList.GetValue("HoverBG2").GetComponent<Image>().color = new Color(color.r * .44f, color.g * .44f, color.b * .44f);
        }

        public static Vector2 GetHexCenter(int col, int row)
        {
            float size = _hexWidth / 2f;
            float hexHeight = Mathf.Sqrt(3f) * size;

            float horizSpacing = 0.75f * _hexWidth;
            float vertSpacing = hexHeight;

            float centerX = col * horizSpacing;
            float centerY = row * vertSpacing + (col % 2 == 0 ? 0 : vertSpacing / 2f);

            return new Vector2(centerX, centerY);
        }

        public static Vector2Int GetHexCoordinates(Vector2 worldPosition)
        {
            float size = _hexWidth / 2f;
            float hexHeight = Mathf.Sqrt(3f) * size;

            float horizSpacing = 0.75f * _hexWidth;
            float vertSpacing = hexHeight;

            int col = Mathf.RoundToInt(worldPosition.x / horizSpacing);
            float rowOffset = (col % 2 == 0) ? 0 : vertSpacing / 2f;
            int row = Mathf.RoundToInt((worldPosition.y - rowOffset) / vertSpacing);

            return new Vector2Int(col, row);
        }

        public static Vector2 GetClosestHexCenter(Vector2 worldPosition)
        {
            Vector2Int coords = GetHexCoordinates(worldPosition);
            return GetHexCenter(coords.x, coords.y);
        }

        public static Vector2Int MoveHex(Vector2Int start, string direction, int distance)
        {
            Vector2Int current = start;
            if (distance == 0)
            {
                return current;
            }

            bool isEvenCol = current.x % 2 == 0;
            Vector2Int delta = direction switch
            {
                "s" => new Vector2Int(0, -1),
                "n" => new Vector2Int(0, 1),
                "sw" => isEvenCol ? new Vector2Int(-1, -1) : new Vector2Int(-1, 0),
                "se" => isEvenCol ? new Vector2Int(+1, -1) : new Vector2Int(+1, 0),
                "ne" => isEvenCol ? new Vector2Int(+1, 0) : new Vector2Int(+1, +1),
                "nw" => isEvenCol ? new Vector2Int(-1, 0) : new Vector2Int(-1, +1),

                _ => throw new System.ArgumentException($"Unknown direction: {direction}")
            };

            current += delta;

            return MoveHex(current, direction, distance - 1);
        }

        public static List<Vector2Int> AdjacentHexes(Vector2Int coords)
        {
            List<Vector2Int> adjacent = new List<Vector2Int>();
            adjacent.Add(MoveHex(coords, "n", 1));
            adjacent.Add(MoveHex(coords, "s", 1));
            adjacent.Add(MoveHex(coords, "ne", 1));
            adjacent.Add(MoveHex(coords, "se", 1));
            adjacent.Add(MoveHex(coords, "nw", 1));
            adjacent.Add(MoveHex(coords, "sw", 1));
            return adjacent;
        }

        public string HexType(Vector2Int coords)
        {
            if (_boardDictionary.ContainsKey(coords))
            {
                return _boardDictionary[coords];
            }

            return "none";
        }
    }
}
