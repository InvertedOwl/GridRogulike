using System;
using System.Collections.Generic;
using Entities;
using StateManager;
using Cards.CardEvents;
using Newtonsoft.Json;
using UnityEngine;


namespace Cards.Actions
{
    [Serializable]
    public abstract class AbstractAction
    {
        [SerializeField] public int _baseCost;
        [SerializeField] public string _color;
        [JsonIgnore] private AbstractEntity _entity;
        [SerializeField] public bool visible = true;

        public virtual string Icon
        {
            get
            {
                return "Question";
            }
        }

        public virtual string ToSimpleText()
        {
            return "<sprite name=question>";
        }
        
        
        public static RandomState guidRandom = RunInfo.NewRandom("aguid".GetHashCode());
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ResetStaticsOnLoad()
        {
            guidRandom = RunInfo.NewRandom("aguid".GetHashCode());
        }
        
        public static string GenerateDeterministicId()
        {
            byte[] bytes = new byte[16];
            guidRandom.NextBytes(bytes);
            return new Guid(bytes).ToString();
        }
        
        protected RandomState _actionRandom = RunInfo.NewRandom(GenerateDeterministicId().GetHashCode());
        
        [JsonIgnore]
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
        
        public AbstractAction(int baseCost, string color, AbstractEntity entity, bool visible = true)
        {
            this._baseCost = baseCost;
            this._color = color;
            this.entity = entity;
            this.visible = visible;
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
