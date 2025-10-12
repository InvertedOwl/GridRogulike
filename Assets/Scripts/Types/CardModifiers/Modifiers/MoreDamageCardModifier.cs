using System;
using Types.CardEvents;

namespace Types.CardModifiers
{
    public class MoreDamageCardModifier: AbstractCardModifier
    {
        public MoreDamageCardModifier()
        {
            this.ModifierText = "Damage is increased by 1.2 times";
            this.Rarity = Rarity.Common;
        }
        
        public override AbstractCardEvent Modify(AbstractCardEvent cardEvent)
        {
            if (cardEvent is AttackCardEvent)
            {
                AttackCardEvent attackCardEvent = (AttackCardEvent)cardEvent;
                attackCardEvent.amount = (int) MathF.Floor(attackCardEvent.amount * 1.2f);
            }

            return cardEvent;
        }
    }
}