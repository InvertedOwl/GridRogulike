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

        public override List<AbstractAction> NextTurn()
        {
            return self.AvailableActions;
        }
    }
}