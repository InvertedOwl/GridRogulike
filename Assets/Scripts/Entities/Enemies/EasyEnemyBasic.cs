using System.Collections;
using System.Collections.Generic;
using Cards.Actions;
using Cards.CardEvents;
using Grid;
using StateManager;
using UnityEngine;
using Util;

namespace Entities.Enemies
{
    public class EasyEnemyBasic : Enemy
    {
        private AbstractAction _plannedAction;
        private int movementPerTurn = 1;
        public int DefaultDamage = 10;
        
        public void Awake()
        {
            AvailableActions.Add(new AttackAction(1, "basic", this, "n", 1, DefaultDamage));
            AvailableActions.Add(new AttackAction(1, "basic", this, "s", 1, DefaultDamage));
            AvailableActions.Add(new AttackAction(1, "basic", this, "s", 1, DefaultDamage));
            AvailableActions.Add(new AttackAction(1, "basic", this, "ne", 1, DefaultDamage));
            AvailableActions.Add(new AttackAction(1, "basic", this, "nw", 1, DefaultDamage));
            AvailableActions.Add(new AttackAction(1, "basic", this, "se", 1, DefaultDamage));
            AvailableActions.Add(new AttackAction(1, "basic", this, "sw", 1, DefaultDamage));
        }
        

        public override IEnumerator MakeTurn()
        {
            if (_plannedAction == null)
            {
                yield break;
            }

            foreach (AbstractCardEvent cardEvent in _plannedAction?.Activate(null))
            {

                if (cardEvent is AttackCardEvent)
                {
                    AttackCardEvent attackCardEvent = (AttackCardEvent)cardEvent;
                    Vector2Int targetPos = HexGridManager.MoveHex(positionRowCol, attackCardEvent.direction,
                        attackCardEvent.distance);
                    transform.localPosition +=
                        ((Vector3)HexGridManager.GetHexCenter(targetPos.x, targetPos.y) - transform.position)
                        .normalized * 0.5f;
                }

                foreach (AbstractCardEvent modifiedEvent in ModifyEvents(new List<AbstractCardEvent> { cardEvent }))
                {
                    modifiedEvent.Activate(this);
                }


                yield return new WaitForSeconds(0.5f);
            }
            
            _plannedAction = null;
        }

        
        
        private void PlanTurn()
        {
            Debug.Log("PlanTurn! " + AvailableActions.Count);
            
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
            
            if (_plannedAction == null)
            {
                // Move towards player if can't attack them (Add player as non blocker)
                Dictionary<Vector2Int, int> distanceMap = CalculateDistanceMap(GameStateManager.Instance.GetCurrent<PlayingState>().player.positionRowCol, GameStateManager.Instance.GetCurrent<PlayingState>(), GameStateManager.Instance.GetCurrent<PlayingState>().player);
                
                List<MoveAction> moveActions = new List<MoveAction>();
                moveActions.Add(new MoveAction(1, "basic", this, "n", 1));
                moveActions.Add(new MoveAction(1, "basic", this, "ne", 1));
                moveActions.Add(new MoveAction(1, "basic", this, "nw", 1));
                moveActions.Add(new MoveAction(1, "basic", this, "s", 1));
                moveActions.Add(new MoveAction(1, "basic", this, "se", 1));
                moveActions.Add(new MoveAction(1, "basic", this, "sw", 1));

                int minDistance = int.MaxValue;
                MoveAction actionToTake = null;
                
                foreach (MoveAction a in moveActions)
                {
                    string dir = a.Direction;
                    Vector2Int movedHex = HexGridManager.MoveHex(this.positionRowCol, dir, 1);

                    if (distanceMap.ContainsKey(movedHex) && distanceMap[movedHex] < minDistance && distanceMap[movedHex] != -1)
                    {
                        minDistance = distanceMap[movedHex];
                        actionToTake = a;
                    }
                }
                
                _plannedAction = actionToTake;
                if (actionToTake != null)
                {
                    Debug.Log("Just Planned " + actionToTake.Direction + " " + actionToTake.Distance);
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