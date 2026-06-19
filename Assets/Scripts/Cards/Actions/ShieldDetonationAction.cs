using System.Collections.Generic;
using Entities;
using Cards.CardEvents;
using StateManager;
using UnityEngine;

namespace Cards.Actions
{
    public class ShieldDetonationAction : AbstractAction
    {
        public ShieldDetonationAction(int baseCost, string color, AbstractEntity entity) : base(baseCost, color, entity)
        {

        }

        public override List<AbstractCardEvent> Activate(CardMonobehaviour cardMono)
        {
            return Activate(cardMono, previewMode: false);
        }

        public override List<AbstractCardEvent> Activate(CardMonobehaviour cardMono, bool previewMode)
        {
            List<AbstractCardEvent> cardEvents = new List<AbstractCardEvent>();
            int amount = CurrentDetonationDamage();

            if (amount <= 0 ||
                GameStateManager.Instance == null ||
                !GameStateManager.Instance.IsCurrent<PlayingState>())
            {
                return cardEvents;
            }

            PlayingState playing = GameStateManager.Instance.GetCurrent<PlayingState>();
            foreach (AbstractEntity target in playing.GetEntities())
            {
                if (!playing.IsPlayerAttackTarget(target))
                    continue;

                cardEvents.Add(new AttackCardEvent(target.positionRowCol, amount, manual: false));
            }

            return cardEvents;
        }

        public override List<AbstractCardEvent> Activate(CardPlayContext context)
        {
            List<AbstractCardEvent> cardEvents = new List<AbstractCardEvent>();
            int amount = CurrentDetonationDamage(context?.SourceEntity);

            if (amount <= 0)
                return cardEvents;

            if (context?.Targets == null)
                return Activate(context?.CardMono, context?.PreviewMode ?? false);

            foreach (AbstractEntity target in context.Targets.TargetEntities)
            {
                if (target != null && target.Health > 0)
                    cardEvents.Add(new AttackCardEvent(target.positionRowCol, amount, manual: false));
            }

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
            return "Deal <sprite name=\"damage4\"> to all enemies equal to 50% of current <shield>";
        }

        public override string GetText(CardActionPreview preview)
        {
            int amount = CurrentDetonationDamage();
            int finalAmount = preview.GetFirstFinalValue(CardPreviewKeys.Damage, amount);
            return "Deal <sprite name=\"damage4\"> to all enemies equal to 50% of current <shield> (" +
                   preview.FormatValue("<attack>", amount, finalAmount) + ")";
        }

        public override string ToSimpleText()
        {
            return " <sprite name=shield>";
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

            return Mathf.FloorToInt(source.Shield * 0.5f);
        }

    }
}
