using System;
using System.Collections.Generic;
using Cards;
using Cards.CardEvents;
using UnityEngine;

namespace Types.Tiles
{
    public class TileEntry
    {
        public Func<List<AbstractCardEvent>> landEvent;
        public Func<List<AbstractCardEvent>, List<AbstractCardEvent>> cardModifier;
        public string name;
        public string description;
        public Color color;
        public bool canAppearInShop;
        public Rarity rarity;
        public TileType tileType;

        public TileEntry(string name, string description, Color color, bool canAppearInShop, Rarity rarity, TileType tileType, Func<List<AbstractCardEvent>, List<AbstractCardEvent>> cardModifier, Func<List<AbstractCardEvent>> landEvent)
        {
            this.name = name;
            this.description = description;
            this.color = color;
            this.canAppearInShop = canAppearInShop;
            this.rarity = rarity;
            this.tileType = tileType;
            this.landEvent = landEvent;
            this.cardModifier = cardModifier;
        }

    }
}