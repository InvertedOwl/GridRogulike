using UnityEngine;
using Util;

public class IndicatorItem : MonoBehaviour
{
    public EaseColor glow1;
    public EaseColor glow2;
    public EaseColor glow3;
    public EaseColor darkenOverlay;
    public EaseScale easeScale;
    
    public void Active()
    {
        Color color = glow1.targetColor;
        color.a = 1;
        glow1.SendToColor(color);
        glow2.SendToColor(color);
        glow3.SendToColor(color);
        
        Color color2 = darkenOverlay.targetColor;
        color2.a = 0;
        darkenOverlay.SendToColor(color2);
        easeScale.SetScale(new Vector3(.85f, .85f, .85f));
        Debug.Log("Activated");
    }

    public void Inactive()
    {
        Color color = glow1.targetColor;
        color.a = 0;
        glow1.SendToColor(color);
        glow2.SendToColor(color);
        glow3.SendToColor(color);
        
        Color color2 = darkenOverlay.targetColor;
        color2.a = 0.6f;
        darkenOverlay.SendToColor(color2);
        easeScale.SetScale(new Vector3(0.73483f, 0.73483f, 0.73483f));
        
    }
}
