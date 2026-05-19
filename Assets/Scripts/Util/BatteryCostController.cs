using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class BatteryCostController : MonoBehaviour
{

    [Header("Cost Text")]
    public TextMeshProUGUI text;
    
    [Header("Values")]
    public float heightOne;
    public float heightTwo;
    public float heightThree;
    
    [Header("References")]
    public Image batteryOne;
    public Image batteryTwo;
    public Image batteryThree;
    
    private RectTransform batteryOneRectTransform;
    private RectTransform batteryTwoRectTransform;
    private RectTransform batteryThreeRectTransform;
    private bool _isDirty;
    
    void Start()
    {
        TMPro_EventManager.TEXT_CHANGED_EVENT.Add(CostUpdated);
        
        batteryOneRectTransform = batteryOne.GetComponent<RectTransform>();
        batteryTwoRectTransform = batteryTwo.GetComponent<RectTransform>();
        batteryThreeRectTransform = batteryThree.GetComponent<RectTransform>();
        _isDirty = true;
    }

    private void OnDestroy()
    {
        TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(CostUpdated);
    }

    private void LateUpdate()
    {
        if (!_isDirty)
            return;

        _isDirty = false;
        ApplyCostVisuals();
    }

    private void CostUpdated(Object changed)
    {
        if (changed != text)
            return;

        _isDirty = true;
    }

    private void ApplyCostVisuals()
    {
        if (!int.TryParse(text.text, out int value))
            return;

        int batteriesNeeded = (int) Math.Ceiling(value / 3.0f);
        
        // Set only needed batteries active. Battery one is always required
        batteryTwo.gameObject.SetActive(batteriesNeeded > 1);
        batteryThree.gameObject.SetActive(batteriesNeeded > 2);

        // If free, then do a height of one without the center to indicate that its free.
        // Otherwise just continue to the normal calculations
        if (batteriesNeeded == 0)
        {
            batteryOne.fillCenter = false;
            batteryOneRectTransform.sizeDelta = new Vector2(batteryOneRectTransform.sizeDelta.x, heightOne);
            return;
        }
        batteryOne.fillCenter = true;


        RectTransform batteryToSet;
        switch (batteriesNeeded)
        {
            case 1:
                batteryToSet = batteryOneRectTransform;
                break;
            case 2:
                batteryOneRectTransform.sizeDelta = new Vector2(batteryOneRectTransform.sizeDelta.x, heightThree);
                batteryToSet = batteryTwoRectTransform;
                break;
            case 3:
                batteryOneRectTransform.sizeDelta = new Vector2(batteryOneRectTransform.sizeDelta.x, heightThree);
                batteryTwoRectTransform.sizeDelta = new Vector2(batteryTwoRectTransform.sizeDelta.x, heightThree);
                batteryToSet = batteryThreeRectTransform;
                break;
            default:
                batteryToSet = batteryOneRectTransform;
                break;
        }
        
        
        switch (value % 3)
        {
            case 1:
                batteryToSet.sizeDelta = new Vector2(batteryOneRectTransform.sizeDelta.x, heightOne);
                break;
            case 2:
                batteryToSet.sizeDelta = new Vector2(batteryOneRectTransform.sizeDelta.x, heightTwo);
                break;
            case 0:
                batteryToSet.sizeDelta = new Vector2(batteryOneRectTransform.sizeDelta.x, heightThree);
                break;
            default:
                break;
        }
    }
}
