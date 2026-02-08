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
        private int movementPerTurn = 1;
        
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
                yield break;
            }
            
            // TODO: Animation for action ?
            _plannedAction?.Activate(null).ForEach(action => {action.Activate(this);});
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

        // For assigning to things, need to tell controllers what enemies next turn is
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