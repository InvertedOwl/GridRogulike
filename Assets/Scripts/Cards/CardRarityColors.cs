using UnityEngine;

namespace Cards
{
    public class CardRarityColors
    {
        public static Color GetColor(Rarity rarity) => rarity switch
        {
            Rarity.Common => new Color(51/255.0f, 98/255.0f, 127/255.0f),
            Rarity.Uncommon => new Color(51/255.0f, 127/255.0f, 53/255.0f),
            Rarity.Rare => new Color(120/255.0f, 61/255.0f, 127/255.0f),
            Rarity.Epic => new Color(127/255.0f, 61/255.0f, 58/255.0f),
            Rarity.Legendary => new Color(127/255.0f, 84/255.0f, 61/255.0f),
            Rarity.Mythic => Color.red,
            _ => Color.black
        };
    }
}