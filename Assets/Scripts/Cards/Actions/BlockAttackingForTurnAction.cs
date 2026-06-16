using System.Collections.Generic;
using Cards.CardEvents;
using Entities;
using Types.CardRestrictions;
using UnityEngine;

namespace Cards.Actions
{
    public class BlockAttackingForTurnAction : AbstractAction
    {
        public BlockAttackingForTurnAction(int baseCost, string color, AbstractEntity entity)
            : base(baseCost, color, entity)
        {
        }

        public override List<AbstractCardEvent> Activate(CardMonobehaviour cardMono)
        {
            return new List<AbstractCardEvent>
            {
                new AddGeneratedEventCardRestrictionEvent(
                    RestrictedCardEventKind.Attack,
                    CardPlayRestrictionDuration.Turn,
                    "Cannot attack this turn.")
            };
        }

        public override string GetText()
        {
            return "Cannot attack for the rest of this turn";
        }

        public override string GetText(CardActionPreview preview)
        {
            return GetText();
        }

        public override List<RectTransform> UpdateGraphic(GameObject diagram, GameObject tilePrefab, GameObject arrowPrefab)
        {
            return new List<RectTransform>();
        }

        public override string ToString()
        {
            return "Block Attacking For Turn";
        }
    }
}
