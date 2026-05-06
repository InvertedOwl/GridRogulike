using Serializer;

namespace StateManager
{
    using System;
    using UnityEngine;

    public abstract class GameState : MonoBehaviour
    {
        public static object SaveData;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ResetStaticsOnLoad()
        {
            ResetStatics();
        }

        public static void ResetStatics()
        {
            SaveData = null;
        }
        
        public virtual void Enter() { }
        public virtual void Exit() { }

        public virtual PlayingStateSaveData CaptureSaveData() => null;

        public virtual Type GetSaveDataType() => null;

        protected GameStateManager Manager => GameStateManager.Instance;
    }
}
