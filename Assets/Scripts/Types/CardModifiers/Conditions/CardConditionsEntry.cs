using System;

namespace Types.CardModifiers.Conditions
{
    public class CardConditionsEntry
    {
        public Type ConditionType;

        public CardConditionsEntry(Type conditionType)
        {
            this.ConditionType = conditionType;
        }
    }
}