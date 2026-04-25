using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using Util;

public class SpriteArrowManager : MonoBehaviour
{
    public static SpriteArrowManager Instance { get; private set; }
    
    public Dictionary<string, GameObject> arrows = new Dictionary<string, GameObject>();

    void Awake ()
    {
        Instance = this;
    }
    
    public GameObject arrowPrefab;


    public string CreateArrow(Vector2Int tail, Vector2Int head, Color newColor, string icon, int amount = 0)
    {
        GameObject arrow = Instantiate(arrowPrefab);
        string uuid = Guid.NewGuid().ToString();
        arrows.Add(uuid, arrow);

        SpriteArrowController controller = GetArrowController(arrow);
        controller.head = head;
        controller.Tail = tail;
        controller.SetColor(newColor);

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

    private SpriteArrowController GetArrowController(GameObject arrow)
    {
        SpriteArrowController controller = arrow.GetComponentInChildren<SpriteArrowController>();
        if (controller != null)
            return controller;

        LineRenderer lineRenderer = arrow.GetComponentInChildren<LineRenderer>();
        GameObject target = lineRenderer != null ? lineRenderer.gameObject : arrow.transform.GetChild(0).gameObject;
        controller = target.AddComponent<SpriteArrowController>();
        controller.lineRenderer = lineRenderer;
        return controller;
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
