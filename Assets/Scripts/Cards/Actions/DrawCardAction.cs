using System.Collections.Generic;
using Entities;
using Types.CardEvents;

namespace Cards.Actions
{
    public class DrawCardAction : AbstractAction
    {
        public DrawCardAction(int baseCost, string color, AbstractEntity entity) : base(baseCost, color, entity)
        {

        }

        public override List<AbstractCardEvent> Activate()
        {
            return new List<AbstractCardEvent> { new DrawCardEvent() };
        }

        public override void Hover()
        {

        }
        
        public override string ToString()
        {
            return "Draw Card ";
        }
    }
}