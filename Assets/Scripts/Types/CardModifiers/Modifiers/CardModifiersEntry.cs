using System;

namespace Types.CardModifiers
{
    public class CardModifiersEntry
    {
        public Type ModifierType;
        public Rarity Rarity;

        public CardModifiersEntry(Type modifierType, Rarity rarity)
        {
            this.ModifierType = modifierType;
            this.Rarity = rarity;
        }
    }
}