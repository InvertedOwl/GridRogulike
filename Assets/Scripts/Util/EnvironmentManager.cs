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
            SpawnBG.instance.currentColors.Remove(passiveEntries[2].Color);
            passiveEntries.RemoveAt(2);
            Destroy(entryGameObjects[2]);
            entryGameObjects.RemoveAt(2);
        }
        passiveEntries.Add(entry);
        CreatePassiveObject(entry);
        
        SpawnBG.instance.currentColors.Add(entry.Color);
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
