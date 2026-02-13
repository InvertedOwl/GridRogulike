using System;
using System.Collections.Generic;
using Cards.CardEvents;
using Entities;
using UnityEngine;

public class HexPreviewHandler : MonoBehaviour
{
    public Dictionary<AbstractEntity, List<AbstractCardEvent>> eventsOnThisHex = new Dictionary<AbstractEntity, List<AbstractCardEvent>>();
    public GOList GoList;

    private bool _disablePreview = false;
    public bool DisablePreview
    {
        get => _disablePreview;
        set
        {
            _disablePreview = value;
            // Clear previews
            UpdatePreview(new Dictionary<AbstractEntity, List<AbstractCardEvent>>());
        }
    }

    public void Update()
    {
        if (DisablePreview)
            return;
        
        UpdatePreview(eventsOnThisHex);
    }

    public void UpdatePreview(Dictionary<AbstractEntity, List<AbstractCardEvent>> localEvents)
    {
        bool hasAttack = false;

        foreach (AbstractEntity entity in localEvents.Keys)
        {
            foreach (AbstractCardEvent abstractCardEvent in localEvents[entity])
            {
                if (abstractCardEvent is AttackCardEvent)
                {
                    hasAttack = true;
                }
            }
        }


        try
        {
            if (hasAttack)
                GoList.GetValue("ToAttack").SetActive(true);
            else
                GoList.GetValue("ToAttack").SetActive(false);
        } catch (Exception _) {}
    }
}
