using System.Collections;
using System.Collections.Generic;
using Grid;
using UnityEditor;
using UnityEngine;

public class SpawnBG : MonoBehaviour
{
    public GameObject hexPrefab;

    [SerializeField] private int startX = -10;
    [SerializeField] private int startY = -10;
    [SerializeField] private int widthX = 20;
    [SerializeField] private int widthY = 20;

    public List<Color> grasslandColors = new List<Color>();

    public List<Color> currentColors = new List<Color>();

    public static SpawnBG instance;
    public bool IsColorAnimationRunning => _activeColorAnimations > 0;
    private int _activeColorAnimations;

    // Fallback wait if a tile has no EaseScale to report its own duration.
    [SerializeField] private float flipAnimDuration = 0.2f;
    [SerializeField] private float radialRingPause = 0.04f;

    public void Awake ()
    {
        instance = this;
    }

    // This class is allowed to use UnityEngine.Random because the BG colors
    // are non-consequential and probably shouldn't be seeded anyway.
    void Start()
    {
        DestroyBackground();
        SpawnBackground();
    }

    private void DestroyBackground()
    {
        foreach (Transform hex in transform)
        {
            DestroyImmediate(hex.gameObject);
        }
    }

    private void SpawnBackground()
    {
        for (int y = startY; y < startY + widthY; y++)
        {
            for (int x = startX; x < startX + widthX; x++)
            {
                GameObject hex = Instantiate(hexPrefab, transform);
                hex.transform.localPosition = HexGridManager.GetHexCenter(x, y);

                hex.GetComponent<BGTile>().SetColor(
                    RandomizeColor(grasslandColors[Random.Range(0, grasslandColors.Count)])
                );
            }
        }
    }

    public static Color RandomizeColor(Color original, float hueShift = 0.002f, float satShift = 0.02f, float valShift = 0.02f)
    {
        Color.RGBToHSV(original, out float h, out float s, out float v);

        h += Random.Range(-hueShift, hueShift);
        h = (h + 1f) % 1f;

        s = Mathf.Clamp01(s + Random.Range(-satShift, satShift));
        v = Mathf.Clamp01(v + Random.Range(-valShift, valShift));

        return Color.HSVToRGB(h, s, v);
    }

    [ContextMenu("Refresh Grid")]
    public void RefreshGrid()
    {
        DestroyBackground();
        SpawnBackground();
    }

    public void SetColorAnimation()
    {
        StartCoroutine(ColorAnimationRadial());
    }

    IEnumerator ColorAnimationRadial()
    {
        _activeColorAnimations++;

        List<Color> colors = new List<Color>();

        if (currentColors.Count != 0)
        {
            colors.AddRange(currentColors);
        }
        else
        {
            colors.AddRange(grasslandColors);
        }
        
        List<RadialTileEntry> tiles = GetTilesByRadialRing();
        int currentRing = -1;
        float longestFlipDuration = 0f;

        foreach (RadialTileEntry entry in tiles)
        {
            if (entry.Ring != currentRing)
            {
                if (currentRing >= 0)
                    yield return new WaitForSeconds(Mathf.Max(0f, radialRingPause));

                currentRing = entry.Ring;
            }

            BGTile tile = entry.Tile;
            if (tile == null)
                continue;

            // Animate to -1 (flip)
            EaseScale ease = tile.GetComponent<EaseScale>();
            if (ease != null)
            {
                Transform tileTransform = tile.transform;
                longestFlipDuration = Mathf.Max(longestFlipDuration, ease.durationSeconds);
                ease.SetScale(new Vector3(-1, 1, 1), () => SnapScaleBackToOne(tileTransform));
            }

            StartCoroutine(SetBGColor(tile, colors[Random.Range(0, colors.Count)]));
        }

        yield return new WaitForSeconds(Mathf.Max(longestFlipDuration, flipAnimDuration, 0.1f));
        _activeColorAnimations = Mathf.Max(0, _activeColorAnimations - 1);
    }

    private List<RadialTileEntry> GetTilesByRadialRing()
    {
        List<RadialTileEntry> tiles = new List<RadialTileEntry>();
        int count = transform.childCount;

        if (count == 0 || widthX <= 0)
            return tiles;

        Vector2Int center = GetBackgroundCenterCoords();

        for (int i = 0; i < count; i++)
        {
            Transform child = transform.GetChild(i);
            BGTile tile = child.GetComponentInChildren<BGTile>();

            if (tile == null)
                continue;

            Vector2Int coords = GetCoordsForChildIndex(i);
            int ring = HexDistance(center, coords);

            tiles.Add(new RadialTileEntry
            {
                Tile = tile,
                Ring = ring,
                Index = i
            });
        }

        tiles.Sort((a, b) =>
        {
            int ringCompare = a.Ring.CompareTo(b.Ring);
            return ringCompare != 0 ? ringCompare : a.Index.CompareTo(b.Index);
        });

        return tiles;
    }

    private Vector2Int GetBackgroundCenterCoords()
    {
        return new Vector2Int(startX + widthX / 2, startY + widthY / 2);
    }

    private Vector2Int GetCoordsForChildIndex(int index)
    {
        return new Vector2Int(startX + index % widthX, startY + index / widthX);
    }

    private int HexDistance(Vector2Int a, Vector2Int b)
    {
        Vector3Int cubeA = OddRowOffsetToCube(a);
        Vector3Int cubeB = OddRowOffsetToCube(b);

        return Mathf.Max(
            Mathf.Abs(cubeA.x - cubeB.x),
            Mathf.Abs(cubeA.y - cubeB.y),
            Mathf.Abs(cubeA.z - cubeB.z)
        );
    }

    private Vector3Int OddRowOffsetToCube(Vector2Int coords)
    {
        int rowParity = Mathf.Abs(coords.y % 2);
        int q = coords.x - ((coords.y - rowParity) / 2);
        int r = coords.y;
        int s = -q - r;

        return new Vector3Int(q, r, s);
    }

    private struct RadialTileEntry
    {
        public BGTile Tile;
        public int Ring;
        public int Index;
    }

    private void SnapScaleBackToOne(Transform t)
    {
        if (t == null)
            return;

        // Snap immediately back to normal
        t.localScale = Vector3.one;
    }

    IEnumerator SetBGColor(BGTile tile, Color color)
    {
        yield return new WaitForSeconds(0.1f);
        tile.SetColor(RandomizeColor(color));
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(SpawnBG))]
public class MyButtonExampleEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SpawnBG myScript = (SpawnBG)target;
        if (GUILayout.Button("Change Color"))
        {
            myScript.SetColorAnimation();
        }
    }
}
#endif
