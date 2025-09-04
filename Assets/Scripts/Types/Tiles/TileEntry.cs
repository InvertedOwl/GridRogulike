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
        public Rarity rarity;
        public TileType tileType;

        public TileEntry(Action landEvent, string name, string description, Color color, bool canAppearInShop, Rarity rarity, TileType tileType)
        {
            this.landEvent = landEvent;
            this.name = name;
            this.description = description;
            this.color = color;
            this.canAppearInShop = canAppearInShop;
            this.rarity = rarity;
            this.tileType = tileType;
        }

    }
}