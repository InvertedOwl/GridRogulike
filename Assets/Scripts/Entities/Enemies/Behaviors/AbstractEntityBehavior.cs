using System.Collections;
using System.Collections.Generic;
using Cards.Actions;
using UnityEngine;

namespace Entities.Enemies
{
    public abstract class AbstractEntityBehavior : MonoBehaviour
    {
        public NonPlayerEntity self;
        
        public abstract IEnumerator MakeTurn();
        public abstract List<AbstractAction> NextTurn();
    }
}