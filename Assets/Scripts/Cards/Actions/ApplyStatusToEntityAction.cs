using System.Collections.Generic;
using Cards.Actions;
using Cards.CardEvents;
using Entities;
using Types.Statuses;
using UnityEngine;

namespace Cards.Actions
{
    public enum StatusApplicationType
    {
        Buffed,
        Dazed,
        Frost,
        Poison,
        Restless,
        Fall,
        Shocked,
        Energetic,
        Excited,
        Frozen,
        Sleepy,
        Ranged,
        Haste,
        Blind,
        Volatile,
        Dizzy,
        Daze,
        ShieldCarryover
    }

    public class ApplyStatusToEntityAction : AbstractAction
    {
        public AbstractEntity target;
        public StatusApplicationType statusType;
        public int amount;

        public ApplyStatusToEntityAction(
            int baseCost,
            string color,
            AbstractEntity entity,
            AbstractEntity target,
            StatusApplicationType statusType,
            int amount) : base(baseCost, color, entity)
        {
            this.target = target;
            this.statusType = statusType;
            this.amount = amount;
        }

        public override List<AbstractCardEvent> Activate(CardMonobehaviour cardMono)
        {
            return Activate(cardMono, previewMode: false);
        }

        public override List<AbstractCardEvent> Activate(CardMonobehaviour cardMono, bool previewMode)
        {
            return new List<AbstractCardEvent>
            {
                new ApplyStatusToEntityCardEvent(target, CreateStatus(cardMono, previewMode))
            };
        }

        public override List<AbstractCardEvent> Activate(CardPlayContext context)
        {
            AbstractEntity resolvedTarget = target;
            if (resolvedTarget == null && context?.Targets != null)
                context.Targets.TryGetFirstEntity(out resolvedTarget);

            return new List<AbstractCardEvent>
            {
                new ApplyStatusToEntityCardEvent(resolvedTarget, CreateStatus(context))
            };
        }

        public override string GetText()
        {
            return "Apply " + amount + " " + StatusIcon() + " to target";
        }

        public override string GetText(CardActionPreview preview)
        {
            int finalAmount = preview.GetTotalFinalValue(CardPreviewKeys.StatusAmount, amount);
            return "Apply " + preview.FormatValue(StatusIcon(), amount, finalAmount) + " to target";
        }

        public override string ToSimpleText()
        {
            return amount + " " + StatusIcon();
        }

        public override List<RectTransform> UpdateGraphic(GameObject diagram, GameObject tilePrefab, GameObject arrowPrefab)
        {
            return new List<RectTransform>();
        }

        public override string ToString()
        {
            return "Apply " + statusType + " " + amount;
        }

        private AbstractStatus CreateStatus(CardMonobehaviour cardMono, bool previewMode)
        {
            RandomState statusRandom = GetStableActionRandom(cardMono, previewMode, "status");

            switch (statusType)
            {
                case StatusApplicationType.Dazed:
                case StatusApplicationType.Daze:
                    return new DazedStatus(amount, statusRandom);
                case StatusApplicationType.Frost:
                    return new FrostStatus(amount);
                case StatusApplicationType.Frozen:
                    return new FrozenStatus(amount);
                case StatusApplicationType.Poison:
                    return new PoisonStatus(amount);
                case StatusApplicationType.Restless:
                    return new RestlessStatus(amount);
                case StatusApplicationType.Fall:
                    return new FallStatus(amount);
                case StatusApplicationType.Shocked:
                    return new ShockedStatus(amount);
                case StatusApplicationType.Energetic:
                    return new EnergeticStatus(amount);
                case StatusApplicationType.Excited:
                    return new ExcitedStatus(amount);
                case StatusApplicationType.Sleepy:
                    return new SleepyStatus(amount);
                case StatusApplicationType.Ranged:
                    return new RangedStatus(amount);
                case StatusApplicationType.Haste:
                    return new HasteStatus(amount);
                case StatusApplicationType.Blind:
                    return new BlindStatus(amount, statusRandom);
                case StatusApplicationType.Volatile:
                    return new VolatileStatus(amount);
                case StatusApplicationType.Dizzy:
                    return new DizzyStatus(amount, statusRandom);
                case StatusApplicationType.ShieldCarryover:
                    return new ShieldCarryoverStatus(amount);
                case StatusApplicationType.Buffed:
                default:
                    return new BuffedStatus(amount);
            }
        }

        private AbstractStatus CreateStatus(CardPlayContext context)
        {
            RandomState statusRandom = GetStableActionRandom(context, "status");

            switch (statusType)
            {
                case StatusApplicationType.Dazed:
                case StatusApplicationType.Daze:
                    return new DazedStatus(amount, statusRandom);
                case StatusApplicationType.Frost:
                    return new FrostStatus(amount);
                case StatusApplicationType.Frozen:
                    return new FrozenStatus(amount);
                case StatusApplicationType.Poison:
                    return new PoisonStatus(amount);
                case StatusApplicationType.Restless:
                    return new RestlessStatus(amount);
                case StatusApplicationType.Fall:
                    return new FallStatus(amount);
                case StatusApplicationType.Shocked:
                    return new ShockedStatus(amount);
                case StatusApplicationType.Energetic:
                    return new EnergeticStatus(amount);
                case StatusApplicationType.Excited:
                    return new ExcitedStatus(amount);
                case StatusApplicationType.Sleepy:
                    return new SleepyStatus(amount);
                case StatusApplicationType.Ranged:
                    return new RangedStatus(amount);
                case StatusApplicationType.Haste:
                    return new HasteStatus(amount);
                case StatusApplicationType.Blind:
                    return new BlindStatus(amount, statusRandom);
                case StatusApplicationType.Volatile:
                    return new VolatileStatus(amount);
                case StatusApplicationType.Dizzy:
                    return new DizzyStatus(amount, statusRandom);
                case StatusApplicationType.ShieldCarryover:
                    return new ShieldCarryoverStatus(amount);
                case StatusApplicationType.Buffed:
                default:
                    return new BuffedStatus(amount);
            }
        }

        private string StatusIcon()
        {
            switch (statusType)
            {
                case StatusApplicationType.Dazed:
                case StatusApplicationType.Daze:
                    return "<sprite name=\"dazed\">";
                case StatusApplicationType.Frost:
                case StatusApplicationType.Frozen:
                    return "<sprite name=\"snowflake\">";
                case StatusApplicationType.Poison:
                    return "<sprite name=\"droplets\">";
                case StatusApplicationType.Restless:
                    return "<sprite name=\"footsteps\">";
                case StatusApplicationType.Fall:
                    return "<sprite name=\"damage4\">";
                case StatusApplicationType.Shocked:
                    return "<sprite name=\"energyicon\">";
                case StatusApplicationType.Energetic:
                    return "<sprite name=\"energyicon\">";
                case StatusApplicationType.Excited:
                    return "<sprite name=\"drawcard\">";
                case StatusApplicationType.Sleepy:
                    return "<sprite name=\"dazed\">";
                case StatusApplicationType.Ranged:
                    return "<sprite name=\"arrow\">";
                case StatusApplicationType.Haste:
                    return "<sprite name=\"footsteps\">";
                case StatusApplicationType.Blind:
                    return "<sprite name=\"dazed\">";
                case StatusApplicationType.Volatile:
                    return "<sprite name=\"damage4\">";
                case StatusApplicationType.Dizzy:
                    return "<sprite name=\"dazed\">";
                case StatusApplicationType.ShieldCarryover:
                    return "<sprite name=\"shield\">";
                case StatusApplicationType.Buffed:
                default:
                    return "<sprite name=\"buffenemies\">";
            }
        }
    }
}
