using System;
using UnityEngine;
using Util;

public class AreYouSure : MonoBehaviour
{
    public Action<bool> callback;
    public bool IsOpen { get; private set; }
    
    public static AreYouSure Instance;

    public void Start()
    {
        Instance = this;
    }
    
    public void AskConfirm(Action<bool> confirmCallback)
    {
        IsOpen = true;
        this.callback = confirmCallback;
        GetComponent<LerpPosition>().targetLocation = new Vector2(0, 0);
    }
    
    public void Confirm()
    {
        callback?.Invoke(true);
        callback = null;
        IsOpen = false;
        GetComponent<LerpPosition>().targetLocation = new Vector2(0, 700);
    }

    public void Cancel()
    {
        callback?.Invoke(false);
        callback = null;
        IsOpen = false;
        GetComponent<LerpPosition>().targetLocation = new Vector2(0, 700);
    }
}
