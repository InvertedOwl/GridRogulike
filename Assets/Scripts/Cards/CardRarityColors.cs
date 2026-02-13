using UnityEngine;
using Types;

namespace Cards
{
    public class CardRarityColors
    {
        public static Color GetColor(Rarity rarity) => rarity switch
        {
            Rarity.Common => new Color(0.1529f, 0.4902f, 0.6314f),
            Rarity.Uncommon => new Color(0.3020f, 0.5647f, 0.5569f),
            Rarity.Rare => new Color(0.5647f * 0.8f, 0.7451f * 0.8f, 0.4275f * 0.8f),
            Rarity.Epic => new Color(0.9765f * 0.8f, 0.7804f * 0.8f, 0.3098f * 0.8f),
            Rarity.Legendary => new Color(0.9725f * 0.8f, 0.5882f * 0.8f, 0.1176f * 0.8f),
            Rarity.Mythic => new Color(0.9765f * 0.8f, 0.2549f * 0.8f, 0.2667f * 0.8f),
            _ => Color.black
        };
    }
}