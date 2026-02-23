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

    public List<PassiveEntry> GetPassiveEntries()
    {
        return passiveEntries;
    }
    
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public void AddPassive(PassiveEntry entry)
    {
        if (passiveEntries.Count == 3)
        {
            SpawnBG.instance.currentColors.Remove(passiveEntries[0].Color);
            passiveEntries.RemoveAt(0);
            Destroy(entryGameObjects[0]);
            entryGameObjects.RemoveAt(0);
        }
        passiveEntries.Add(entry);
        CreatePassiveObject(entry);
        
        SpawnBG.instance.currentColors.Add(entry.Color);
        SpawnBG.instance.SetColorAnimation();
    }

    public void ClearPassives()
    {
        foreach (var passive in passiveEntries)
        {
            SpawnBG.instance.currentColors.Remove(passive.Color);
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
}
