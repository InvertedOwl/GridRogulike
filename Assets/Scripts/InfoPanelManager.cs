using System.Collections.Generic;
using System.Text.RegularExpressions;
using Cards.CardList;
using ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Util;

public class InfoPanelManager : MonoBehaviour
{
    public GameObject infoPanelPrefab;
    public InfoPanelsData InfoPanelsData;
    public List<TextMeshProUGUI> textSources = new List<TextMeshProUGUI>();

    private HashSet<string> addedPanels = new HashSet<string>();
    private HashSet<string> manualInputs = new HashSet<string>();
    private string _lastTextSignature;
    private bool _dirty = true;

    private void Update()
    {
        string textSignature = GetTextSignature();
        if (!_dirty && textSignature == _lastTextSignature)
            return;

        RebuildPanels(textSignature);
    }

    public void RemovePanels()
    {
        textSources.Clear();
        manualInputs.Clear();
        ClearPanelObjects();
        _lastTextSignature = string.Empty;
        _dirty = false;
    }

    public void AddTextSource(TextMeshProUGUI textSource)
    {
        if (textSource == null || textSources.Contains(textSource))
            return;

        textSources.Add(textSource);
        RebuildPanels(GetTextSignature());
    }

    public void AddTextSources(IEnumerable<TextMeshProUGUI> newTextSources)
    {
        if (newTextSources == null)
            return;

        foreach (TextMeshProUGUI textSource in newTextSources)
        {
            AddTextSource(textSource);
        }
    }

    public void SetTextSources(IEnumerable<TextMeshProUGUI> newTextSources)
    {
        textSources.Clear();
        AddTextSources(newTextSources);
        RebuildPanels(GetTextSignature());
    }

    public string FormatTextForInfo(string info)
    {
        if (string.IsNullOrEmpty(info))
            return string.Empty;

        string newInfo = info;

        foreach (string key in BattleStats.names.Keys)
        {
            if (newInfo.ToLower().Contains(key))
            {
                newInfo = Regex.Replace(newInfo, Regex.Escape(key), BattleStats.names[key](), RegexOptions.IgnoreCase);
            }
        }

        foreach (string iconsKey in CardActionTextMapper.icons.Keys)
        {
            if (newInfo.ToLower().Contains(iconsKey))
            {
                newInfo = Regex.Replace(newInfo, iconsKey, CardActionTextMapper.icons[iconsKey], RegexOptions.IgnoreCase);
            }
        }

        return newInfo;
    }

    private void ClearPanelObjects()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        addedPanels.Clear();
    }

    public void AddPanels(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return;

        manualInputs.Add(input);
        AddPanelsFromString(input);
    }

    private void RebuildPanels(string textSignature)
    {
        ClearPanelObjects();

        foreach (string manualInput in manualInputs)
        {
            AddPanelsFromString(manualInput);
        }

        for (int i = 0; i < textSources.Count; i++)
        {
            TextMeshProUGUI textSource = textSources[i];
            if (textSource == null)
            {
                textSources.RemoveAt(i);
                i--;
                continue;
            }

            AddPanelsFromString(textSource.text);
        }

        _lastTextSignature = textSignature;
        _dirty = false;
    }

    private void AddPanelsFromString(string input)
    {
        if (string.IsNullOrWhiteSpace(input) || InfoPanelsData == null)
            return;

        List<InfoPanelsData.InfoPanelMatch> matches = InfoPanelsData.GetAllMatchesInString(input);
        int iconOrder = 10000;

        foreach (var icon in CardActionTextMapper.icons)
        {
            int iconIndex = input.IndexOf(icon.Value, System.StringComparison.OrdinalIgnoreCase);
            if (iconIndex < 0)
                continue;

            string key = icon.Key.Trim('<', '>');
            InfoPanelsData.InfoPanel? infoPanel = InfoPanelsData.Get(key);
            if (infoPanel.HasValue)
            {
                matches.Add(new InfoPanelsData.InfoPanelMatch(infoPanel.Value, iconIndex, iconOrder));
                iconOrder++;
            }
        }

        matches.Sort((a, b) =>
        {
            int indexCompare = a.index.CompareTo(b.index);
            return indexCompare != 0 ? indexCompare : a.order.CompareTo(b.order);
        });

        foreach (InfoPanelsData.InfoPanelMatch match in matches)
        {
            AddPanel(match.infoPanel);
        }
    }

    private void AddPanel(InfoPanelsData.InfoPanel infoPanel)
    {
        if (!addedPanels.Add(infoPanel.title))
            return;

        GameObject infoPanelObject = Instantiate(infoPanelPrefab, transform);
        infoPanelObject.transform.GetChild(0).GetComponent<Image>().sprite = infoPanel.sprite;
        infoPanelObject.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = infoPanel.title;
        infoPanelObject.transform.GetChild(1).GetComponent<TextMeshProUGUI>().color = infoPanel.color;
        infoPanelObject.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = infoPanel.description;
    }

    private string GetTextSignature()
    {
        if (textSources.Count == 0)
            return string.Empty;

        System.Text.StringBuilder builder = new System.Text.StringBuilder();
        for (int i = 0; i < textSources.Count; i++)
        {
            TextMeshProUGUI textSource = textSources[i];
            if (textSource == null)
            {
                textSources.RemoveAt(i);
                i--;
                continue;
            }

            builder.Append(textSource.text);
            builder.Append('\n');
        }

        return builder.ToString();
    }
}
