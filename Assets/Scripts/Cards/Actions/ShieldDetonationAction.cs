using System.Collections.Generic;
using Entities;
using Cards.CardEvents;
using UnityEngine;

namespace Cards.Actions
{
    public class ShieldDetonationAction : AbstractAction
    {
        private const float ShieldDamageMultiplier = 0.75f;

        public ShieldDetonationAction(int baseCost, string color, AbstractEntity entity) : base(baseCost, color, entity)
        {

        }

        public override List<AbstractCardEvent> Activate(CardMonobehaviour cardMono)
        {
            return new List<AbstractCardEvent>();
        }

        public override List<AbstractCardEvent> Activate(CardMonobehaviour cardMono, bool previewMode)
        {
            return new List<AbstractCardEvent>();
        }

        public override List<AbstractCardEvent> Activate(CardPlayContext context)
        {
            List<AbstractCardEvent> cardEvents = new List<AbstractCardEvent>();
            int amount = CurrentDetonationDamage(context?.SourceEntity);

            if (amount <= 0)
                return cardEvents;

            if (context?.Targets == null)
                return cardEvents;

            if (context.Targets.TryGetFirstEntity(out AbstractEntity target))
                cardEvents.Add(new AttackCardEvent(target.positionRowCol, amount, manual: false));
            else if (context.Targets.TryGetFirstPosition(out Vector2Int targetPosition))
                cardEvents.Add(new AttackCardEvent(targetPosition, amount, manual: false));

            return cardEvents;
        }

        public override List<AbstractCardEvent> Preview(CardMonobehaviour cardMono)
        {
            return Activate(cardMono, previewMode: true);
        }

        public override List<AbstractCardEvent> Preview(CardPlayContext context)
        {
            return Activate(context);
        }

        public override string GetText()
        {
            return "Deal <attack> to target equal to 75% of current <shield>";
        }

        public override string GetText(CardActionPreview preview)
        {
            int amount = CurrentDetonationDamage();
            int finalAmount = preview.GetFirstFinalValue(CardPreviewKeys.Damage, amount);
            return "Deal <attack> to target equal to 75% of current <shield> (" +
                   preview.FormatValue("", amount, finalAmount) + ")";
        }

        public override string ToSimpleText()
        {
            return "<sprite name=Damage4>";
        }

        public override List<RectTransform> UpdateGraphic(GameObject diagram, GameObject tilePrefab,
            GameObject arrowPrefab)
        {
            return new List<RectTransform>();
        }

        public override string ToString()
        {
            return "Shield Detonation";
        }

        private int CurrentDetonationDamage()
        {
            return CurrentDetonationDamage(entity);
        }

        private int CurrentDetonationDamage(AbstractEntity source)
        {
            if (source == null)
                return 0;

            return Mathf.FloorToInt(source.Shield * ShieldDamageMultiplier);
        }

    }
}
