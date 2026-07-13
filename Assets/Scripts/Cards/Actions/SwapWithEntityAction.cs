using System.Collections.Generic;
using Cards.CardEvents;
using Entities;
using UnityEngine;

namespace Cards.Actions
{
    public class SwapWithEntityAction : AbstractAction
    {
        public SwapWithEntityAction(int baseCost, string color, AbstractEntity entity)
            : base(baseCost, color, entity)
        {
        }

        public override List<AbstractCardEvent> Activate(CardMonobehaviour cardMono)
        {
            return new List<AbstractCardEvent>();
        }

        public override List<AbstractCardEvent> Activate(CardPlayContext context)
        {
            if (context?.Targets == null ||
                !context.Targets.TryGetFirstEntity(out AbstractEntity target))
            {
                return new List<AbstractCardEvent>();
            }

            AbstractEntity source = context.SourceEntity ?? entity;
            if (source == null ||
                target == null ||
                source == target ||
                source.Health <= 0 ||
                target.Health <= 0)
            {
                return new List<AbstractCardEvent>();
            }

            return new List<AbstractCardEvent>
            {
                new SwapEntitiesCardEvent(source, target)
            };
        }

        public override List<AbstractCardEvent> Preview(CardPlayContext context)
        {
            return Activate(context);
        }

        public override string GetText()
        {
            return "Swap positions with random enemy";
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
            return "Swap With Entity";
        }
    }
}
