using Types.CardModifiers;
using UnityEngine;

namespace Passives
{
    public class PassiveEntry
    {
		public string Name;
        public string Desc;
        public AbstractCardCondition Condition;
		public AbstractCardModifier CardModifier;
        public Color Color;

        public PassiveEntry(string name, string desc, AbstractCardCondition condition, AbstractCardModifier cardModifier, Color color)
        {
            this.Name = name;
            this.Desc = desc;
            this.Condition = condition;
            this.CardModifier = cardModifier;
            this.Color = color;
        }
    }
}