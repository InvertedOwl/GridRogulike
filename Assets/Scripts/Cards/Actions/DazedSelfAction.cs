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
            return Activate(cardMono, previewMode: false);
        }

        public override List<AbstractCardEvent> Activate(CardMonobehaviour cardMono, bool previewMode)
        {
            return new List<AbstractCardEvent>
            {
                new ApplyStatusSelfCardEvent(
                    new DazedStatus(dazedAmount, GetStableActionRandom(cardMono, previewMode, "dazed")))
            };
        }

        public override List<RectTransform> UpdateGraphic(GameObject diagram, GameObject tilePrefab, GameObject arrowPrefab)
        {
            return new List<RectTransform> { };
        }

        public override string GetText()
        {
            return  "Apply " + dazedAmount + " <dazed> to self";
        }

        public override string GetText(CardActionPreview preview)
        {
            int finalAmount = preview.GetTotalFinalValue(CardPreviewKeys.StatusAmount, dazedAmount);
            return "Apply " + preview.FormatValue("<dazed>", dazedAmount, finalAmount) + " to self";
        }

        public override string ToString()
        {
            return "Buff " + this.dazedAmount;
        }
    }
}
