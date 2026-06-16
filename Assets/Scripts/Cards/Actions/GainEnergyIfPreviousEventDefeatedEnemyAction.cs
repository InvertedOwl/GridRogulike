using System.Collections.Generic;
using Cards.CardEvents;
using Entities;
using UnityEngine;

namespace Cards.Actions
{
    public class GainEnergyIfPreviousEventDefeatedEnemyAction : AbstractAction
    {
        public int Amount;

        public GainEnergyIfPreviousEventDefeatedEnemyAction(int baseCost, string color, AbstractEntity entity, int amount)
            : base(baseCost, color, entity)
        {
            Amount = amount;
        }

        public override List<AbstractCardEvent> Activate(CardMonobehaviour cardMono)
        {
            return new List<AbstractCardEvent>
            {
                new IfPreviousEventDefeatedEnemyCardEvent(new List<AbstractCardEvent>
                {
                    new GainEnergyCardEvent(Amount)
                })
            };
        }

        public override string GetText()
        {
            return "If this defeats an enemy, gain " + Amount + " <energy>";
        }

        public override string GetText(CardActionPreview preview)
        {
            int finalAmount = preview.GetTotalFinalValue(CardPreviewKeys.Energy, Amount);
            return "If this defeats an enemy, gain " + preview.FormatValue("<energy>", Amount, finalAmount);
        }

        public override List<RectTransform> UpdateGraphic(GameObject diagram, GameObject tilePrefab, GameObject arrowPrefab)
        {
            return new List<RectTransform>();
        }

        public override string ToString()
        {
            return "Gain " + Amount + " energy if previous event defeated enemy";
        }
    }
}
