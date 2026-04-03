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
    public class DirectionalAttackEnemy : AbstractEntityBehavior
    {
        public int DefaultDamage = 10;

        private string _direction = "n";
        

        public override IEnumerator MakeTurn()
        {
            for (int i = 0; i < 4; i++)
            {
                foreach (AbstractCardEvent cardEvent in self.ModifyEvents(new AttackAction(0, "basic", self, _direction, i + 1, DefaultDamage).Activate(null)))
                {
                    cardEvent.Activate(self);
                    Vector2Int pos = HexGridManager.MoveHex(self.positionRowCol, _direction, i+1);
                    
                    transform.localPosition +=
                        
                        ((Vector3)HexGridManager.GetHexCenter(pos.x, pos.y) - transform.position) * 0.5f;
                    yield return new WaitForSeconds(0.25f * (1/GameplayNavSettings.speed));
                }
            }
        }

        

        // For assigning to things, need to tell controllers what enemies next turn is
        public override List<AbstractAction> NextTurn()
        {
            string[] directions = new[] { "n", "ne", "nw", "s", "se", "sw" };
            _direction = directions[self.EntityRandom.Next(0, directions.Length)];

            string maxDirection = "-";
            int maxDirectionCount = 0;
            
            List<AbstractAction> actions = new List<AbstractAction>();
            PlayingState playingState = GameStateManager.Instance.GetCurrent<PlayingState>();
            foreach (string direction in directions)
            {
                int countDirection = 0;
                
                for (int i = 0; i < 4; i++)
                {
                    if (HexGridManager.Instance.GetAllGridPositions()
                        .Contains(HexGridManager.MoveHex(self.positionRowCol, direction, i + 1)))
                        countDirection++;
                    
                    List<AbstractEntity> entitiesOnHex = new List<AbstractEntity>();
                    playingState.EntitiesOnHex(HexGridManager.MoveHex(self.positionRowCol, direction, i+1), out entitiesOnHex);
                    
                    bool hasPlayer = false;
                    foreach (AbstractEntity entityOnHex in entitiesOnHex)
                    {
                        if (entityOnHex.entityType == EntityType.Player)
                        {
                            hasPlayer = true;
                        }
                    }

                    if (hasPlayer)
                    {
                        maxDirectionCount = 1000;
                        maxDirection = direction;
                    }
                }

                if (countDirection > maxDirectionCount)
                {
                    maxDirectionCount = countDirection;
                    maxDirection = direction;
                }
            }
            
            _direction = maxDirection;

            for (int i = 0; i < 4; i++)
            {
                actions.Add(new AttackAction(0, "basic", self, _direction, i + 1, DefaultDamage));
            }
            self.plannedAction = actions;
            return actions;
        }
        
    }
}