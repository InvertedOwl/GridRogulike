using System.Collections.Generic;
using Entities;
using Cards.CardEvents;
using UnityEngine;

namespace Cards.Actions
{
    public class GainStepsCardAction : AbstractAction
    {
        private int _steps;
        public GainStepsCardAction(int baseCost, string color, AbstractEntity entity, int steps) : base(baseCost, color, entity)
        {
            this._steps = steps;
        }

        public override List<AbstractCardEvent> Activate(CardMonobehaviour cardMono)
        {
            return new List<AbstractCardEvent> { new GainStepsCardEvent(_steps) };
        }

        public override List<RectTransform> UpdateGraphic(GameObject diagram, GameObject tilePrefab,
            GameObject arrowPrefab)
        {
            return new List<RectTransform>();
        }

        public override string GetText()
        {
            return _steps + "<move>";
        }

        public override string ToString()
        {
            return "Add Steps ";
        }
    } }