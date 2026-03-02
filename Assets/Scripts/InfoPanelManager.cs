using System.Collections.Generic;
using ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InfoPanelManager : MonoBehaviour
{
    public GameObject infoPanelPrefab;
    public InfoPanelsData InfoPanelsData;

    private HashSet<string> addedPanels = new HashSet<string>();

    public void RemovePanels()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        addedPanels.Clear();
    }

    public void AddPanels(string input)
    {
        foreach (InfoPanelsData.InfoPanel infoPanel in InfoPanelsData.GetAllInString(input))
        {
            if (!addedPanels.Add(infoPanel.title))
                continue;

            GameObject infoPanelObject = Instantiate(infoPanelPrefab, transform);
            infoPanelObject.transform.GetChild(0).GetComponent<Image>().sprite = infoPanel.sprite;
            infoPanelObject.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = infoPanel.title;
            infoPanelObject.transform.GetChild(1).GetComponent<TextMeshProUGUI>().color = infoPanel.color;
            infoPanelObject.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = infoPanel.description;
        }
    }
}
