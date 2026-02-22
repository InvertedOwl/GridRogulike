using System;
using System.Collections;
using System.Collections.Generic;
using Cards.Actions;
using StateManager;
using UnityEngine;

namespace Entities.Enemies
{
    public class TestNonPlayerEntity: NonPlayerEntity
    {
        
        public void Awake()
        {
            AvailableActions.Add(new PoisonAttackAction(1, "basic", this, "s", 1, 10, 2));
        }

        public override IEnumerator MakeTurn()
        {
            Debug.Log("Making Turn");
            AvailableActions[_entityRandom.Next(AvailableActions.Count)].Activate(null).ForEach(action => {action.Activate(this);});
            yield break;
        }

        public override List<AbstractAction> NextTurn()
        {
            return AvailableActions;
        }
    }
}