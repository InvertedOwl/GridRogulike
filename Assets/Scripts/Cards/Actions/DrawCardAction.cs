using Entities;
using Types.CardEvents;

namespace Cards.Actions
{
    public class DrawCardAction : AbstractAction
    {
        public DrawCardAction(int baseCost, string color, AbstractEntity entity) : base(baseCost, color, entity)
        {

        }

        public override AbstractCardEvent Activate()
        {
            return new DrawCardEvent();
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