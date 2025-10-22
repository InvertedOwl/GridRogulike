using System.Collections.Generic;
using Entities;
using Grid;
using NUnit.Framework;
using StateManager;
using Cards.CardEvents;
using Types.Tiles;
using UnityEngine;
using UnityEngine.UI;

namespace Cards.Actions
{
    public class MoveAction: AbstractAction
    {
        private string _direction;
        public string Direction { get { return _direction; } set { _direction = value; } }
        private int _distance;
        public int Distance { get { return _distance; } set { _distance = value; } }
        public MoveAction(int baseCost, string color, AbstractEntity entity, string direction, int distance) : base(baseCost, color, entity)
        {
            this._direction = direction;
            this._distance = distance;
        }

        public override List<AbstractCardEvent> Activate(CardMonobehaviour cardMono)
        {
            return new List<AbstractCardEvent> { new MoveCardEvent(_distance, _direction) };
        }

        public override void Hover()
        {
            if (!entity) return;
            
            HexGridManager instance = HexGridManager.Instance;
            Vector2Int newCoords = HexGridManager.MoveHex(this.entity.positionRowCol, _direction, _distance);

            // if (!instance.IsValidHex(newCoords)) Debug.Log("Invalid Hex");
        }
        public override string GetText()
        {
            return Distance.ToString();
        }
        public override List<RectTransform> UpdateGraphic(GameObject diagram, GameObject tilePrefab, GameObject arrowPrefab)
        {
            GameObject basic = GameObject.Instantiate(tilePrefab, diagram.transform);
            Vector2Int newPosT =
                HexGridManager.MoveHex(new Vector2Int(0, 0), this.Direction, this.Distance);
            Vector2 newPosWorldT = HexGridManager.GetHexCenter(newPosT.x, newPosT.y) * 46.2222f;
                
            basic.GetComponent<RectTransform>().localPosition = newPosWorldT;
            basic.GetComponent<Image>().color = TileData.tiles["basic"].color;
            
            GameObject move = GameObject.Instantiate(arrowPrefab, diagram.transform);
            move.GetComponent<ArrowController>().SetHeight(40 * this.Distance);
            Vector2Int newPosA =
                HexGridManager.MoveHex(new Vector2Int(0, 0), this.Direction, this.Distance);
            Vector2 newPosWorldA = HexGridManager.GetHexCenter(newPosA.x, newPosA.y);
                
            float angle = Vector2.SignedAngle(Vector2.up, newPosWorldA);
                
            move.transform.eulerAngles = new Vector3(0, 0, angle);
            
            return new List<RectTransform> { basic.GetComponent<RectTransform>(), move.GetComponent<RectTransform>() };
        }
        
        public override string ToString()
        {
            return "Move " + FixDirection(this._direction) + " " + this._distance;
        }
    }
}