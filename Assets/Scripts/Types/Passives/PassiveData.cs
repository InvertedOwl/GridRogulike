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
                ["Shielded Strikes"] = () => new("Shielded Strikes", "Every time you attack, gain 4 shield.",
                    new AttackingCondition(), new GainShieldModifier(4), new Color(0.0196f, 0.2588f, 0.0275f)),

            };
        
        public static PassiveEntry GetPassiveEntry(string name)
        {
            return defs[name]();
        }
    }
}