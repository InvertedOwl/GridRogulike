using System;
using System.Collections.Generic;
using Entities;
using Grid;
using StateManager;
using Cards.CardEvents;
using Types.Statuses;
using UnityEngine;
using UnityEngine.UI;
using Util;

namespace Cards.Actions
{
    public class AttackFromSteps: AbstractAction
    {
        
        public override string Icon
        {
            get
            {
                return "Damage4";
            }
        }
        
        protected string _direction;
        public string Direction { get { return _direction; } set { _direction = value; } }
        public int _distance;
        public int Distance { get { return _distance; } set { _distance = value; } }
        public int _amount;
        public int Amount { get { return _amount; } set { _amount = value; } }
        protected HexGridManager _grid;
        public AttackFromSteps(int baseCost, string color, AbstractEntity entity, string direction, int distance) : base(baseCost, color, entity)
        {
            this._direction = direction;
            this._distance = distance;
            this._amount = 0;
            _grid = HexGridManager.Instance;

        }

        public override List<AbstractCardEvent> Activate(CardMonobehaviour cardMono)
        {
            PlayingState playingState = GameStateManager.Instance.GetCurrent<PlayingState>();
            List<AbstractEntity> entities = new List<AbstractEntity>();
            if (!String.IsNullOrEmpty(_direction) && _distance > 0)
            {
                playingState.EntitiesOnHex(HexGridManager.MoveHex(entity.positionRowCol, _direction, _distance), out entities);
                bool containsFriend = false;
                foreach (AbstractEntity e in entities)
                {
                    if (e.entityType == entity.entityType)
                    {
                        containsFriend = true;
                    }
                }

                if (containsFriend)
                {
                    return new List<AbstractCardEvent>();
                }
            }

            
            return new List<AbstractCardEvent> { new AttackCardEvent(_distance, _direction, BattleStats.TilesMovedThisTurn * 5) };
        }

        public string arrowUUID;
        
        public override void HoverOn()
        {
            Debug.Log("Hovering on attack actions");
        }

        public override void HoverOff()
        {
            Debug.Log("Hovering off attack actions");
        }

        public override List<RectTransform> UpdateGraphic(GameObject diagram, GameObject tilePrefab, GameObject arrowPrefab)
        {
            GameObject basic = GameObject.Instantiate(tilePrefab, diagram.transform);
            return new List<RectTransform> { basic.GetComponent<RectTransform>() };
        }

        public override string GetText()
        {
            return "Deal damage equal to number of tiles moved this turn (0)";
        }

        public override string GetText(CardActionPreview preview)
        {
            return "Deal damage equal to number of tiles moved this turn (" + (BattleStats.TilesMovedThisTurn * 5) + ")";
        }

        public override string ToString()
        {
            return "Attack " + this._distance + " " + FixDirection(this._direction) + " D:" + this._amount;
        }
    }
}
