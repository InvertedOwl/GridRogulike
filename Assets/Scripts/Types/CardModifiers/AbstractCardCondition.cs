using System;
using Cards;
using Cards.CardEvents;

namespace Types.CardModifiers
{
    public abstract class AbstractCardCondition
    {
        public static Random guidRandom = RunInfo.NewRandom("ccguid".GetHashCode());
        public static string GenerateDeterministicId()
        {
            byte[] bytes = new byte[16];
            guidRandom.NextBytes(bytes);
            return new Guid(bytes).ToString();
        }
        
        protected Random cardConditionRandom = RunInfo.NewRandom(GenerateDeterministicId().GetHashCode());
        
        public string ConditionText;
        public abstract bool Condition(Card card);
    }
}