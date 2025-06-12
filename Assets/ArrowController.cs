using UnityEngine;

public class ArrowController : MonoBehaviour
{
    public GameObject arrow1;
    public GameObject arrow2;

    public void SetHeight(float height)
    {
        arrow1.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(arrow1.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.x, height);
        arrow2.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(arrow2.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.x, height);
    }
}
