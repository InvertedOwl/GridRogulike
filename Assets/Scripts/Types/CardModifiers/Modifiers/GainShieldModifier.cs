using System;
using System.Collections.Generic;
using Cards.CardEvents;
using UnityEngine;

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
            this.damageToShieldMultiplier = damageToShieldMultiplier;
            if (basedOnDamage)
            {
                this.ModifierText = "Gain " + (damageToShieldMultiplier * 100) + "% of attack damage as <shield>";
            }
            else
            {
                this.ModifierText = "Gain " + amount + " Shield";
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
                        damageDone += attackCardEvent.amount;
                    }
                }
                
                cardEvents.Add(new ShieldCardEvent(Mathf.RoundToInt(damageDone * damageToShieldMultiplier)));
            }
            else
            {
                cardEvents.Add(new ShieldCardEvent(this.amount));
            }
            return cardEvents;
        }
    }
}
