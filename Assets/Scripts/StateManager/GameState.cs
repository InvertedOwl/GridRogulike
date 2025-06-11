namespace StateManager
{
    using UnityEngine;

    public abstract class GameState : MonoBehaviour
    {
        public virtual void Enter() { }
        public virtual void Exit() { }

        protected GameStateManager Manager => GameStateManager.Instance;
    }

}