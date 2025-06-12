using UnityEngine;

namespace Cards
{
    public class CardRarityColors
    {
        public static Color GetColor(CardRarity rarity) => rarity switch
        {
            CardRarity.Common => new Color(11/255.0f, 48/255.0f, 77/255.0f),
            CardRarity.Uncommon => new Color(11/255.0f, 77/255.0f, 53/255.0f),
            CardRarity.Rare => new Color(70/255.0f, 11/255.0f, 77/255.0f),
            CardRarity.Epic => new Color(77/255.0f, 11/255.0f, 38/255.0f),
            CardRarity.Legendary => new Color(77/255.0f, 44/255.0f, 11/255.0f),
            CardRarity.Mythic => Color.red,
            _ => Color.black
        };
    }
}