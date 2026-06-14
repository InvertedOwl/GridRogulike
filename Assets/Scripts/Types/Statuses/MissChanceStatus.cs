using System.Collections.Generic;
using System.Linq;
using Cards.CardEvents;
using UnityEngine;

namespace Types.Statuses
{
    public abstract class MissChanceStatus : AbstractStatus
    {
        private readonly RandomState _random;

        protected MissChanceStatus(int amount, RandomState random, string randomKey)
        {
            Amount = Mathf.Max(1, amount);
            _random = random ?? RunInfo.NewRandom(randomKey);
        }

        protected abstract int MissChancePercent { get; }

        public override List<AbstractCardEvent> Modify(List<AbstractCardEvent> cardEvents)
        {
            return Modify(cardEvents, false);
        }

        public override List<AbstractCardEvent> Modify(List<AbstractCardEvent> cardEvents, bool previewMode)
        {
            if (previewMode || Amount <= 0)
                return cardEvents;

            List<AbstractCardEvent> modifiedEvents = new List<AbstractCardEvent>(cardEvents);
            int missChance = Mathf.Clamp(MissChancePercent, 0, 100);
            foreach (AttackCardEvent attackCardEvent in cardEvents.OfType<AttackCardEvent>())
            {
                if (_random.Next(100) < missChance)
                    modifiedEvents.Remove(attackCardEvent);
            }

            return modifiedEvents;
        }

        public override void OnEndTurn()
        {
        }
    }
}
