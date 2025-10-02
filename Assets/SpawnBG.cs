using System.Collections.Generic;
using Grid;
using UnityEngine;

public class SpawnBG : MonoBehaviour
{
    public GameObject hexPrefab;

    [SerializeField] private int startX = -10;
    [SerializeField] private int startY = -10;
    [SerializeField] private int widthX = 20;
    [SerializeField] private int widthY = 20;
    
    [SerializeField] private List<Color> grasslandColors = new List<Color>();
    
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
        for (int y = startY; y < startY+widthY; y++)
        {
            for (int x = startX; x < startX+widthX; x++)
            {
                GameObject hex = Instantiate(hexPrefab, transform);
                hex.transform.position = HexGridManager.GetHexCenter(x, y);
                hex.GetComponent<BGTile>().SetColor(RandomizeColor(grasslandColors[Random.Range(0, grasslandColors.Count)]));
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
}
