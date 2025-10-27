using System;
using System.Collections.Generic;
using Entities;
using StateManager;
using Cards.CardEvents;
using UnityEngine;
using Random = System.Random;

namespace Cards.Actions
{
    public abstract class AbstractAction
    {
        private int _baseCost;
        private string _color;
        private AbstractEntity _entity;
        
        public static Random guidRandom = RunInfo.NewRandom("aguid".GetHashCode());
        public static string GenerateDeterministicId()
        {
            byte[] bytes = new byte[16];
            guidRandom.NextBytes(bytes);
            return new Guid(bytes).ToString();
        }
        
        protected Random _actionRandom = RunInfo.NewRandom(GenerateDeterministicId().GetHashCode());
        
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
        
        public abstract List<AbstractCardEvent> Activate(CardMonobehaviour cardMono);
        public bool hovering = false;

        public void Hover()
        {
            if (!hovering)
            {
                hovering = true;
                HoverOn();
            }
        }
        public void NotHover()
        {
            if (hovering)
            {
                hovering = false;
                HoverOff();
            }
        }


        public virtual void HoverOn()
        {
            
        }
        public virtual void HoverOff()
        {
            
        }
        
        public abstract string GetText();
        public abstract List<RectTransform> UpdateGraphic(GameObject diagram, GameObject tilePrefab, GameObject arrowPrefab);
        
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
