using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ScriptableObjects;
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
        public static readonly string[] HexDirections = { "e", "ne", "nw", "w", "sw", "se" };

        public static readonly Dictionary<string, List<string>> neighborDirections = new Dictionary<string, List<string>>()
        {
            ["e"] = new List<string> { "ne", "se" },
            ["ne"] = new List<string> { "e", "nw" },
            ["nw"] = new List<string> { "ne", "w" },
            ["w"] = new List<string> { "nw", "sw" },
            ["sw"] = new List<string> { "w", "se" },
            ["se"] = new List<string> { "sw", "e" },
        };
        
        private static float _hexWidth = 1f;
        private const int OrthogonalLayerCount = 5;

        public GameObject hexPrefab;
        private Dictionary<Vector2Int, string> _boardDictionary = new Dictionary<Vector2Int, string>();
        private Dictionary<Vector2Int, int> _heightDictionary = new Dictionary<Vector2Int, int>();
        public Dictionary<Vector2Int, GameObject> _hexObjects = new Dictionary<Vector2Int, GameObject>();
        public Transform grid;
        public static HexGridManager Instance;
        private static MapData _saveData;
        private readonly Dictionary<Transform, Vector3> _tileBaseScales = new Dictionary<Transform, Vector3>();
        private const float BasicTileEnvironmentSaturationScale = 0.65f;
        private const float BasicTileEnvironmentMinimumSaturation = 0.35f;
        private const float BasicTileEnvironmentValueBoost = 0.07f;
        private const float BasicTileEnvironmentMinimumValue = 0.22f;
        
        public SpriteDatabase spriteDatabase;

        private readonly List<Action<Vector2Int, GameObject>> _hexClickedCallbacks = new();

        private readonly List<Action<Vector2Int, GameObject>> _hexHoverEnterCallbacks = new();
        private readonly List<Action<Vector2Int, GameObject>> _hexHoverExitCallbacks = new();

        [Header("Hex Click Events (Inspector)")]
        public HexClickedEvent onHexClicked;

        [Header("Hex Hover Events (Inspector)")]
        public HexHoveredEvent onHexHoverEnter;
        public HexHoveredEvent onHexHoverExit;

        [SerializeField] private float tileOrthogonalSeparation = 0.001f;
        private const float TileHeightZOffset = -0.25f;

        void Awake ()
        {
            Instance = this;
            if (_saveData != null)
            {
                RestoreFromSaveData(_saveData);
                _saveData = null;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ResetStaticsOnLoad()
        {
            ResetStatics();
        }

        public static void ResetStatics()
        {
            Instance = null;
            _saveData = null;
        }

        public void Update()
        {
            // Vector3 mousePos = Input.mousePosition;
            //
            // Vector2 viewportPos = new Vector2(
            //     mousePos.x / Screen.width,
            //     mousePos.y / Screen.height
            // );
            //
            // Vector2 normalizedPos = new Vector2(
            //     viewportPos.x * 2f - 1f,
            //     viewportPos.y * 2f - 1f
            // ) * 0.05f;
            //
            // Camera.main.transform.parent.GetComponent<LerpPosition>().targetLocation =
            //     new Vector3(normalizedPos.x, normalizedPos.y, -10);
        }

        public void TryAdd(Vector2Int current, string type, int height = 0)
        {
            if (!_boardDictionary.ContainsKey(current))
            {
                _boardDictionary.Add(current, type);
                _heightDictionary[current] = Mathf.Max(0, height);
            }
        }

        public void Replace(Vector2Int current, string type)
        {
            _boardDictionary[current] = type;
            if (!_heightDictionary.ContainsKey(current))
                _heightDictionary[current] = 0;
        }

        public int GetHeight(Vector2Int position)
        {
            return _heightDictionary.TryGetValue(position, out int height) ? height : 0;
        }

        public void SetHeight(Vector2Int position, int height)
        {
            if (!_boardDictionary.ContainsKey(position))
                return;

            _heightDictionary[position] = Mathf.Max(0, height);

            if (_hexObjects.TryGetValue(position, out GameObject tile) && tile != null)
            {
                Vector3 targetPosition = GetHexCenterWithOrthogonalOffset(
                    position.x,
                    position.y,
                    tileOrthogonalSeparation,
                    _heightDictionary[position]);
                AnimateTileToPosition(tile, targetPosition);
            }
        }

        private static void AnimateTileToPosition(GameObject tile, Vector3 localPosition)
        {
            EasePosition easePosition = tile.GetComponent<EasePosition>();
            if (easePosition != null)
            {
                easePosition.targetLocation = easePosition.isLocal || tile.transform.parent == null
                    ? localPosition
                    : tile.transform.parent.TransformPoint(localPosition);
                return;
            }

            LerpPosition lerpPosition = tile.GetComponent<LerpPosition>();
            if (lerpPosition != null)
            {
                lerpPosition.targetLocation = lerpPosition.isLocal || tile.transform.parent == null
                    ? localPosition
                    : tile.transform.parent.TransformPoint(localPosition);
                return;
            }

            easePosition = tile.AddComponent<EasePosition>();
            easePosition.isLocal = true;
            easePosition.targetLocation = localPosition;
        }

        public void ResetAllHeights()
        {
            foreach (Vector2Int position in _boardDictionary.Keys)
            {
                SetHeight(position, 0);
            }
        }

        public MapData CaptureSaveData()
        {
            MapData data = new MapData();

            foreach (var kvp in _boardDictionary.OrderBy(kvp => kvp.Key.x).ThenBy(kvp => kvp.Key.y))
            {
                data.entries.Add(new MapEntry
                {
                    key = kvp.Key,
                    value = kvp.Value,
                    height = GetHeight(kvp.Key)
                });
            }

            return data;
        }

        public void RestoreFromSaveData(MapData data)
        {
            if (data == null)
            {
                return;
            }

            _boardDictionary = data.ToDictionary();
            _heightDictionary = data.ToHeightDictionary();
            UpdateBoard();
        }

        public static void LoadFromSaveData(MapData data)
        {
            _saveData = data;
            if (Instance != null)
            {
                Instance.RestoreFromSaveData(_saveData);
                _saveData = null;
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
            _tileBaseScales.Clear();

            foreach (var kvp in _boardDictionary)
            {
                var pos = kvp.Key;
                GameObject newHex = GetHexPrefab(HexType(pos), grid);
                newHex.transform.localPosition = GetHexCenterWithOrthogonalOffset(
                    pos.x,
                    pos.y,
                    tileOrthogonalSeparation);
                _hexObjects[pos] = newHex;

                SpriteRenderer displayRenderer = newHex.transform.GetChild(3).GetComponent<SpriteRenderer>();
                displayRenderer.sortingOrder = (int)(newHex.transform.position.y * -3);

                newHex.GetComponent<HexPreviewHandler>().currentPos = pos;

                AttachClickForwarder(newHex, pos);
                AttachHoverForwarder(newHex, pos);
                SetHeight(pos, GetHeight(pos));
            }

            SpawnBG.instance?.RefreshDecorations();
        }

        private void AttachClickForwarder(GameObject hexObj, Vector2Int gridPos)
        {
            var forwarder = hexObj.GetComponent<HexClickForwarder>();
            if (forwarder == null)
                forwarder = hexObj.AddComponent<HexClickForwarder>();

            forwarder.Init(this, gridPos);
        }

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
        
        public Dictionary<Vector2Int, int> CalculateDistanceMap(
            Vector2Int start,
            List<Vector2Int> blockers,
            bool includeElevationCosts = false)
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

                    int candidateDistance = currentDist +
                                            (includeElevationCosts ? GetMovementCost(current, neighbor) : 1);

                    if (distances.TryGetValue(neighbor, out int existingDistance) &&
                        (existingDistance < 0 || existingDistance <= candidateDistance))
                    {
                        continue;
                    }

                    distances[neighbor] = candidateDistance;
                    queue.Enqueue(neighbor);
                }
            }

            return distances;
        }

        public int GetMovementCost(Vector2Int from, Vector2Int to)
        {
            return GetHeight(from) == GetHeight(to) ? 1 : 2;
        }

        public GameObject GetHexPrefab(string id, Transform parent)
        {
            GameObject newTile = Instantiate(hexPrefab, parent);
            TileHover tileHover = newTile.GetComponent<TileHover>();
            tileHover.hoverWhenNotPlaytate = false;
            tileHover.tileType = id;
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

        public IReadOnlyDictionary<Vector2Int, int> HeightDictionary => _heightDictionary;

        public void UpdateHexObject(TileEntry entry, GameObject tile)
        {
            Color color = SpawnBG.RandomizeColor(entry.color.ToColor(), 0.001f, 0.01f, 0.05f);

            GOList goList = tile.GetComponentInChildren<GOList>();
            Transform stepsHere = tile.GetComponentsInChildren<Transform>(true)
                .FirstOrDefault(child => child.name == "StepsHere");
            if (stepsHere != null)
                stepsHere.gameObject.SetActive(false);

            SetHexObjectDisplayColors(tile, color);

            goList.GetValue("Title").GetComponent<TextMeshProUGUI>().text = entry.name;
            goList.GetValue("Description").GetComponent<TextMeshProUGUI>().text = entry.description;
            // goList.GetValue("HoverBG1").GetComponent<Image>().color = color;
            // goList.GetValue("HoverBG2").GetComponent<Image>().color = new Color(color.r * .44f, color.g * .44f, color.b * .44f);
            try
            {
                goList.GetValue("IconHover").GetComponent<Image>().sprite = spriteDatabase.Get(entry.icon).Value.sprite;

            }
            catch (Exception e)
            {

            }

            TextMeshProUGUI titleText = goList.GetValue("Title").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI descriptionText = goList.GetValue("Description").GetComponent<TextMeshProUGUI>();

            if (entry.pairedTilesEntry != null)
            {
                goList.GetValue("PairedContainer").SetActive(true);
                goList.GetValue("PairedText").GetComponent<TextMeshProUGUI>().text = entry.pairedTilesEntry.description;
                goList.GetValue("PairedAmount").GetComponent<TextMeshProUGUI>().text = entry.pairedTilesEntry.PairCount.ToString();
            }

            titleText.text = entry.name;
            descriptionText.text = entry.description;

            ForceRebuildTMPLayout(titleText);
            ForceRebuildTMPLayout(descriptionText);

            // Rebuild the layout roots, not Display4.
            RebuildParents(descriptionText.rectTransform);
            RebuildParents(titleText.rectTransform);
            
            SetHexObjectIcon(tile, entry.icon);
        }

        public void AnimateTilesWithBackgroundColors(
            List<Color> colors,
            Vector2Int centerCoords,
            float radialRingPause,
            float flipDuration,
            float colorChangeDelay)
        {
            if (colors == null || colors.Count == 0 || _hexObjects.Count == 0)
                return;

            StartCoroutine(AnimateTilesWithBackgroundColorsCoroutine(
                colors,
                centerCoords,
                radialRingPause,
                flipDuration,
                colorChangeDelay));
        }

        private IEnumerator AnimateTilesWithBackgroundColorsCoroutine(
            List<Color> colors,
            Vector2Int centerCoords,
            float radialRingPause,
            float flipDuration,
            float colorChangeDelay)
        {
            List<BoardTileAnimationEntry> tiles = GetBoardTilesByRadialRing(centerCoords);
            int currentRing = -1;

            foreach (BoardTileAnimationEntry entry in tiles)
            {
                if (entry.Ring != currentRing)
                {
                    if (currentRing >= 0)
                        yield return new WaitForSeconds(Mathf.Max(0f, radialRingPause));

                    currentRing = entry.Ring;
                }

                AnimateTileFlip(entry.Tile, flipDuration);

                if (entry.TileType == "basic")
                {
                    Color color = colors[UnityEngine.Random.Range(0, colors.Count)];
                    StartCoroutine(SetBasicTileColorAfterDelay(entry.Tile, color, colorChangeDelay));
                }
            }
        }

        private List<BoardTileAnimationEntry> GetBoardTilesByRadialRing(Vector2Int centerCoords)
        {
            List<BoardTileAnimationEntry> tiles = new List<BoardTileAnimationEntry>();

            foreach (var kvp in _hexObjects)
            {
                Vector2Int position = kvp.Key;
                GameObject tile = kvp.Value;

                if (tile == null)
                    continue;

                _boardDictionary.TryGetValue(position, out string tileType);
                tiles.Add(new BoardTileAnimationEntry
                {
                    Tile = tile,
                    TileType = tileType,
                    Ring = HexDistance(centerCoords, position)
                });
            }

            tiles.Sort((a, b) => a.Ring.CompareTo(b.Ring));
            return tiles;
        }

        private void AnimateTileFlip(GameObject tile, float flipDuration)
        {
            if (tile == null)
                return;

            Transform tileTransform = tile.transform;
            if (!_tileBaseScales.TryGetValue(tileTransform, out Vector3 baseScale))
            {
                baseScale = tileTransform.localScale;
                _tileBaseScales[tileTransform] = baseScale;
            }

            EaseScale easeScale = tile.GetComponent<EaseScale>();
            if (easeScale == null)
                easeScale = tile.AddComponent<EaseScale>();

            easeScale.durationSeconds = Mathf.Max(0f, flipDuration);
            easeScale.SetScale(new Vector3(-baseScale.x, baseScale.y, baseScale.z), () =>
            {
                if (tileTransform != null && _tileBaseScales.TryGetValue(tileTransform, out Vector3 cachedScale))
                    tileTransform.localScale = cachedScale;
            });
        }

        private IEnumerator SetBasicTileColorAfterDelay(GameObject tile, Color backgroundColor, float colorChangeDelay)
        {
            yield return new WaitForSeconds(Mathf.Max(0f, colorChangeDelay));

            if (tile == null)
                yield break;

            Color color = GetBasicTileColorFromBackgroundColor(
                SpawnBG.RandomizeColor(backgroundColor, 0.001f, 0.005f, 0.01f));

            SetHexObjectDisplayColors(tile, color);
        }

        private void SetHexObjectDisplayColors(GameObject tile, Color color)
        {
            GOList goList = tile.GetComponentInChildren<GOList>();
            if (goList == null)
                return;

            color.a = 1f;
            Color darker = new Color(color.r * .6f, color.g * .6f, color.b * .6f, 1f);

            if (goList.HasValue("Display2"))
                goList.GetValue("Display2").GetComponent<SpriteRenderer>().color = darker;

            if (goList.HasValue("Display3"))
                goList.GetValue("Display3").GetComponent<SpriteRenderer>().color = darker;

            if (goList.HasValue("Display4"))
                goList.GetValue("Display4").GetComponent<SpriteRenderer>().color = color;
        }

        private Color GetBasicTileColorFromBackgroundColor(Color backgroundColor)
        {
            Color.RGBToHSV(backgroundColor, out float h, out float s, out float v);

            if (s > 0.05f)
                s = Mathf.Max(s * BasicTileEnvironmentSaturationScale, BasicTileEnvironmentMinimumSaturation);

            v = Mathf.Max(v + BasicTileEnvironmentValueBoost, BasicTileEnvironmentMinimumValue);

            Color color = Color.HSVToRGB(h, s, v);
            color.a = 1f;
            return color;
        }

        private struct BoardTileAnimationEntry
        {
            public GameObject Tile;
            public string TileType;
            public int Ring;
        }

        private void ForceRebuildTMPLayout(TextMeshProUGUI tmp)
        {
            if (tmp == null)
                return;

            tmp.ForceMeshUpdate();

            RectTransform rect = tmp.rectTransform;

            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);

            ContentSizeFitter fitter = rect.GetComponent<ContentSizeFitter>();
            if (fitter != null)
            {
                fitter.SetLayoutVertical();
                fitter.SetLayoutHorizontal();
            }
        }

        private void RebuildParents(RectTransform start)
        {
            if (start == null)
                return;

            Canvas.ForceUpdateCanvases();

            RectTransform current = start;

            while (current != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(current);

                ContentSizeFitter fitter = current.GetComponent<ContentSizeFitter>();
                if (fitter != null)
                {
                    fitter.SetLayoutVertical();
                    fitter.SetLayoutHorizontal();
                }

                current = current.parent as RectTransform;
            }

            Canvas.ForceUpdateCanvases();
        }

        public void SetHexIcon(Vector2Int position, string icon)
        {
            if (!_hexObjects.TryGetValue(position, out GameObject tile) || tile == null)
                return;

            SetHexObjectIcon(tile, icon);
        }

        private void SetHexObjectIcon(GameObject tile, string icon)
        {
            GOList goList = tile.GetComponentInChildren<GOList>();
            if (goList == null || !goList.HasValue("HexIconParent") || !goList.HasValue("HexIcon"))
                return;

            GameObject iconParent = goList.GetValue("HexIconParent");
            GameObject iconObj = goList.GetValue("HexIcon");
            if (iconParent == null || iconObj == null)
                return;

            SpriteRenderer iconRenderer = iconObj.GetComponent<SpriteRenderer>();
            if (iconRenderer == null)
                return;

            if (string.IsNullOrEmpty(icon) || icon == "none" || spriteDatabase == null)
            {
                iconParent.SetActive(false);
                iconRenderer.sprite = null;
                return;
            }

            iconParent.SetActive(true);

            Sprite sprite = spriteDatabase.Get(icon).Value.sprite;
            iconRenderer.sprite = sprite;

            NormalizeSpriteRendererSize(iconRenderer, 0.3f); // target size in world units
        }

        private void NormalizeSpriteRendererSize(SpriteRenderer sr, float targetMaxSize)
        {
            if (sr.sprite == null) return;

            Vector2 spriteSize = sr.sprite.bounds.size;

            float maxDimension = Mathf.Max(spriteSize.x, spriteSize.y);
            if (maxDimension <= 0f) return;

            float scale = targetMaxSize / maxDimension;
            sr.transform.localScale = new Vector3(scale, scale, 1f);
        }

        public static Vector2 GetHexCenter(int col, int row)
        {
            float hexHeight = _hexWidth;
            float hexWidth = Mathf.Sqrt(3f) * hexHeight / 2f;

            float horizSpacing = hexWidth;
            float vertSpacing = 0.75f * hexHeight;

            float rowOffset = IsOdd(row) ? horizSpacing / 2f : 0f;
            float centerX = col * horizSpacing + rowOffset;
            float centerY = row * vertSpacing;

            return new Vector2(centerX, centerY);
        }

        public static Vector3 GetHexCenterWithOrthogonalOffset(
            int col,
            int row,
            float orthogonalSeparation,
            int height = 0)
        {
            Vector2 center = GetHexCenter(col, row);
            float z = GetHexOrthogonalOffset(col, row, orthogonalSeparation) +
                      GetTileHeightWorldOffset(height);
            return new Vector3(center.x, center.y, z);
        }

        public static float GetTileHeightWorldOffset(int height)
        {
            return Mathf.Max(0, height) * TileHeightZOffset;
        }

        private static float GetHexOrthogonalOffset(int col, int row, float orthogonalSeparation)
        {
            if (orthogonalSeparation <= 0f)
                return 0f;

            int layer = PositiveModulo(col + (row * 2), OrthogonalLayerCount);
            float centeredLayer = layer - ((OrthogonalLayerCount - 1) * 0.5f);
            return centeredLayer * orthogonalSeparation;
        }

        private static int PositiveModulo(int value, int modulus)
        {
            return ((value % modulus) + modulus) % modulus;
        }

        public static Vector2Int GetHexCoordinates(Vector2 worldPosition)
        {
            float hexHeight = _hexWidth;
            float hexWidth = Mathf.Sqrt(3f) * hexHeight / 2f;

            float horizSpacing = hexWidth;
            float vertSpacing = 0.75f * hexHeight;

            int roughRow = Mathf.RoundToInt(worldPosition.y / vertSpacing);
            float rowOffset = IsOdd(roughRow) ? horizSpacing / 2f : 0f;
            int roughCol = Mathf.RoundToInt((worldPosition.x - rowOffset) / horizSpacing);

            Vector2Int closest = new Vector2Int(roughCol, roughRow);
            float closestDistance = float.MaxValue;

            for (int row = roughRow - 1; row <= roughRow + 1; row++)
            {
                for (int col = roughCol - 1; col <= roughCol + 1; col++)
                {
                    Vector2Int candidate = new Vector2Int(col, row);
                    float distance = Vector2.SqrMagnitude(worldPosition - GetHexCenter(candidate.x, candidate.y));

                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closest = candidate;
                    }
                }
            }

            return closest;
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

            bool isOddRow = IsOdd(current.y);
            Vector2Int delta = direction switch
            {
                "e" => new Vector2Int(1, 0),
                "w" => new Vector2Int(-1, 0),
                "ne" => isOddRow ? new Vector2Int(1, 1) : new Vector2Int(0, 1),
                "nw" => isOddRow ? new Vector2Int(0, 1) : new Vector2Int(-1, 1),
                "se" => isOddRow ? new Vector2Int(1, -1) : new Vector2Int(0, -1),
                "sw" => isOddRow ? new Vector2Int(0, -1) : new Vector2Int(-1, -1),

                _ => throw new System.ArgumentException($"Unknown direction: {direction}")
            };

            current += delta;

            return MoveHex(current, direction, distance - 1);
        }

        public static List<Vector2Int> AdjacentHexes(Vector2Int coords)
        {
            List<Vector2Int> adjacent = new List<Vector2Int>();
            foreach (string direction in HexDirections)
            {
                adjacent.Add(MoveHex(coords, direction, 1));
            }
            return adjacent;
        }

        public static List<Vector2Int> HexesInRadius(int radius)
        {
            List<Vector2Int> hexes = new List<Vector2Int>();
            radius = Mathf.Max(0, radius);

            for (int row = -radius; row <= radius; row++)
            {
                for (int col = -radius; col <= radius; col++)
                {
                    if (HexDistanceFromOrigin(col, row) <= radius)
                    {
                        hexes.Add(new Vector2Int(col, row));
                    }
                }
            }

            return hexes;
        }

        private static int HexDistanceFromOrigin(int col, int row)
        {
            int rowParity = Mathf.Abs(row % 2);
            int q = col - ((row - rowParity) / 2);
            int r = row;
            int s = -q - r;

            return Mathf.Max(Mathf.Abs(q), Mathf.Abs(r), Mathf.Abs(s));
        }

        private static int HexDistance(Vector2Int a, Vector2Int b)
        {
            Vector3Int cubeA = OddRowOffsetToCube(a);
            Vector3Int cubeB = OddRowOffsetToCube(b);

            return Mathf.Max(
                Mathf.Abs(cubeA.x - cubeB.x),
                Mathf.Abs(cubeA.y - cubeB.y),
                Mathf.Abs(cubeA.z - cubeB.z)
            );
        }

        private static Vector3Int OddRowOffsetToCube(Vector2Int coords)
        {
            int rowParity = Mathf.Abs(coords.y % 2);
            int q = coords.x - ((coords.y - rowParity) / 2);
            int r = coords.y;
            int s = -q - r;

            return new Vector3Int(q, r, s);
        }

        private static bool IsOdd(int value)
        {
            return Mathf.Abs(value % 2) == 1;
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
