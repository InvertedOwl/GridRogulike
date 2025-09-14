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
            AvailableActions.Add(new AttackAction(1, "basic", this, "s", 1, 10));
        }

        public override IEnumerator MakeTurn()
        {
            yield return new WaitForSeconds(.25f);
            AvailableActions[random.Next(AvailableActions.Count)].Activate();
            yield return new WaitForSeconds(.25f);
            
            
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