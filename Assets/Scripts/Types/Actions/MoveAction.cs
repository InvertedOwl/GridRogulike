using Entities;
using Grid;
using StateManager;
using UnityEngine;

namespace Types.Actions
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

        public override void Activate()
        {
            if (!entity) return;

            if (GameStateManager.Instance.GetCurrent<PlayingState>() is { } playing)
                playing.MoveEntity(this.entity, this._direction, _distance);
        }

        public override void Hover()
        {
            if (!entity) return;
            
            HexGridManager instance = HexGridManager.Instance;
            Vector2Int newCoords = HexGridManager.MoveHex(this.entity.positionRowCol, _direction, _distance);

            // if (!instance.IsValidHex(newCoords)) Debug.Log("Invalid Hex");
        }
        
        public override string ToString()
        {
            return "Move " + FixDirection(this._direction) + " " + this._distance;
        }
    }
}