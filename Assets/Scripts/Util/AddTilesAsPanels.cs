using System.Collections.Generic;
using TMPro;
using Types.Tiles;
using UnityEngine;
using UnityEngine.UI;
using Util;

public class AddTilesAsPanels : MonoBehaviour
{
    public GameObject panelPrefab;
    public InfoData infoData;
    
    void Awake()
    {
        foreach (KeyValuePair<string, TileEntry> tile in TileData.tiles)
        {
            GameObject newPanel = Instantiate(panelPrefab, transform);
            newPanel.transform.GetChild(0).GetComponent<Image>().color = tile.Value.color;
            newPanel.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = tile.Value.name + " Tile";
            newPanel.transform.GetChild(1).GetComponent<TextMeshProUGUI>().color = tile.Value.color;
            newPanel.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = tile.Value.description;
            string colorString = "#" + ColorUtility.ToHtmlStringRGB(tile.Value.color);
            InfoEntry entry = new InfoEntry();
            entry.name = tile.Key + " tile";
            entry.formattedName = "<b><color=" + colorString + ">" + tile.Value.name + " Tile</color></b>";
            entry.infoPanel = newPanel;
            newPanel.SetActive(false);
            infoData.info.Add(entry);
        }
    }

}
