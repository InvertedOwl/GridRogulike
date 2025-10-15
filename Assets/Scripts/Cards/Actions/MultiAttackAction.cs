using System;
using System.Collections.Generic;
using Entities;
using Grid;
using Types.CardEvents;

namespace Cards.Actions
{
    public class MultiAttackAction : AttackAction
    {
        public MultiAttackAction(int baseCost, string color, AbstractEntity entity, string direction, int distance, int _amount) : base(baseCost, color, entity, direction, distance, _amount) { }


        public override List<AbstractCardEvent> Activate()
        {
            throw new NotImplementedException("Multi attack is not implemented");
            return base.Activate();
        }
    }
}