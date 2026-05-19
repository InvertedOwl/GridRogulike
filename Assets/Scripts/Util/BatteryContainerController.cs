using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Util;

public class BatteryContainerController : MonoBehaviour
{
    [Header("Energy Text")]
    public TextMeshProUGUI text;
    
    [Header("Values")]
    public float heightBase = 16.7f;
    public float heightOne = 24.3f;
    
    [Header("Colors")]
    public Color full;
    public Color half;
    public Color low;

    [Header("References")] 
    public LerpRectHeight batteryElement1;
    public LerpRectHeight batteryElement2;
    public LerpRectHeight batteryElement3;
    public LerpRectHeight extraBattery;


    private bool _isDirty;
    

    void Start()
    {
        TMPro_EventManager.TEXT_CHANGED_EVENT.Add(CostUpdated);
        
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
        Debug.Log("reloading energy visuals");
        
        string first = text.text.Split("/")[0];
        string second = text.text.Split("/")[1];
        
        if (!int.TryParse(first, out int energy))
            return;
        if (!int.TryParse(second, out int max))
            return;
        
        int batteryEnergy = Mathf.Max(energy, max);

        if (batteryEnergy <= 3)
            extraBattery.GetComponent<LerpRectHeight>().SetHeight(0);
        else
            extraBattery.SetHeight(((batteryEnergy-3) * heightOne) + heightBase);

        if (energy == 0)
        {
            batteryElement1.SetHeight(0);
            batteryElement2.SetHeight(0);
            batteryElement3.SetHeight(0);
        }
        else if (energy == 1)
        {
            batteryElement1.SetHeight(61);
            batteryElement2.SetHeight(0);
            batteryElement3.SetHeight(0);
            batteryElement1.GetComponent<EaseColor>().SendToColor(low);
        }
        else if (energy == 2)
        {
            batteryElement1.SetHeight(61);
            batteryElement2.SetHeight(61);
            batteryElement3.SetHeight(0);
            batteryElement1.GetComponent<EaseColor>().SendToColor(half);
            batteryElement2.GetComponent<EaseColor>().SendToColor(half);
        }
        else if (energy >= 3)
        {
            batteryElement1.SetHeight(61);
            batteryElement2.SetHeight(61);
            batteryElement3.SetHeight(61);   
            batteryElement1.GetComponent<EaseColor>().SendToColor(full);
            batteryElement2.GetComponent<EaseColor>().SendToColor(full);
            batteryElement3.GetComponent<EaseColor>().SendToColor(full);
        }
    }
}
