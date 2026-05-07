using System;
using System.Collections.Generic;
using Passives;
using Types.CardModifiers.Conditions;
using Types.CardModifiers.Modifiers;
using UnityEngine;
using Util;

namespace Types.Passives
{
    public class PassiveData
    {
        private static readonly IReadOnlyDictionary<string, Func<PassiveEntry>> defs =
            new Dictionary<string, Func<PassiveEntry>>
            {
                ["forest"] = () => new("The Forest", "20% of all cards are played twice",
                    new PercentChanceCardCondition(20), new AgainCardModifier(), new Color(0.0196f, 0.2588f, 0.0275f)),
                ["12"] = () => new("Roll", "Gain 4 <shield> when attacking",
                    new AttackingCondition(), new GainShieldModifier(4), HexColorUtility.HexToColor("#295eb3")),
                ["14"] = () => new("Light Roll", "Gain 50% <shield> of damage done in an attack",
                    new AttackingCondition(), new GainShieldModifier(basedOnDamage:true, damageToShieldMultiplier:.5f), HexColorUtility.HexToColor("#4d73db")),
                ["15"] = () => new("Sharpen Blade", "Attacks deal 3 additional damage",
                    new AttackingCondition(), new MoreDamageCardModifier(), HexColorUtility.HexToColor("#781860")),

            };
        
        public static PassiveEntry GetPassiveEntry(string name)
        {
            return defs[name]();
        }
    }
}