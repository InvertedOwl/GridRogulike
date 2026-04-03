using System;
using System.Collections;
using System.Collections.Generic;
using Cards.Actions;
using StateManager;
using UnityEngine;

namespace Entities.Enemies
{
    public class TestNonPlayerEntity: AbstractEntityBehavior
    {
        
        public void Start()
        {
            self.AvailableActions.Add(new PoisonAttackAction(1, "basic", self, "s", 1, 10, 2));
        }

        public override IEnumerator MakeTurn()
        {
            Debug.Log("Making Turn");
            self.AvailableActions[self.EntityRandom.Next(self.AvailableActions.Count)].Activate(null).ForEach(action => {action.Activate(self);});
            yield break;
        }

        public override List<AbstractAction> NextTurn()
        {
            return self.AvailableActions;
        }
    }
}