using UnityEngine;

public class EntityGlow : MonoBehaviour
{
    public EaseScale glow;

    public void Glow()
    {
        glow.SetScale(new Vector3(0, 0, 0));
    }
    public void Unglow()
    {
        glow.SetScale(new Vector3(0, 0, 0));
    }
}
