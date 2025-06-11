using System;
using System.Collections.Generic;
using StateManager;
using Types.Actions;
using UnityEngine;

namespace Entities.Enemies
{
    public class TestEnemy: Enemy
    {
        public void Start()
        {
            AvailableActions.Add(new AttackAction(1, "basic", this, "s", 1, 10));
        }

        public override void MakeTurn()
        {
            // Simple ai (Random moves)
            // Need to figure some sort of alpha beta pruning thing for the ai
            Debug.Log("Enemy Making turn");
            Debug.Log(AvailableActions.Count);
            AvailableActions[random.Next(AvailableActions.Count)].Activate();
            if (GameStateManager.Instance.GetCurrent<PlayingState>() is { } playing)
            {
                playing.EntityEndTurn();
            }

        }

        public override List<AbstractAction> NextTurn()
        {
            throw new System.NotImplementedException();
            return null;
        }
    }
}