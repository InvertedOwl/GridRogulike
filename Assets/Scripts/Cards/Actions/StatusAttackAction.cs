using System.Collections.Generic;
using Cards.CardEvents;
using Entities;
using Types.Statuses;
using UnityEngine;

namespace Cards.Actions
{
    public class StatusAttackAction : AttackAction
    {
        public StatusApplicationType statusType;
        public int statusAmount;

        public StatusAttackAction(
            int baseCost,
            string color,
            AbstractEntity entity,
            string direction,
            int distance,
            int amount,
            StatusApplicationType statusType,
            int statusAmount) : base(baseCost, color, entity, direction, distance, amount)
        {
            this.statusType = statusType;
            this.statusAmount = statusAmount;
        }

        public override List<AbstractCardEvent> Activate(CardMonobehaviour cardMono)
        {
            return Activate(cardMono, previewMode: false);
        }

        public override List<AbstractCardEvent> Activate(CardMonobehaviour cardMono, bool previewMode)
        {
            return new List<AbstractCardEvent>
            {
                new AttackCardEvent(_distance, _direction, _amount, CreateStatus(previewMode))
            };
        }

        public override string GetText()
        {
            return Amount + " <attack><pos=60%>" + statusAmount + " " + StatusIcon();
        }

        public override string GetText(CardActionPreview preview)
        {
            int finalAmount = preview.GetFirstFinalValue(CardPreviewKeys.Damage, Amount);
            int finalStatus = preview.GetFirstFinalValue(CardPreviewKeys.StatusAmount, statusAmount);
            return preview.FormatValue("<attack>", Amount, finalAmount) +
                   "<pos=60%>" +
                   preview.FormatValue(StatusIcon(), statusAmount, finalStatus);
        }

        private AbstractStatus CreateStatus(bool previewMode)
        {
            RandomState statusRandom = GetActionRandom(previewMode);

            switch (statusType)
            {
                case StatusApplicationType.Dazed:
                case StatusApplicationType.Daze:
                    return new DazedStatus(statusAmount, statusRandom);
                case StatusApplicationType.Frost:
                    return new FrostStatus(statusAmount);
                case StatusApplicationType.Frozen:
                    return new FrozenStatus(statusAmount);
                case StatusApplicationType.Poison:
                    return new PoisonStatus(statusAmount);
                case StatusApplicationType.Restless:
                    return new RestlessStatus(statusAmount);
                case StatusApplicationType.Fall:
                    return new FallStatus(statusAmount);
                case StatusApplicationType.Shocked:
                    return new ShockedStatus(statusAmount);
                case StatusApplicationType.Energetic:
                    return new EnergeticStatus(statusAmount);
                case StatusApplicationType.Excited:
                    return new ExcitedStatus(statusAmount);
                case StatusApplicationType.Sleepy:
                    return new SleepyStatus(statusAmount);
                case StatusApplicationType.Ranged:
                    return new RangedStatus(statusAmount);
                case StatusApplicationType.Haste:
                    return new HasteStatus(statusAmount);
                case StatusApplicationType.Blind:
                    return new BlindStatus(statusAmount, statusRandom);
                case StatusApplicationType.Volatile:
                    return new VolatileStatus(statusAmount);
                case StatusApplicationType.Dizzy:
                    return new DizzyStatus(statusAmount, statusRandom);
                case StatusApplicationType.ShieldCarryover:
                    return new ShieldCarryoverStatus(statusAmount);
                case StatusApplicationType.Buffed:
                default:
                    return new BuffedStatus(statusAmount);
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
                case StatusApplicationType.Energetic:
                    return "<sprite name=\"energyicon\">";
                case StatusApplicationType.Excited:
                    return "<sprite name=\"drawcard\">";
                case StatusApplicationType.Sleepy:
                case StatusApplicationType.Blind:
                case StatusApplicationType.Dizzy:
                    return "<sprite name=\"dazed\">";
                case StatusApplicationType.Ranged:
                    return "<sprite name=\"arrow\">";
                case StatusApplicationType.Haste:
                    return "<sprite name=\"footsteps\">";
                case StatusApplicationType.Volatile:
                    return "<sprite name=\"damage4\">";
                case StatusApplicationType.ShieldCarryover:
                    return "<sprite name=\"shield\">";
                case StatusApplicationType.Buffed:
                default:
                    return "<sprite name=\"buffenemies\">";
            }
        }
    }
}
