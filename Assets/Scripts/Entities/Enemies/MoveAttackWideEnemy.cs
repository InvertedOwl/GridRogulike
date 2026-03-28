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
    public class MoveAttackWideEnemy : NonPlayerEntity
    {

        private int movementPerTurn = 1;
        public int DefaultDamage = 10;
        
        public void Awake()
        {
            AvailableActions.Add(new AttackAction(1, "basic", this, "n", 1, DefaultDamage));
            AvailableActions.Add(new AttackAction(1, "basic", this, "s", 1, DefaultDamage));
            AvailableActions.Add(new AttackAction(1, "basic", this, "ne", 1, DefaultDamage));
            AvailableActions.Add(new AttackAction(1, "basic", this, "nw", 1, DefaultDamage));
            AvailableActions.Add(new AttackAction(1, "basic", this, "se", 1, DefaultDamage));
            AvailableActions.Add(new AttackAction(1, "basic", this, "sw", 1, DefaultDamage));
        }
        

        public override IEnumerator MakeTurn()
        {
            if (_plannedAction.Count == 0)
            {
                yield break;
            }

            foreach (AbstractAction action in _plannedAction)
            {
                foreach (AbstractCardEvent cardEvent in action.Activate(null))
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
            }

            
            _plannedAction.Clear();
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
                    if (e.entityType == EntityType.Player)
                    {
                        _plannedAction.Add(action);
                    }
                }
            }

            if (_plannedAction.Count > 0)
            {
                AttackAction attack = (AttackAction) _plannedAction[0];

                foreach (string dir in HexGridManager.neighborDirections[attack.Direction])
                {
                    _plannedAction.Add(new AttackAction(1, "basic", this, dir, 1, DefaultDamage));                    
                }
            }


            if (_plannedAction.Count == 0)
            {
                // Move towards player if can't attack them (Add player as non blocker)
                PlayingState state = GameStateManager.Instance.GetCurrent<PlayingState>();
                Dictionary<Vector2Int, int> distanceMap = CalculateDistanceMap(state.player.positionRowCol, state, state.player);
                
                Vector2Int currentPosition = positionRowCol;
                for (int i = 0; i < movementPerTurn; i++)
                {
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
                        Vector2Int movedHex = HexGridManager.MoveHex(currentPosition, dir, 1);

                        if (distanceMap.ContainsKey(movedHex) && distanceMap[movedHex] < minDistance && distanceMap[movedHex] != -1)
                        {
                            minDistance = distanceMap[movedHex];
                            actionToTake = a;
                        }
                    }

                    currentPosition = HexGridManager.MoveHex(currentPosition, actionToTake.Direction, 1);
                
                    _plannedAction.Add(actionToTake);
                    
                    if (distanceMap[currentPosition] <= 1)
                    {
                        break;
                    }
                }   
            }
        }

        // For assigning to things, need to tell controllers what enemies next turn is
        public override List<AbstractAction> NextTurn()
        {
            if (_plannedAction.Count == 0)
            {
                PlanTurn();
            }
            
            return _plannedAction;
        }
        
    }
}