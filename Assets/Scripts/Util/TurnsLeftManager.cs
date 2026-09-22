using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Util;

public class TurnsLeftManager : MonoBehaviour
{

    [SerializeField] private GameObject NodeBG;
    [SerializeField] private GameObject NodePrefab;
    
    [SerializeField] private Color baseColor;
    [SerializeField] private Color warningColor;
    
    private readonly List<TurnsLeftNode> _turnsLeftVisuals = new List<TurnsLeftNode>();


    public int TurnsLeft
    {
        get => _turnsLeft;
        set { _turnsLeft = value;
            SetTurnsLeft();
        }
    }

    private int _turnsLeft = 5;

    public int TurnsLeftMax
    {
        get => _turnsLeftMax;
        set { _turnsLeftMax = value; UpdateTurnsLeftVisuals(); }
    }

    private int _turnsLeftMax = 5;

    public void ResetTurns(int turnCount)
    {
        _turnsLeftMax = Mathf.Max(0, turnCount);
        _turnsLeft = _turnsLeftMax;
        UpdateTurnsLeftVisuals();
    }


    public void SetTurnsLeft()
    {

    }
    
    public void UpdateTurnsLeftVisuals()
    {

        if (_turnsLeftVisuals.Count != TurnsLeftMax || NodeBG.transform.childCount != TurnsLeftMax)
        {
            _turnsLeftVisuals.Clear();
            for (int i = NodeBG.transform.childCount - 1; i >= 0; i--)
            {
                GameObject oldNode = NodeBG.transform.GetChild(i).gameObject;
                oldNode.SetActive(false);
                // Destroy is deferred, so detach now to keep this frame's child count correct.
                oldNode.transform.SetParent(null, false);
                Destroy(oldNode);
            }

            for (int i = 0; i < TurnsLeftMax; i++)
            {
                GameObject newNode = Instantiate(NodePrefab, NodeBG.transform);
                _turnsLeftVisuals.Add(newNode.GetComponent<TurnsLeftNode>());
            }
        }

        int usedTurns = TurnsLeftMax - Mathf.Clamp(TurnsLeft, 0, TurnsLeftMax);
        for (int i = 0; i < _turnsLeftVisuals.Count; i++)
        {
            _turnsLeftVisuals[i].SetUsed(i < usedTurns);
        }

        if (TurnsLeft <= 1)
        {
            NodeBG.GetComponent<Image>().color = warningColor;
            NodeBG.GetComponent<PulseLightness>().Play();
        }
        else
        {
            NodeBG.GetComponent<Image>().color = baseColor;
            NodeBG.GetComponent<PulseLightness>().Stop();
        }
    }
}
