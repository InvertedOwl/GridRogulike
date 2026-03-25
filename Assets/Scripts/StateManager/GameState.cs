namespace StateManager
{
    using System;
    using UnityEngine;

    public abstract class GameState : MonoBehaviour
    {
        public virtual void Enter() { }
        public virtual void Exit() { }

        // public abstract object CaptureSaveData();
        // public abstract void RestoreSaveData(object data);
        //
        // public abstract  Type GetSaveDataType();

        protected GameStateManager Manager => GameStateManager.Instance;
    }
}