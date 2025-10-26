using System.Collections.Generic;
using Entities;
using Grid;
using StateManager;
using Cards.CardEvents;
using Types.Statuses;
using UnityEngine;
using UnityEngine.UI;

namespace Cards.Actions
{
    public class AttackAction: AbstractAction
    {
        protected string _direction;
        public string Direction { get { return _direction; } set { _direction = value; } }
        protected int _distance;
        public int Distance { get { return _distance; } set { _distance = value; } }
        protected int _amount;
        public int Amount { get { return _amount; } set { _amount = value; } }
        protected HexGridManager _grid;
        public AttackAction(int baseCost, string color, AbstractEntity entity, string direction, int distance, int _amount) : base(baseCost, color, entity)
        {
            this._direction = direction;
            this._distance = distance;
            this._amount = _amount;
            _grid = HexGridManager.Instance;

        }

        public override List<AbstractCardEvent> Activate(CardMonobehaviour cardMono)
        {
            return new List<AbstractCardEvent> { new AttackCardEvent(_distance, _direction, _amount) };
        }

        
        public string arrowUUID;
        
        public override void HoverOn()
        {
            if (!entity) return;
            
            Vector2Int newCoords = HexGridManager.MoveHex(this.entity.positionRowCol, _direction, _distance);

            arrowUUID = SpriteArrowManager.Instance.CreateArrow(entity.positionRowCol, newCoords, Color.red, "AttackIcon", _amount);
        }

        public override void HoverOff()
        {
            SpriteArrowManager.Instance.DestroyArrow(arrowUUID);
        }

        public override List<RectTransform> UpdateGraphic(GameObject diagram, GameObject tilePrefab, GameObject arrowPrefab)
        {
            GameObject basic = GameObject.Instantiate(tilePrefab, diagram.transform);
            Vector2Int newPos =
                HexGridManager.MoveHex(new Vector2Int(0, 0), this.Direction, this.Distance);
            Vector2 newPosWorld = HexGridManager.GetHexCenter(newPos.x, newPos.y) * 46.2222f;
                
            basic.GetComponent<RectTransform>().localPosition = newPosWorld;
            basic.GetComponent<Image>().color = new Color(212/255.0f, 81/255.0f, 81/255.0f);
            basic.GetComponent<RectTransform>();
            return new List<RectTransform> { basic.GetComponent<RectTransform>() };
        }

        public override string GetText()
        {
            return Amount.ToString();
        }

        public override string ToString()
        {
            return "Attack " + this._distance + " " + FixDirection(this._direction) + " D:" + this._amount;
        }
    }
}