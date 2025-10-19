using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using Util;

public class ButtonHoverScale : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // Thank you AI :)
    
    [Header("Scale Settings")]
    [Tooltip("Enable scale animation on hover")]
    [SerializeField] private bool enableScale = true;
    
    [Tooltip("The percent scale of the button when hovered (1.0 = 100%, 1.2 = 120%)")]
    [SerializeField] private float scalePercent = 1.2f;
    
    [Tooltip("Time it takes to scale")]
    [SerializeField] private float scaleTime = 0.2f;
    
    [Tooltip("Time before the scale starts")]
    [SerializeField] private float scaleOffsetBefore = 0f;
    
    [Tooltip("Time it takes before scale starts going away")]
    [SerializeField] private float scaleOffsetAfter = 0f;
    
    [Header("Position Settings")]
    [Tooltip("Enable position animation on hover")]
    [SerializeField] private bool enablePosition = false;
    
    [Tooltip("Position offset when hovered (in local space if EasePosition.isLocal is true)")]
    [SerializeField] private Vector3 positionOffset = new Vector3(0, 10, 0);
    
    [Tooltip("Time it takes to move")]
    [SerializeField] private float positionTime = 0.2f;
    
    [Tooltip("Time before the position movement starts")]
    [SerializeField] private float positionOffsetBefore = 0f;
    
    [Tooltip("Time it takes before position starts returning")]
    [SerializeField] private float positionOffsetAfter = 0f;
    
    [Header("Target Components")]
    [Tooltip("The EaseScale component to animate")]
    [SerializeField] private EaseScale scaleGraphic;
    
    [Tooltip("The EasePosition component to animate")]
    [SerializeField] private EasePosition positionGraphic;
    
    // Coroutine references for cancellation
    private Coroutine _scaleUpCoroutine;
    private Coroutine _scaleDownCoroutine;
    private Coroutine _moveUpCoroutine;
    private Coroutine _moveDownCoroutine;
    
    // Store the original values to return to
    private Vector3 _originalScale;
    private Vector3 _originalPosition;
    
    // Flag to track if we're currently hovered
    private bool _isHovered = false;

    /// <summary>
    /// Initialize the script and validate components
    /// </summary>
    private void Start()
    {
        // Initialize scale component if enabled
        if (enableScale)
        {
            if (scaleGraphic == null)
            {
                scaleGraphic = GetComponent<EaseScale>();
                if (scaleGraphic == null)
                {
                    Debug.LogError($"ButtonHoverScale on {gameObject.name} has enableScale=true but no EaseScale component!");
                    enableScale = false;
                }
            }
            
            if (scaleGraphic != null)
            {
                _originalScale = scaleGraphic.transform.localScale;
                scaleGraphic.durationSeconds = scaleTime;
            }
        }
        
        // Initialize position component if enabled
        if (enablePosition)
        {
            if (positionGraphic == null)
            {
                positionGraphic = GetComponent<EasePosition>();
                if (positionGraphic == null)
                {
                    Debug.LogError($"ButtonHoverScale on {gameObject.name} has enablePosition=true but no EasePosition component!");
                    enablePosition = false;
                }
            }
            
            if (positionGraphic != null)
            {
                _originalPosition = positionGraphic.isLocal 
                    ? positionGraphic.transform.localPosition 
                    : positionGraphic.transform.position;
                positionGraphic.durationSeconds = positionTime;
            }
        }
        
        // Disable the component if neither animation is enabled or available
        if (!enableScale && !enablePosition)
        {
            Debug.LogWarning($"ButtonHoverScale on {gameObject.name} has no animations enabled!");
            enabled = false;
        }
    }

    /// <summary>
    /// Called when the mouse pointer enters the UI element
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        _isHovered = true;
        
        // Cancel any ongoing return animations
        if (_scaleDownCoroutine != null)
        {
            StopCoroutine(_scaleDownCoroutine);
            _scaleDownCoroutine = null;
        }
        
        if (_moveDownCoroutine != null)
        {
            StopCoroutine(_moveDownCoroutine);
            _moveDownCoroutine = null;
        }
        
        // Start the hover animations
        if (enableScale && scaleGraphic != null)
        {
            _scaleUpCoroutine = StartCoroutine(ScaleUp());
        }
        
        if (enablePosition && positionGraphic != null)
        {
            _moveUpCoroutine = StartCoroutine(MoveToOffset());
        }
    }

    /// <summary>
    /// Called when the mouse pointer exits the UI element
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        _isHovered = false;
        
        // Cancel any ongoing hover animations
        if (_scaleUpCoroutine != null)
        {
            StopCoroutine(_scaleUpCoroutine);
            _scaleUpCoroutine = null;
        }
        
        if (_moveUpCoroutine != null)
        {
            StopCoroutine(_moveUpCoroutine);
            _moveUpCoroutine = null;
        }
        
        // Start the return animations
        if (enableScale && scaleGraphic != null)
        {
            _scaleDownCoroutine = StartCoroutine(ScaleDown());
        }
        
        if (enablePosition && positionGraphic != null)
        {
            _moveDownCoroutine = StartCoroutine(MoveToOriginal());
        }
    }

    /// <summary>
    /// Coroutine to handle scaling up with time offset
    /// </summary>
    private IEnumerator ScaleUp()
    {
        // Wait for the time offset before starting the scale animation
        if (scaleOffsetBefore > 0f)
        {
            yield return new WaitForSeconds(scaleOffsetBefore);
        }
        
        // Only scale up if we're still hovered
        if (_isHovered)
        {
            Vector3 targetScale = _originalScale * scalePercent;
            scaleGraphic.durationSeconds = scaleTime;
            scaleGraphic.SetScale(targetScale);
        }
        
        _scaleUpCoroutine = null;
    }

    /// <summary>
    /// Coroutine to handle scaling down with time offset
    /// </summary>
    private IEnumerator ScaleDown()
    {
        // Wait for the time offset before starting the scale down animation
        if (scaleOffsetAfter > 0f)
        {
            yield return new WaitForSeconds(scaleOffsetAfter);
        }
        
        // Only scale down if we're not hovered
        if (!_isHovered)
        {
            scaleGraphic.durationSeconds = scaleTime;
            scaleGraphic.SetScale(_originalScale);
        }
        
        _scaleDownCoroutine = null;
    }

    /// <summary>
    /// Coroutine to handle moving to offset position with time offset
    /// </summary>
    private IEnumerator MoveToOffset()
    {
        // Wait for the time offset before starting the position animation
        if (positionOffsetBefore > 0f)
        {
            yield return new WaitForSeconds(positionOffsetBefore);
        }
        
        // Only move if we're still hovered
        if (_isHovered)
        {
            Vector3 targetPosition = _originalPosition + positionOffset;
            positionGraphic.durationSeconds = positionTime;
            positionGraphic.SendToLocation(targetPosition);
        }
        
        _moveUpCoroutine = null;
    }

    /// <summary>
    /// Coroutine to handle returning to original position with time offset
    /// </summary>
    private IEnumerator MoveToOriginal()
    {
        // Wait for the time offset before starting the return animation
        if (positionOffsetAfter > 0f)
        {
            yield return new WaitForSeconds(positionOffsetAfter);
        }
        
        // Only move back if we're not hovered
        if (!_isHovered)
        {
            positionGraphic.durationSeconds = positionTime;
            positionGraphic.SendToLocation(_originalPosition);
        }
        
        _moveDownCoroutine = null;
    }

    /// <summary>
    /// Clean up coroutines and reset when the object is disabled
    /// </summary>
    private void OnDisable()
    {
        // Stop all running coroutines
        if (_scaleUpCoroutine != null)
        {
            StopCoroutine(_scaleUpCoroutine);
            _scaleUpCoroutine = null;
        }
        
        if (_scaleDownCoroutine != null)
        {
            StopCoroutine(_scaleDownCoroutine);
            _scaleDownCoroutine = null;
        }
        
        if (_moveUpCoroutine != null)
        {
            StopCoroutine(_moveUpCoroutine);
            _moveUpCoroutine = null;
        }
        
        if (_moveDownCoroutine != null)
        {
            StopCoroutine(_moveDownCoroutine);
            _moveDownCoroutine = null;
        }
        
        // Reset to original values immediately
        if (enableScale && scaleGraphic != null)
        {
            scaleGraphic.transform.localScale = _originalScale;
        }
        
        if (enablePosition && positionGraphic != null)
        {
            if (positionGraphic.isLocal)
                positionGraphic.transform.localPosition = _originalPosition;
            else
                positionGraphic.transform.position = _originalPosition;
        }
    }

    /// <summary>
    /// Allow runtime validation of the parameter values
    /// </summary>
    private void OnValidate()
    {
        // Ensure scale percent is reasonable
        scalePercent = Mathf.Max(0.1f, scalePercent);
        
        // Ensure time values are non-negative
        scaleTime = Mathf.Max(0f, scaleTime);
        scaleOffsetBefore = Mathf.Max(0f, scaleOffsetBefore);
        scaleOffsetAfter = Mathf.Max(0f, scaleOffsetAfter);
        
        positionTime = Mathf.Max(0f, positionTime);
        positionOffsetBefore = Mathf.Max(0f, positionOffsetBefore);
        positionOffsetAfter = Mathf.Max(0f, positionOffsetAfter);
    }
}