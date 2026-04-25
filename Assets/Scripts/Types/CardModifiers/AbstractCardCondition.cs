using System;
using Cards;
using Cards.CardEvents;
using UnityEngine;

namespace Types.CardModifiers
{
    public abstract class AbstractCardCondition
    {
        public static RandomState guidRandom = RunInfo.NewRandom("ccguid");
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ResetStaticsOnLoad()
        {
            guidRandom = RunInfo.NewRandom("ccguid");
        }
        public static string GenerateDeterministicId()
        {
            byte[] bytes = new byte[16];
            guidRandom.NextBytes(bytes);
            return new Guid(bytes).ToString();
        }
        
        protected RandomState cardConditionRandom = RunInfo.NewRandom(GenerateDeterministicId());
        
        public string ConditionText;
        public abstract bool Condition(Card card);
    }
}
