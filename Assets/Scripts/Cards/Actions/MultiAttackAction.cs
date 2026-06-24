using System;
using System.Collections.Generic;
using Cards.CardEvents;
using Entities;

namespace Cards.Actions
{
    public class MultiAttackAction : AttackAction
    {
        public MultiAttackAction(int baseCost, string color, AbstractEntity entity, int amount) : base(baseCost, color, entity, amount) { }

        public override List<AbstractCardEvent> Activate(CardMonobehaviour cardMono)
        {
            throw new NotImplementedException("Multi attack is not implemented");
        }
    }
}
