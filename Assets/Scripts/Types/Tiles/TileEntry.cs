using System;
using System.Collections.Generic;
using Cards;
using Cards.CardEvents;
using UnityEngine;

namespace Types.Tiles
{
    public enum TileTriggerLimit
    {
        None,
        OncePerTurn,
        OncePerCombat,
    }
    
    public enum TriggerEventTime
    {
        Land,
        StartTurn,
        EndTurn,
    }

    public class TileEntry
    {
        public Dictionary<TriggerEventTime, Func<List<AbstractCardEvent>>> triggerEvents;
        public Func<List<AbstractCardEvent>, Card, List<AbstractCardEvent>> cardModifier;
        public string name;
        public string description;
        public Color color;
        public bool canAppearInShop;
        public Rarity rarity;
        public TileType tileType;
        public string icon;
        public TileTriggerLimit triggerLimit;
        public Func<List<AbstractCardEvent>, Card, bool> shouldMarkAsTriggered;

        public TileEntry(string name, string description, Color color, bool canAppearInShop, Rarity rarity, TileType tileType, 
            Func<List<AbstractCardEvent>, Card, List<AbstractCardEvent>> cardModifier, 
            Dictionary<TriggerEventTime, Func<List<AbstractCardEvent>>> triggerEvents, 
            string icon = "none", 
            TileTriggerLimit triggerLimit = TileTriggerLimit.None,
            Func<List<AbstractCardEvent>, Card, bool> shouldMarkAsTriggered = null)
        {
            this.name = name;
            this.description = description;
            this.color = color;
            this.canAppearInShop = canAppearInShop;
            this.rarity = rarity;
            this.tileType = tileType;
            this.triggerEvents = triggerEvents;
            this.cardModifier = cardModifier;
            this.icon = icon;
            this.triggerLimit = triggerLimit;
            this.shouldMarkAsTriggered = shouldMarkAsTriggered;
        }

    }
}
