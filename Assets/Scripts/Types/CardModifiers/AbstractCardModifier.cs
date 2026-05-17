using System;
using System.Collections.Generic;
using Cards.CardEvents;
using UnityEngine;

namespace Types.CardModifiers
{
    public abstract class AbstractCardModifier
    {
        public static RandomState guidRandom = RunInfo.NewRandom("cmguid");
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ResetStaticsOnLoad()
        {
            ResetStatics();
        }

        public static void ResetStatics()
        {
            guidRandom = RunInfo.NewRandom("cmguid");
        }
        public static string GenerateDeterministicId()
        {
            byte[] bytes = new byte[16];
            guidRandom.NextBytes(bytes);
            return new Guid(bytes).ToString();
        }
        
        protected RandomState cardModifierRandom = RunInfo.NewRandom(GenerateDeterministicId());

        
        public string ModifierText;
        public Rarity Rarity;
        public abstract List<AbstractCardEvent> Modify(List<AbstractCardEvent> cardEvent);

        public virtual List<AbstractCardEvent> Modify(List<AbstractCardEvent> cardEvent, bool previewMode)
        {
            return Modify(cardEvent);
        }

        protected RandomState GetModifierRandom(bool previewMode)
        {
            return previewMode ? cardModifierRandom.Clone() : cardModifierRandom;
        }
    }
}
