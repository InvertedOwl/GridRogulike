using System;
using System.Collections.Generic;
using System.Linq;
using Cards;
using UnityEngine;

namespace Types.Tiles
{
    public class TileData
    {
        public static readonly Dictionary<string, TileEntry> tiles = new Dictionary<string, TileEntry>()
        {
            ["basic"] = new (() => {}, 
                "Basic",
                "No effect.",
                new Color(32.0f/255.0f, 99.0f/255.0f, 155.0f/255.0f),
                true,
                Rarity.Common,
                TileType.Good),
            
            ["start"] = new (() => {}, 
                "Start", 
                "The starting tile.", 
                new Color(173.0f/255.0f,173.0f/255.0f,173.0f/255.0f), 
                false, 
                Rarity.Common, 
                TileType.Good),
            
            ["wall"] = new (() => {}, 
                "Wall", 
                "An impassible tile.", 
                new Color(23.0f/255.0f, 63.0f/255.0f, 95.0f/255.0f), 
                false, 
                Rarity.Common, 
                TileType.Good),
            
            ["draw"] = new (
                () => { Deck.Instance.FullDrawHand(1); }, 
                "Draw", 
                "When landing on this tile, draw a card.", 
                new Color(191.0f/255.0f, 51.0f/255.0f, 195.0f/255.0f), 
                true, 
                Rarity.Common,
                TileType.Good),
            
            ["money"] = new (
                () => { RunInfo.Instance.Money += 1;}, 
                "Money", 
                "When landing on this tile, gain 1$.", 
                new Color(252.0f/255.0f, 168.0f/255.0f, 3.0f/255.0f), 
                true, 
                Rarity.Common,
                TileType.Good),
            
            ["double"] = new (
                () => { throw new NotImplementedException("Need to implement double damage"); }, 
                "Double", 
                "20% Chance to double damage dealt on this tile.", 
                new Color(235.0f/255.0f, 124.0f/255.0f, 28.0f/255.0f), 
                true, 
                Rarity.Common,
                TileType.Good),
            
            
            ["example_bad"] = new (
                () => { throw new NotImplementedException("Need to implement example_bad"); }, 
                "Example Bad", 
                "Example bad", 
                new Color(235.0f/255.0f, 20.0f/255.0f, 28.0f/255.0f), 
                true, 
                Rarity.Common,
                TileType.Good),
        };
        
        public static IEnumerable<TileEntry> GetTilesByType(TileType tileType)
        {
            return tiles.Values.Where(tile => tile.tileType == tileType);
        }

    }
}