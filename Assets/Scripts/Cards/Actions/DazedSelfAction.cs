using System.Collections.Generic;
using Entities;
using Grid;
using StateManager;
using Cards.CardEvents;
using Types.Statuses;
using UnityEngine;
using UnityEngine.UI;
using Util;

namespace Cards.Actions
{
    public class DazedSelfAction: AbstractAction
    {
        public int dazedAmount;

        public DazedSelfAction(int baseCost, string color, AbstractEntity entity, int dazedAmount) : base(baseCost, color, entity)
        {
            this.dazedAmount = dazedAmount;
        }

        public override List<AbstractCardEvent> Activate(CardMonobehaviour cardMono)
        {
            return new List<AbstractCardEvent> { new ApplyStatusSelfCardEvent(new DazedStatus(dazedAmount, _actionRandom)) };
        }

        public override List<RectTransform> UpdateGraphic(GameObject diagram, GameObject tilePrefab, GameObject arrowPrefab)
        {
            return new List<RectTransform> { };
        }

        public override string GetText()
        {
            return  "Apply <dazed> " + dazedAmount + " to self";
        }

        public override string ToString()
        {
            return "Buff " + this.dazedAmount;
        }
    }
}