using System.Collections.Generic;
using Cards.CardEvents;
using Entities;
using UnityEngine;

namespace Cards.Actions
{
    public class RootSelfForCombatAction : AbstractAction
    {
        public RootSelfForCombatAction(int baseCost, string color, AbstractEntity entity) : base(baseCost, color, entity)
        {
        }

        public override List<AbstractCardEvent> Activate(CardMonobehaviour cardMono)
        {
            return new List<AbstractCardEvent> { new BlockPlayerMovementThisCombatCardEvent() };
        }

        public override string GetText()
        {
            return "Cannot move for the rest of combat.";
        }

        public override List<RectTransform> UpdateGraphic(GameObject diagram, GameObject tilePrefab, GameObject arrowPrefab)
        {
            return new List<RectTransform>();
        }

        public override string ToString()
        {
            return "Root self this combat";
        }
    }
}
