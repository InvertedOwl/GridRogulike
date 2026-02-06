using System.Collections.Generic;
using UnityEngine;

public class GOList : MonoBehaviour
{
    [System.Serializable]
    public class StringGOPair
    {
        public string Key;
        public GameObject Value;
    }

    
    [SerializeField]
    private List<StringGOPair> myDictionaryList = new List<StringGOPair>();

    private Dictionary<string, GameObject> myDictionary = new Dictionary<string, GameObject>();

    // Populate the dictionary from the list
    private void Awake()
    {
        RebuildDictionary();
    }

    // Rebuild dictionary from serialized list
    private void RebuildDictionary()
    {
        myDictionary.Clear();
        foreach (var pair in myDictionaryList)
        {
            if (!myDictionary.ContainsKey(pair.Key))
                myDictionary[pair.Key] = pair.Value;
        }
    }

    // Public Getter
    public GameObject GetValue(string key)
    {
        if (myDictionary.ContainsKey(key))
            return myDictionary[key];

        Debug.LogError($"Gameobject '{key}' Not Found");
        return default;
    }

    // Public Setter
    public void SetValue(string key, GameObject value)
    {
        // Update dictionary
        myDictionary[key] = value;

        // Update or add in the list so the Inspector reflects it
        var pair = myDictionaryList.Find(p => p.Key == key);
        if (pair != null)
        {
            pair.Value = value;
        }
        else
        {
            myDictionaryList.Add(new StringGOPair() { Key = key, Value = value });
        }
    }

    public bool HasValue(string key)
    {
        return myDictionary.ContainsKey(key);
    }

}
