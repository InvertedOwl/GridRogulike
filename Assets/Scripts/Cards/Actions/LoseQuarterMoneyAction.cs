using System.Collections.Generic;
using Cards.CardEvents;
using Entities;
using UnityEngine;

namespace Cards.Actions
{
    public class LoseQuarterMoneyAction : AbstractAction
    {
        public LoseQuarterMoneyAction(int baseCost, string color, AbstractEntity entity) : base(baseCost, color, entity)
        {
        }

        public override List<AbstractCardEvent> Activate(CardMonobehaviour cardMono)
        {
            return new List<AbstractCardEvent> { new GainMoneyCardEvent(-CurrentMoneyLoss()) };
        }

        public override string GetText()
        {
            return "Lose 25% of your money";
        }

        public override string GetText(CardActionPreview preview)
        {
            return "Lose 25% of your money";
        }

        public override List<RectTransform> UpdateGraphic(GameObject diagram, GameObject tilePrefab,
            GameObject arrowPrefab)
        {
            return new List<RectTransform>();
        }

        public override string ToString()
        {
            return "Lose 25% Money";
        }

        private int CurrentMoneyLoss()
        {
            if (RunInfo.Instance == null)
                return 0;

            return Mathf.FloorToInt(RunInfo.Instance.Money * 0.25f);
        }
    }
}
