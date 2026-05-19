using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(RectTransform))]
public class LerpRectHeight : MonoBehaviour
{
    public float speed = 1;
    public float targetHeight;
    public bool initializeTargetToCurrentHeight = true;
    public RectTransform rectTransform;
    public UnityEvent arriveAt;

    private bool _isAtHeight;
    private const float HeightThreshold = 0.01f;

    public bool IsAtHeight => _isAtHeight;

    private void Awake()
    {
        CacheRectTransform();
    }

    private void Start()
    {
        CacheRectTransform();

        if (initializeTargetToCurrentHeight)
            targetHeight = rectTransform.sizeDelta.y;
    }

    private void Update()
    {
        CacheRectTransform();

        Vector2 size = rectTransform.sizeDelta;
        size.y = Mathf.Lerp(size.y, targetHeight, speed * Time.deltaTime);
        rectTransform.sizeDelta = size;

        bool wasAtHeight = _isAtHeight;
        _isAtHeight = Mathf.Abs(rectTransform.sizeDelta.y - targetHeight) < HeightThreshold;

        if (_isAtHeight && !wasAtHeight)
            arriveAt?.Invoke();
    }

    public void SetHeight(float height)
    {
        targetHeight = height;
    }

    public void InstantSetHeight(float height)
    {
        targetHeight = height;

        CacheRectTransform();
        Vector2 size = rectTransform.sizeDelta;
        size.y = height;
        rectTransform.sizeDelta = size;
    }

    private void CacheRectTransform()
    {
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();
    }
}
