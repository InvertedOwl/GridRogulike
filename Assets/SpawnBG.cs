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
            }
        }
    }
    
    [ContextMenu("Refresh Grid")]
    public void RefreshGrid()
    {
        DestroyBackground();
        SpawnBackground();
    }
}
