using System.Collections.Generic;
using Cards.CardEvents;
using Entities;
using UnityEngine;

namespace Cards.Actions
{
    public class TeleportToStartingTileAction : AbstractAction
    {
        public TeleportToStartingTileAction(int baseCost, string color, AbstractEntity entity)
            : base(baseCost, color, entity)
        {
        }

        public override List<AbstractCardEvent> Activate(CardMonobehaviour cardMono)
        {
            return new List<AbstractCardEvent> { new TeleportPlayerToStartingTileCardEvent() };
        }

        public override string GetText()
        {
            return "Teleport to the starting tile";
        }

        public override string GetText(CardActionPreview preview)
        {
            return "Teleport to the starting tile";
        }

        public override List<RectTransform> UpdateGraphic(GameObject diagram, GameObject tilePrefab, GameObject arrowPrefab)
        {
            return new List<RectTransform>();
        }

        public override string ToString()
        {
            return "Teleport To Starting Tile";
        }
    }
}
