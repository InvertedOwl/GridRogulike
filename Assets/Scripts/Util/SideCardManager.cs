using UnityEngine;

[ExecuteAlways]
public class SideCardManager : MonoBehaviour
{

    public RectTransform front;
    public RectTransform back;
    void Update()
    {
        Vector3 toCam = (Camera.main.transform.position - transform.position).normalized;

        if (Vector3.Dot(transform.forward, toCam) < 0)
        {
            front.gameObject.SetActive(true);
            back.gameObject.SetActive(false);
        }
        else
        {
            front.gameObject.SetActive(false);
            back.gameObject.SetActive(true);
        }
    }
}
