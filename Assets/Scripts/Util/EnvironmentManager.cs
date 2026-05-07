using System.Collections.Generic;
using Passives;
using UnityEngine;
using Util;

public class EnvironmentManager : MonoBehaviour
{
    public static EnvironmentManager instance;
    public GameObject environGOPrefab;
    public Transform environGOParent;
    
    private List<PassiveEntry> passiveEntries = new List<PassiveEntry>();
    public List<GameObject> entryGameObjects = new List<GameObject>();
    private static readonly float[] PassiveBackgroundValueMultipliers = { 0.6f, .7f, .8f };

    public List<PassiveEntry> GetPassiveEntries()
    {
        return passiveEntries;
    }
    
    
    private void Awake ()
    {
        instance = this;
    }

    public void AddPassive(PassiveEntry entry)
    {
        if (passiveEntries.Count == 3)
        {
            RemovePassiveBackgroundColors(passiveEntries[0]);
            passiveEntries.RemoveAt(0);
            Destroy(entryGameObjects[0]);
            entryGameObjects.RemoveAt(0);
        }
        passiveEntries.Add(entry);
        CreatePassiveObject(entry);
        
        AddPassiveBackgroundColors(entry);
        SpawnBG.instance.SetColorAnimation();
    }

    public void ClearPassives()
    {
        foreach (var passive in passiveEntries)
        {
            RemovePassiveBackgroundColors(passive);
        }

        // Destroy spawned GameObjects
        foreach (var go in entryGameObjects)
        {
            if (go != null)
            {
                Destroy(go);
            }
        }

        passiveEntries.Clear();
        entryGameObjects.Clear();

        // Update background animation
        SpawnBG.instance.SetColorAnimation();
    }


    public void CreatePassiveObject(PassiveEntry entry)
    {
        GameObject go = Instantiate(environGOPrefab, environGOParent);
        go.GetComponent<EnvironMonobehavior>().SetEnviron(entry.Name, entry.Desc, entry.Color);
        entryGameObjects.Add(go);
        go.GetComponent<EaseRotation>().InstantSend(new Vector3(110, 0, 0));
        go.GetComponent<EaseRotation>().SendToRotation(new Vector3(0, 0, 0));
    }

    private void AddPassiveBackgroundColors(PassiveEntry entry)
    {
        foreach (Color color in GetPassiveBackgroundColors(entry.Color))
        {
            SpawnBG.instance.currentColors.Add(color);
        }
    }

    private void RemovePassiveBackgroundColors(PassiveEntry entry)
    {
        foreach (Color color in GetPassiveBackgroundColors(entry.Color))
        {
            SpawnBG.instance.currentColors.Remove(color);
        }
    }

    private IEnumerable<Color> GetPassiveBackgroundColors(Color baseColor)
    {
        Color.RGBToHSV(baseColor, out float h, out float s, out float v);

        foreach (float multiplier in PassiveBackgroundValueMultipliers)
        {
            Color color = Color.HSVToRGB(h, s, Mathf.Clamp01(v * multiplier));
            color.a = baseColor.a;
            yield return color;
        }
    }
}
