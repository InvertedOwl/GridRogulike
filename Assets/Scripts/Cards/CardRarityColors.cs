using UnityEngine;

namespace Cards
{
    public class CardRarityColors
    {
        public static Color GetColor(CardRarity rarity) => rarity switch
        {
            CardRarity.Common => new Color(51/255.0f, 98/255.0f, 127/255.0f),
            CardRarity.Uncommon => new Color(51/255.0f, 127/255.0f, 53/255.0f),
            CardRarity.Rare => new Color(120/255.0f, 61/255.0f, 127/255.0f),
            CardRarity.Epic => new Color(127/255.0f, 61/255.0f, 58/255.0f),
            CardRarity.Legendary => new Color(127/255.0f, 84/255.0f, 61/255.0f),
            CardRarity.Mythic => Color.red,
            _ => Color.black
        };
    }
}