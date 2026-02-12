using System.Collections.Generic;
using Entities;
using Cards.CardEvents;
using UnityEngine;

namespace Cards.Actions
{
    public class DrawCardAction : AbstractAction
    {
        private int _amount;
        public DrawCardAction(int baseCost, string color, AbstractEntity entity, int amount) : base(baseCost, color, entity)
        {
            this._amount = amount;
        }

        public override List<AbstractCardEvent> Activate(CardMonobehaviour cardMono)
        {
            return new List<AbstractCardEvent> { new DrawCardEvent(_amount) };
        }

        public override List<RectTransform> UpdateGraphic(GameObject diagram, GameObject tilePrefab,
            GameObject arrowPrefab)
        {
            return new List<RectTransform>();
        }

        public override string GetText()
        {
            return _amount + "<draw>";
        }

        public override string ToString()
        {
            return "Draw Card ";
        }
    } }