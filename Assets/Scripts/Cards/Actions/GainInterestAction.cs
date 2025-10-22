using System.Collections.Generic;
using Entities;
using Grid;
using Cards.CardEvents;
using UnityEngine;

namespace Cards.Actions
{
    public class GainInterestAction : AbstractAction
    {
        public GainInterestAction(int baseCost, string color, AbstractEntity entity) : base(baseCost, color, entity)
        {

        }

        public override List<AbstractCardEvent> Activate(CardMonobehaviour cardMono)
        {
            return new List<AbstractCardEvent> { new GainMoneyCardEvent(RunInfo.Instance.Money/3) };
        }

        public override void Hover()
        {

        }
        
        public override string GetText()
        {
            return "$1 for every 3$ owned";
        }
        
        public override List<RectTransform> UpdateGraphic(GameObject diagram, GameObject tilePrefab,
            GameObject arrowPrefab)
        {
            return new List<RectTransform>();
        }
        
        public override string ToString()
        {
            return "Interest";
        }
        
    }
}