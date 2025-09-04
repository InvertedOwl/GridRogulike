using Entities;
using Grid;
using StateManager;
using UnityEngine;

namespace Types.Actions
{
    public class AttackAction: AbstractAction
    {
        private string _direction;
        public string Direction { get { return _direction; } set { _direction = value; } }
        private int _distance;
        public int Distance { get { return _distance; } set { _distance = value; } }
        private int _amount;
        public int Amount { get { return _amount; } set { _amount = value; } }
        private HexGridManager _grid;
        public AttackAction(int baseCost, string color, AbstractEntity entity, string direction, int distance, int _amount) : base(baseCost, color, entity)
        {
            this._direction = direction;
            this._distance = distance;
            this._amount = _amount;
            _grid = HexGridManager.Instance;

        }

        public override void Activate()
        {
            if (GameStateManager.Instance.GetCurrent<PlayingState>() is { } playing)
            {
                Debug.Log(playing);
                Debug.Log(_direction);
                Debug.Log(_distance);
                Debug.Log(_amount);
                playing.DamageEntities(HexGridManager.MoveHex(this.entity.positionRowCol, this._direction, _distance), _amount);
            }
        }

        public override void Hover()
        {

        }
        
        public override string ToString()
        {
            return "Attack " + this._distance + " " + FixDirection(this._direction) + " D:" + this._amount;
        }
    }
}