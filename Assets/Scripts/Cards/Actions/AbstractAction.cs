using System;
using Entities;
using StateManager;
using Types.CardEvents;

namespace Cards.Actions
{
    public abstract class AbstractAction
    {
        private int _baseCost;
        private string _color;
        private AbstractEntity _entity;
        public AbstractEntity entity
        {
            get
            {
                
                if (_entity != null)
                {
                    return _entity;
                }

                try
                {
                    return GameStateManager.Instance.GetCurrent<PlayingState>().player;
                }
                catch (Exception _)
                {
                    return null;
                }
            } 
            set { _entity = value; } 
        }
        
        // Update later to check if cost has changed (?)
        public int Cost { get { return _baseCost; } }
        
        public AbstractAction(int baseCost, string color, AbstractEntity entity)
        {
            this._baseCost = baseCost;
            this._color = color;
            this.entity = entity;
        }
        
        public abstract AbstractCardEvent Activate();
        public abstract void Hover();
        
        public string FixDirection(string direction)
        {
            return direction.Replace("n", "u").Replace("e", "r").Replace("s", "d").Replace("w", "l").ToUpper();
        }
        
        public override string ToString()
        {
            return "Abstract Action. THIS IS AN ERROR.";
        }
    }
}
