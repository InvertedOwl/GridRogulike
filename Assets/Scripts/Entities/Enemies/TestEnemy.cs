using System;
using System.Collections;
using System.Collections.Generic;
using Cards.Actions;
using StateManager;
using UnityEngine;

namespace Entities.Enemies
{
    public class TestEnemy: Enemy
    {
        public void Awake()
        {
            AvailableActions.Add(new PoisonAttackAction(1, "basic", this, "s", 1, 10, 2));
        }

        public override IEnumerator MakeTurn()
        {
            yield return new WaitForSeconds(.25f);
            Debug.Log("Making Turn");
            AvailableActions[random.Next(AvailableActions.Count)].Activate().ForEach(action => {action.Activate(this);});
            yield return new WaitForSeconds(.75f);
            
            
            if (GameStateManager.Instance.GetCurrent<PlayingState>() is { } playing)
            {
                playing.EntityEndTurn();
            }
            yield break;
        }

        public override List<AbstractAction> NextTurn()
        {
            return AvailableActions;
        }
    }
}