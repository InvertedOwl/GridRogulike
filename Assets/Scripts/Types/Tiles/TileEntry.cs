using System;
using Cards;
using UnityEngine;

namespace Types.Tiles
{
    public class TileEntry
    {
        public Action landEvent;
        public string name;
        public string description;
        public Color color;
        public bool canAppearInShop;
        public CardRarity rarity;

        public TileEntry(Action landEvent, string name, string description, Color color, bool canAppearInShop, CardRarity rarity)
        {
            this.landEvent = landEvent;
            this.name = name;
            this.description = description;
            this.color = color;
            this.canAppearInShop = canAppearInShop;
            this.rarity = rarity;
        }

    }
}