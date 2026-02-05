using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using Util;

public class SpriteArrowManager : MonoBehaviour
{
    public static SpriteArrowManager Instance { get; private set; }
    
    public Dictionary<string, GameObject> arrows = new Dictionary<string, GameObject>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Remove if you don't want persistence
    }
    
    public GameObject arrowPrefab;


    public string CreateArrow(Vector2Int tail, Vector2Int head, Color newColor, string icon, int amount = 0)
    {
        GameObject arrow = Instantiate(arrowPrefab);
        string uuid = Guid.NewGuid().ToString();
        arrows.Add(uuid, arrow);

        arrow.transform.GetChild(0).GetComponent<SpriteArrowController>().head = head;
        arrow.transform.GetChild(0).GetComponent<SpriteArrowController>().Tail = tail;
        arrow.transform.GetChild(0).GetComponent<SpriteRenderer>().color = newColor;

        foreach (Transform iconT in arrow.transform.GetChild(1))
        {
            if (iconT.gameObject.name == icon)
            {
                iconT.gameObject.SetActive(true);
                iconT.GetComponentInChildren<TextMeshPro>().text = amount + "";
            }
            else
            {
                iconT.gameObject.SetActive(false);
            }
        }
        
        return uuid;
    }

    public void DestroyArrow(string uuid)
    {

        try
        {
            if (!arrows.ContainsKey(uuid))
            {
                return;
            }

            Destroy(arrows[uuid]);
            arrows.Remove(uuid);
        }
        catch (Exception e)
        {
            
        }
            
    }

}
