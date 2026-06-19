using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using Util;

public class SpriteArrowManager : MonoBehaviour
{
    public static SpriteArrowManager Instance { get; private set; }
    
    public Dictionary<string, GameObject> arrows = new Dictionary<string, GameObject>();
    private readonly HashSet<string> enemyPreviewArrows = new HashSet<string>();
    private readonly Dictionary<string, bool> enemyPreviewArrowLocalVisibility = new Dictionary<string, bool>();
    private bool enemyPreviewArrowsVisible = true;

    void Awake ()
    {
        Instance = this;
    }
    
    public GameObject arrowPrefab;


    public string CreateArrow(Vector2Int tail, Vector2Int head, Color newColor, string icon, int amount = 0, bool enemyPreview = false)
    {
        GameObject arrow = Instantiate(arrowPrefab);
        string uuid = Guid.NewGuid().ToString();
        arrows.Add(uuid, arrow);

        if (enemyPreview)
        {
            enemyPreviewArrows.Add(uuid);
            enemyPreviewArrowLocalVisibility[uuid] = false;
        }

        SpriteArrowController controller = GetArrowController(arrow);
        controller.head = head;
        controller.Tail = tail;
        controller.SetColor(newColor);

        SetArrowIcon(controller, icon, amount);

        if (enemyPreview)
            arrow.SetActive(enemyPreviewArrowsVisible && enemyPreviewArrowLocalVisibility[uuid]);
        
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

    private void SetArrowIcon(SpriteArrowController controller, string icon, int amount)
    {
        Transform iconContainer = controller.IconContainer;
        if (iconContainer == null)
            return;

        foreach (Transform iconTransform in iconContainer)
        {
            bool isTargetIcon = iconTransform.gameObject.name == icon;
            iconTransform.gameObject.SetActive(isTargetIcon);

            if (isTargetIcon)
                SetIconText(iconTransform, amount.ToString());
        }
    }

    private void SetIconText(Transform iconTransform, string text)
    {
        TMP_Text textComponent = iconTransform.GetComponentInChildren<TMP_Text>(true);
        if (textComponent != null)
            textComponent.text = text;
    }

    public void DestroyArrow(string uuid)
    {

        try
        {
            if (!arrows.ContainsKey(uuid))
            {
                return;
            }

            enemyPreviewArrows.Remove(uuid);
            enemyPreviewArrowLocalVisibility.Remove(uuid);
            Destroy(arrows[uuid]);
            arrows.Remove(uuid);
        }
        catch (Exception e)
        {
            
        }
            
    }

    public void SetEnemyPreviewArrowsVisible(bool visible)
    {
        enemyPreviewArrowsVisible = visible;

        foreach (string uuid in enemyPreviewArrows)
        {
            if (arrows.TryGetValue(uuid, out GameObject arrow) && arrow != null)
            {
                bool localVisible = enemyPreviewArrowLocalVisibility.TryGetValue(uuid, out bool value) && value;
                arrow.SetActive(visible && localVisible);
            }
        }
    }

    public void SetEnemyPreviewArrowVisible(string uuid, bool visible)
    {
        if (string.IsNullOrEmpty(uuid) || !enemyPreviewArrows.Contains(uuid))
            return;

        enemyPreviewArrowLocalVisibility[uuid] = visible;

        if (arrows.TryGetValue(uuid, out GameObject arrow) && arrow != null)
            arrow.SetActive(enemyPreviewArrowsVisible && visible);
    }

}
