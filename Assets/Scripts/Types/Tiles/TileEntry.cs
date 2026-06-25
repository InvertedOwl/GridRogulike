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

    public class TileCountdownEffect
    {
        public int startTurns;
        public string[] countdownIcons;
        public string explosionIcon;
        public string inactiveIcon;
        public int explosionDamage;

        public TileCountdownEffect(
            int startTurns,
            string[] countdownIcons,
            string explosionIcon = "Damage4",
            string inactiveIcon = "none",
            int explosionDamage = 4)
        {
            this.startTurns = Mathf.Max(1, startTurns);
            this.countdownIcons = countdownIcons ?? Array.Empty<string>();
            this.explosionIcon = explosionIcon;
            this.inactiveIcon = inactiveIcon;
            this.explosionDamage = explosionDamage;
        }

        public string IconForTurnsRemaining(int turnsRemaining)
        {
            if (turnsRemaining <= 0)
                return explosionIcon;

            if (countdownIcons.Length == 0)
                return "none";

            int index = Mathf.Clamp(startTurns - turnsRemaining, 0, countdownIcons.Length - 1);
            return countdownIcons[index];
        }
    }

    public class TileEntry
    {
        public Dictionary<TriggerEventTime, Func<TileContext, List<AbstractCardEvent>>> triggerEvents;
        public Func<List<AbstractCardEvent>, Card, TileContext, List<AbstractCardEvent>> cardModifier;
        public Func<List<AbstractCardEvent>, TileContext, List<AbstractCardEvent>> incomingEventModifier;
        public string name;
        public string description;
        public TileSetEnum color;
        public bool canAppearInShop;
        public Rarity rarity;
        public TileType tileType;
        public string icon;
        public TileTriggerLimit triggerLimit;
        public Func<List<AbstractCardEvent>, Card, TileContext, bool> shouldMarkAsTriggered;
        public TileCountdownEffect countdownEffect;
        public PairedTilesEntry pairedTilesEntry;

        public TileEntry(string name, string description, TileSetEnum color, bool canAppearInShop, Rarity rarity, TileType tileType,
            Func<List<AbstractCardEvent>, Card, TileContext, List<AbstractCardEvent>> cardModifier,
            Dictionary<TriggerEventTime, Func<TileContext, List<AbstractCardEvent>>> triggerEvents,
            PairedTilesEntry pairedTilesEntry = null,
            string icon = "none", 
            TileTriggerLimit triggerLimit = TileTriggerLimit.None,
            Func<List<AbstractCardEvent>, Card, TileContext, bool> shouldMarkAsTriggered = null,
            TileCountdownEffect countdownEffect = null,
            Func<List<AbstractCardEvent>, TileContext, List<AbstractCardEvent>> incomingEventModifier = null)
        {
            this.name = name;
            this.description = description;
            this.color = color;
            this.canAppearInShop = canAppearInShop;
            this.rarity = rarity;
            this.tileType = tileType;
            this.triggerEvents = triggerEvents;
            this.cardModifier = cardModifier;
            this.incomingEventModifier = incomingEventModifier ?? ((events, context) => events);
            this.icon = icon;
            this.triggerLimit = triggerLimit;
            this.shouldMarkAsTriggered = shouldMarkAsTriggered;
            this.countdownEffect = countdownEffect;
            this.pairedTilesEntry = pairedTilesEntry;
        }

    }
}
