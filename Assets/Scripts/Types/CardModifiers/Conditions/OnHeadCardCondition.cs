using Cards;
using UnityEngine;

namespace Types.CardModifiers.Conditions
{
    public class OnHeadCardCondition: AbstractCardCondition
    {
        public OnHeadCardCondition()
        {
            this.ConditionText = "On Heads: ";
        }
        
        public override bool Condition(Card card)
        {
            // TODO: Create coinflip animation
            
            // TODO: THis is broken as fuck (it gets called every frame so it changes every frame)
            return cardConditionRandom.Next(0, 2) == 0;
        }
    }
}