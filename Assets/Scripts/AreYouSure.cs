using System;
using UnityEngine;
using Util;

public class AreYouSure : MonoBehaviour
{
    public Action<bool> callback;
    
    public static AreYouSure Instance;

    public void Start()
    {
        Instance = this;
    }


    public void AskConfirm(Action<bool> confirmCallback)
    {
        this.callback = confirmCallback;
        GetComponent<LerpPosition>().targetLocation = new Vector2(0, 0);
    }
    
    public void Confirm()
    {
        callback?.Invoke(true);
        callback = null;
        GetComponent<LerpPosition>().targetLocation = new Vector2(0, 10);
    }

    public void Cancel()
    {
        callback?.Invoke(false);
        callback = null;
        GetComponent<LerpPosition>().targetLocation = new Vector2(0, 10);
    }
}
