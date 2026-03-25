using Serializer;

namespace StateManager
{
    using System;
    using UnityEngine;

    public abstract class GameState : MonoBehaviour
    {
        public static object SaveData;
        
        public virtual void Enter() { }
        public virtual void Exit() { }

        public virtual PlayingStateSaveData CaptureSaveData() => null;

        public virtual Type GetSaveDataType() => null;

        protected GameStateManager Manager => GameStateManager.Instance;
    }
}