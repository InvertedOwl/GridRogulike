using System.Collections.Generic;
using UnityEngine;

namespace Types.CardModifiers.Conditions
{
    public static class CardConditionsData
    {
        public static List<CardConditionsEntry> CardConditionsEntries = new List<CardConditionsEntry>
        {
            // new (typeof(OnHeadCardCondition)), < Broken
            new (typeof(HalfHealthCardCondition)),
            new (typeof(TileCardCondition)),
            new (typeof(HighHealthCardCondition)),
            new (typeof(PoorCardCondition)),
            new (typeof(EnergeticCardCondition)),
            new (typeof(LoadedCardCondition)),
            new (typeof(ShieldedCardCondition)),
            new (typeof(StockedCardCondition)),
            new (typeof(AttackVerticalCardCondition)),
            new (typeof(AttackRightCardCondition)),
            new (typeof(AttackLeftCardCondition)),
            new (typeof(FreeCardCondition)),
            new (typeof(ExpensiveCardCondition)),
            new (typeof(MaxwellGotchCardCondition)),
        };
        
        public static CardConditionsEntry GetRandomCondition()
        {
            return CardConditionsEntries[Random.Range(0, CardConditionsEntries.Count)];
        }
    }
}