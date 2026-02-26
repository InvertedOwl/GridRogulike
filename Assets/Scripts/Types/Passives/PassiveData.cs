using System;
using System.Collections.Generic;
using Passives;
using Types.CardModifiers.Conditions;
using Types.CardModifiers.Modifiers;
using UnityEngine;

namespace Types.Passives
{
    public class PassiveData
    {
        private static readonly IReadOnlyDictionary<string, Func<PassiveEntry>> defs =
            new Dictionary<string, Func<PassiveEntry>>
            {
                ["forest"] = () => new("The Forest", "20% of all cards are played twice",
                    new PercentChanceCardCondition(20), new AgainCardModifier(), new Color(0.0196f, 0.2588f, 0.0275f)),
                ["bloodritual"] = () => new("The Blood Ritual", "1.5x All damage",
                    new PercentChanceCardCondition(100), new MoreDamageCardModifier(), new Color(0.2588f, 0.0392f, 0.0196f)),
                ["poisonswamp"] = () => new("Murky Swamp", "All attacks apply 1 poison",
                    new PercentChanceCardCondition(100), new PoisonCardModifier(), new Color(0.2627f, 0.3804f, 0.0431f)),
                ["noattack"] = () => new("Disarmed", "Half of attacks fail",
                    new PercentChanceCardCondition(50), new DisableAttacksCardModifier(), new Color(0.2627f, 0.3804f, 0.0431f)),
                ["support"] = () => new("Support", "For every attack done, gain 3 shield",
                    new AttackingCondition(), new GainShieldModifier(), new Color(0.2627f, 0.3804f, 0.0431f)),
                ["block"] = () => new("Block", "For every time shield gained, double that shield",
                    new ShieldingCondition(), new DoubleShieldModifier(), new Color(0.2627f, 0.3804f, 0.0431f)),
            };
        
        public static PassiveEntry GetPassiveEntry(string name)
        {
            return defs[name]();
        }
    }
}