using System.Collections.Generic;
using Cards.CardEvents;
using Entities;
using UnityEngine;
using Util;

namespace Cards.Actions
{
    public class GainStepsForCardsPlayedThisTurnAction : AbstractAction
    {
        public GainStepsForCardsPlayedThisTurnAction(int baseCost, string color, AbstractEntity entity)
            : base(baseCost, color, entity)
        {
        }

        public override List<AbstractCardEvent> Activate(CardMonobehaviour cardMono)
        {
            int amount = Mathf.Max(0, BattleStats.CardsPlayedThisTurn - 1);
            return amount > 0
                ? new List<AbstractCardEvent> { new GainStepsCardEvent(amount) }
                : new List<AbstractCardEvent>();
        }

        public override string GetText()
        {
            return "Gain 1 <move> for every card played this turn $cardturn$";
        }

        public override string GetText(CardActionPreview preview)
        {
            int amount = BattleStats.CardsPlayedThisTurn;
            int finalAmount = preview.GetTotalFinalValue(CardPreviewKeys.Steps, amount);
            return "Gain " + preview.FormatValue("<move>", amount, finalAmount) +
                   " for every card played this turn $cardturn$";
        }

        public override List<RectTransform> UpdateGraphic(GameObject diagram, GameObject tilePrefab,
            GameObject arrowPrefab)
        {
            return new List<RectTransform>();
        }

        public override string ToString()
        {
            return "Gain Steps For Cards Played This Turn";
        }
    }
}
