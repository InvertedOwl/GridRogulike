using UnityEngine;
using Util;

namespace StateManager
{
    public class MapState: GameState
    {
        public LerpPosition window;
        
        public override void Enter()
        {
            window.targetLocation = new Vector2(0, 0);
        }

        public override void Exit()
        {
            window.targetLocation = new Vector2(0, 750);
        }
    }
}