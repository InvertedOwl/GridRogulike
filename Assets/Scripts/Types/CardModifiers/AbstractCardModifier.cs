using System;
using System.Collections.Generic;
using Cards.CardEvents;

namespace Types.CardModifiers
{
    public abstract class AbstractCardModifier
    {
        public static RandomState guidRandom = RunInfo.NewRandom("cmguid".GetHashCode());
        public static string GenerateDeterministicId()
        {
            byte[] bytes = new byte[16];
            guidRandom.NextBytes(bytes);
            return new Guid(bytes).ToString();
        }
        
        protected RandomState cardModifierRandom = RunInfo.NewRandom(GenerateDeterministicId().GetHashCode());

        
        public string ModifierText;
        public Rarity Rarity;
        public abstract List<AbstractCardEvent> Modify(List<AbstractCardEvent> cardEvent);
    }
}