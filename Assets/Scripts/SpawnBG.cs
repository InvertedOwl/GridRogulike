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

    // How long EaseScale takes to animate to the target scale (match your EaseScale duration)
    [SerializeField] private float flipAnimDuration = 0.2f;

    public void Awake()
    {
        if (instance == null) { instance = this; }
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
                hex.transform.position = HexGridManager.GetHexCenter(x, y);

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
        StartCoroutine(ColorAnimationDiagonal());
    }

    IEnumerator ColorAnimationDiagonal()
    {
        List<Color> colors = new List<Color>();

        if (currentColors.Count != 0)
        {
            colors.AddRange(currentColors);
        }
        else
        {
            colors.AddRange(grasslandColors);
        }
        
        int count = transform.childCount;
        int width = widthX;
        int height = Mathf.CeilToInt(count / (float)width);

        float pause = 0.04f;

        int maxDiag = (width - 1) + (height - 1);

        for (int d = 0; d <= maxDiag; d++)
        {
            for (int y = 0; y < height; y++)
            {
                int x = d - y;
                if (x < 0 || x >= width) continue;

                int i = y * width + x;
                if (i >= count) continue;

                Transform child = transform.GetChild(i);
                BGTile tile = child.GetComponentInChildren<BGTile>();

                // Animate to -1 (flip)
                EaseScale ease = tile.GetComponent<EaseScale>();
                if (ease != null)
                {
                    ease.SetScale(new Vector3(-1, 1, 1));

                    // Then SNAP back to +1 after the flip completes (no reverse animation)
                    StartCoroutine(SnapScaleBackToOne(tile.transform, flipAnimDuration));
                }

                StartCoroutine(SetBGColor(tile, colors[Random.Range(0, colors.Count)]));
            }

            yield return new WaitForSeconds(pause);
        }
    }

    IEnumerator SnapScaleBackToOne(Transform t, float wait)
    {
        yield return new WaitForSeconds(wait);

        // Snap immediately back to normal
        t.localScale = Vector3.one;
    }

    IEnumerator SetBGColor(BGTile tile, Color color)
    {
        yield return new WaitForSeconds(0.1f);
        tile.SetColor(RandomizeColor(color));
    }
}

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
