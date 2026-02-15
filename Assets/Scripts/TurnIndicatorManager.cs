using System;
using System.Collections.Generic;
using Entities;
using UnityEngine;
using UnityEngine.UI.Extensions.EasingCore;
using Util;

public class TurnIndicatorManager : MonoBehaviour
{
    public GameObject TurnIndicatorEnemyPrefab;
    public GameObject TurnIndicatorPlayerPrefab;
    private EasePosition _easePosition;

    public void Start()
    {
        _easePosition = GetComponent<EasePosition>();
        SetAllInactive();
        
    }

    public void UpdateTurnIndicatorList(List<AbstractEntity> entities)
    {
        List<AbstractEntity> newEntities = new List<AbstractEntity>(entities);
        while (newEntities.Count < 5)
        {
            // Ensure even parity in groups of entities
            newEntities.AddRange(newEntities);
        }

        foreach (AbstractEntity entity in newEntities)
        {
            if (entity.entityType == EntityType.Enemy)
            {
                GameObject.Instantiate(TurnIndicatorEnemyPrefab, transform);
            }
            if (entity.entityType == EntityType.Friendly)
                GameObject.Instantiate(TurnIndicatorPlayerPrefab, transform);
        }
        
        _easePosition.InstantSend(new Vector3((-50 * entities.Count) / 2.0f + (entities.Count%2==0?25:0), 0, 0));
        SetAllInactive();
    }

    public void NextEnemy(List<AbstractEntity> entities)
    {
        SetAllInactive();
        _easePosition.SendToLocation(new Vector3(_easePosition.targetLocation.x - 50, 0, 0), () =>
        {
            transform.GetChild(0).SetAsLastSibling();
            _easePosition.InstantSend(new Vector3((-50 * entities.Count) / 2.0f + (entities.Count%2==0?25:0), 0, 0));
            int middleIndex = transform.childCount / 2;
            Transform middleChild = transform.GetChild(middleIndex);
            
            transform.GetChild(middleIndex).GetComponent<IndicatorItem>().Active();
        });
    }

    public void ThisEnemy(List<AbstractEntity> entities)
    {        
        SetAllInactive();
        int middleIndex = transform.childCount / 2;
        
        transform.GetChild(middleIndex).GetComponent<IndicatorItem>().Active();
    }

    public void SetAllInactive()
    {
        foreach (Transform child in transform)
        {
            child.GetComponent<IndicatorItem>().Inactive();
        }
    }
}
