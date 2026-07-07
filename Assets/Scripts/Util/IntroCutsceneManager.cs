using System.Collections;
using Spine.Unity;
using StateManager;
using UnityEngine;
using Util;

public class IntroCutsceneManager : MonoBehaviour
{
    [Header("Cutscene Objects")]
    [SerializeField] private EaseColor easeColor;
    [SerializeField] private EasePosition contentEasePosition;
    [SerializeField] private SkeletonAnimation skeletonAnimation;

    
    void Awake()
    {
        if (GameStateManager.GameLoaded == true)
        {
            transform.position = new Vector3(0, -50, 0);
        }
        else
        {
            StartCoroutine(PlayCutscene());
        }
    }

    IEnumerator PlayCutscene()
    {
        yield return new WaitForSeconds(0.5f);
        skeletonAnimation.AnimationState.SetAnimation(0, "invert", false);
        yield return new WaitForSeconds(1.5f);
        easeColor.SendToColor(new Color(0, 0, 0, 0));
        contentEasePosition.SendToLocation(new Vector3(0, -50, 0));
        yield return new WaitForSeconds(0.5f);
        transform.position = new Vector3(0, -50, 0);
    }
    
    private void OnMouseDown()
    {
        transform.position = new Vector3(0, -50, 0);
    }
}
