using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Types.CardModifiers.Modifiers
{
    public static class CardModifiersData
    {
        public static List<CardModifiersEntry> CardModifiersEntries = new List<CardModifiersEntry>
        {
            new (typeof(DoNothingCardModifier), Rarity.Common),
            new (typeof(SpeedCardModifier), Rarity.Common),
            new (typeof(DisableAttacksCardModifier), Rarity.Common),
            new (typeof(PoisonCardModifier), Rarity.Common),
            new (typeof(ShieldCardModifier), Rarity.Common),
            
            new (typeof(HealCardModifier), Rarity.Uncommon),
            new (typeof(MoreDamageCardModifier), Rarity.Uncommon),
            new (typeof(GainEnergyCardModifier), Rarity.Uncommon),
            new (typeof(AgainCardModifier), Rarity.Uncommon),
            new (typeof(GainRandomCard), Rarity.Common),
            new (typeof(DrawCardModifier), Rarity.Uncommon),
            new (typeof(RandomMoveCardModifier), Rarity.Uncommon),
            
        };
        
        public static CardModifiersEntry GetRandomModifier(Rarity rarity)
        {
            var validModifiers = CardModifiersEntries
                .Where(modifier => modifier.Rarity.Equals(rarity))
                .ToList();

            if (validModifiers.Count == 0)
            {
                Debug.LogError($"No modifiers found for Rarity {rarity}");
                return null;
            }

            return validModifiers[Random.Range(0, validModifiers.Count)];
        }
    }
}