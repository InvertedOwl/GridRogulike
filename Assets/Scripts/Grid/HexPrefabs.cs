using UnityEngine;
using System;
using System.Collections.Generic;

public class HexPrefabs : MonoBehaviour
{
    [Serializable]
    public class HexObject
    {
        public string id;
        public GameObject hexPrefab;
    }
    public HexObject[] hexDictionary;

    private Dictionary<string, GameObject> hexPrefabDictionary = new Dictionary<string, GameObject>();

    public GameObject GetHexPrefab(string id)
    {
        UpdateDictionary();
        return hexPrefabDictionary.ContainsKey(id) ? hexPrefabDictionary[id] : null;
    }
    
    
    void UpdateDictionary()
    {
        foreach (HexObject hexObject in hexDictionary)
        {
            if (!hexPrefabDictionary.ContainsKey(hexObject.id))
            {
                hexPrefabDictionary.Add(hexObject.id, hexObject.hexPrefab);
            }
        }
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
