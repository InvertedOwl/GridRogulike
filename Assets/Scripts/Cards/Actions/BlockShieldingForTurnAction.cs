using System.Collections.Generic;
using Cards.CardEvents;
using Entities;
using Types.CardRestrictions;
using UnityEngine;

namespace Cards.Actions
{
    public class BlockShieldingForTurnAction : AbstractAction
    {
        public BlockShieldingForTurnAction(int baseCost, string color, AbstractEntity entity)
            : base(baseCost, color, entity)
        {
        }

        public override List<AbstractCardEvent> Activate(CardMonobehaviour cardMono)
        {
            return new List<AbstractCardEvent>
            {
                new AddGeneratedEventCardRestrictionEvent(
                    RestrictedCardEventKind.Shield,
                    CardPlayRestrictionDuration.Turn,
                    "Cannot gain shield this turn.")
            };
        }

        public override string GetText()
        {
            return "Cannot gain shield for the rest of this turn";
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
            return "Block Shielding For Turn";
        }
    }
}
