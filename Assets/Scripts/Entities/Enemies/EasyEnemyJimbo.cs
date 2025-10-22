using System.Collections;
using System.Collections.Generic;
using Cards.Actions;
using Grid;
using StateManager;
using UnityEngine;

namespace Entities.Enemies
{
    public class EasyEnemyJimbo : Enemy
    {
        private AbstractAction _plannedAction;
        
        public void Awake()
        {
            AvailableActions.Add(new AttackAction(1, "basic", this, "n", 1, 10));
            AvailableActions.Add(new AttackAction(1, "basic", this, "s", 1, 10));
            AvailableActions.Add(new AttackAction(1, "basic", this, "s", 1, 10));
            AvailableActions.Add(new AttackAction(1, "basic", this, "ne", 1, 10));
            AvailableActions.Add(new AttackAction(1, "basic", this, "nw", 1, 10));
            AvailableActions.Add(new AttackAction(1, "basic", this, "se", 1, 10));
            AvailableActions.Add(new AttackAction(1, "basic", this, "sw", 1, 10));
        }
        

        public override IEnumerator MakeTurn()
        {
            if (_plannedAction == null)
            {
                if (GameStateManager.Instance.GetCurrent<PlayingState>() is { } playing1)
                {
                    playing1.EntityEndTurn();
                }
                yield return new WaitForSeconds(.25f);
                yield break;
            }
            
            yield return new WaitForSeconds(.25f);
            _plannedAction?.Activate(null).ForEach(action => {action.Activate(this);});
            yield return new WaitForSeconds(.75f);
            
            
            if (GameStateManager.Instance.GetCurrent<PlayingState>() is { } playing)
            {
                playing.EntityEndTurn();
            }
        }

        private void PlanTurn()
        {
            // Plan next turn
            foreach (AttackAction action in AvailableActions)
            {
                string dir = action.Direction;

                List<AbstractEntity> entitiesOnHex = new List<AbstractEntity>();
                GameStateManager.Instance.GetCurrent<PlayingState>().EntitiesOnHex(HexGridManager.MoveHex(this.positionRowCol, dir, 1), out entitiesOnHex);

                foreach (AbstractEntity e in entitiesOnHex)
                {
                    if (e is Player)
                    {
                        _plannedAction = action;
                    }
                }
            }
        }

        public override List<AbstractAction> NextTurn()
        {
            if (_plannedAction == null)
            {
                PlanTurn();
            }
            
            return new List<AbstractAction>{_plannedAction};
        }
    }
}