using System;
using System.Collections.Generic;
using Cards.CardEvents;
using StateManager;

namespace Types.CardModifiers.Modifiers
{
    public class GainShieldModifier : AbstractCardModifier
    {
        private int amount;
        private bool basedOnDamage;
        private float damageToShieldMultiplier;
        
        public GainShieldModifier(int amount = 3, bool basedOnDamage = false, float damageToShieldMultiplier = 0.5f)
        {
            this.amount = amount;
            this.basedOnDamage = basedOnDamage;
            if (basedOnDamage)
            {
                this.ModifierText = "Gain " + amount + " Shield";
            }
            else
            {
                this.ModifierText = "Gain " + (damageToShieldMultiplier * 100) + "% of attacks done as <shield>";
            }
        }
        
        public override List<AbstractCardEvent> Modify(List<AbstractCardEvent> cardEvents)
        {
            if (basedOnDamage)
            {
                int damageDone = 0;
                foreach (AbstractCardEvent cardEvent in cardEvents)
                {
                    if (cardEvent is AttackCardEvent attackCardEvent)
                    {
                        damageDone++;
                    }
                }
                
                cardEvents.Add(new ShieldCardEvent((int) (damageDone * damageToShieldMultiplier)));
            }
            else
            {
                cardEvents.Add(new ShieldCardEvent(this.amount));
            }
            return cardEvents;
        }
    }
}