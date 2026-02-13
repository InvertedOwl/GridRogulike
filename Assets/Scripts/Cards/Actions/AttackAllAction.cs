using System.Collections.Generic;
using Entities;
using StateManager;
using Cards.CardEvents;
using UnityEngine;

namespace Cards.Actions
{
    public class AttackAllAction: AbstractAction
    {
        private int _amount;
        public AttackAllAction(int baseCost, string color, AbstractEntity entity, int amount) : base(baseCost, color, entity)
        {
            _amount = amount;
            this.entity = entity;
        }

        public override List<AbstractCardEvent> Activate(CardMonobehaviour cardMono)
        {
            List<AbstractCardEvent> cardEvents = new List<AbstractCardEvent>();
            if (GameStateManager.Instance.IsCurrent<PlayingState>())
            {
                PlayingState playing = GameStateManager.Instance.GetCurrent<PlayingState>();
                foreach (AbstractEntity ent in playing.GetEntities())
                {
                    if (entity == ent)
                        continue;
                    cardEvents.Add(new AttackCardEvent(ent.positionRowCol, _amount, manual:false));
                    
                }
            }
            
            return cardEvents;
        }

        
        public override void HoverOn()
        {
        }

        public override void HoverOff()
        {
        }

        public override List<RectTransform> UpdateGraphic(GameObject diagram, GameObject tilePrefab, GameObject arrowPrefab)
        {
            GameObject basic = GameObject.Instantiate(tilePrefab, diagram.transform);
            return new List<RectTransform> { basic.GetComponent<RectTransform>() };
        }

        public override string GetText()
        {
            return "Attack All " +  _amount + "<attack>";
        }

        public override string ToString()
        {
            return "Attack All " + _amount ;
        }
    }
}