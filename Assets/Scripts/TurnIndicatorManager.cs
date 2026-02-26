using System.Collections.Generic;
using System.Linq;
using Entities;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions.EasingCore;
using Util;

public class TurnIndicatorManager : MonoBehaviour
{
    public GameObject TurnIndicatorEnemyPrefab;
    public GameObject TurnIndicatorPlayerPrefab;

    public GameObject enemiesLeft;
    public GameObject center;
    public GameObject enemiesRight;
    
    private EasePosition _easePosition;

    private int lastTurn = -1;
    
    private void Awake()
    {
        _easePosition = GetComponent<EasePosition>();
    }

    public void Rebuild(List<AbstractEntity> entities, List<int> turnOrder)
    {
        for (int i = enemiesLeft.transform.childCount - 1; i >= 0; i--)
            Destroy(enemiesLeft.transform.GetChild(i).gameObject);

        for (int i = center.transform.childCount - 1; i >= 0; i--)
            Destroy(center.transform.GetChild(i).gameObject);

        for (int i = enemiesRight.transform.childCount - 1; i >= 0; i--)
            Destroy(enemiesRight.transform.GetChild(i).gameObject);
        
        
        foreach (int index in turnOrder)
        {
            AbstractEntity entity = entities[index];

            if (entity.entityType == EntityType.Enemy)
            {
                Instantiate(TurnIndicatorEnemyPrefab, enemiesLeft.transform);
                Instantiate(TurnIndicatorEnemyPrefab, enemiesRight.transform);
                Instantiate(TurnIndicatorEnemyPrefab, center.transform);
            }
            else
            {
                Instantiate(TurnIndicatorPlayerPrefab, enemiesLeft.transform);
                Instantiate(TurnIndicatorPlayerPrefab, enemiesRight.transform);
                Instantiate(TurnIndicatorPlayerPrefab, center.transform);
            }
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(enemiesLeft.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(center.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(enemiesRight.GetComponent<RectTransform>());
    }

    public void SetCurrentTurn(List<int> turnOrder, List<AbstractEntity> liveEntities, int current)
    {
        if (current == -1)
        {
            lastTurn = -1;
            Debug.Log("Was -1!");
            return;
        }
        
        float indexFromMiddle = ((turnOrder.Count-1) / 2.0f) - current;


        // If we are going back to the start (and this isn't the first turn) send to the next like normal but then warp to start after done.
        // Thus, it can be infinite
        if (current == 0 && lastTurn != -1)
        {
            float indexFromMiddleFudged = ((turnOrder.Count-1) / 2.0f) - turnOrder.Count;
            _easePosition.SendToLocation(new Vector3(indexFromMiddleFudged * 50, 0), () =>
            {
                _easePosition.InstantSend(new Vector3(indexFromMiddle * 50, 0));
            });
        }
        else
        {
            _easePosition.SendToLocation(new Vector3(indexFromMiddle * 50, 0));
        }

        
        
        
        Debug.Log(current);
        if (center.transform.childCount == 0)
        {
            Debug.Log("no children");
            return;
        }
        else
        {
            if (lastTurn != -1)
                center.transform.GetChild(lastTurn).GetComponent<IndicatorItem>().Inactive();
            center.transform.GetChild(current).GetComponent<IndicatorItem>().Active();
        }
        
        
        lastTurn = current;
    }
}