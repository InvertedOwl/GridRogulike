using System;
using System.Collections.Generic;
using System.Linq;
using Cards;
using Cards.CardEvents;
using UnityEngine;

namespace Types.Tiles
{
    public class TileData
    {
        public static readonly Dictionary<string, TileEntry> tiles = new Dictionary<string, TileEntry>()
        {
            ["basic"] = new ( 
                "Basic",
                "No effect.",
                new Color(32.0f/255.0f, 99.0f/255.0f, 155.0f/255.0f),
                true,
                Rarity.Common,
                TileType.Good,
                (e) => e,
                () => new List<AbstractCardEvent>()),
            
            ["start"] = new (
                "Start", 
                "The starting tile.", 
                new Color(173.0f/255.0f,173.0f/255.0f,173.0f/255.0f), 
                false, 
                Rarity.Common, 
                TileType.Good,
                (e) => e,
                () => new List<AbstractCardEvent>()),
            
            ["wall"] = new (
                "Wall", 
                "An impassible tile.", 
                new Color(23.0f/255.0f, 63.0f/255.0f, 95.0f/255.0f), 
                false, 
                Rarity.Common, 
                TileType.Good,
                (e) => e,
                () => new List<AbstractCardEvent>()),
            
            ["draw"] = new (
                "Draw", 
                "Draw a card.", 
                new Color(191.0f/255.0f, 51.0f/255.0f, 195.0f/255.0f), 
                true, 
                Rarity.Common,
                TileType.Good,
                (e) => e,
                () => { return new List<AbstractCardEvent> { new DrawCardEvent(1) }; }
                ), 
            
            ["money"] = new (
                "Money", 
                "Gain $2.", 
                new Color(252.0f/255.0f, 168.0f/255.0f, 3.0f/255.0f), 
                true, 
                Rarity.Common,
                TileType.Good,
                (e) => e,
                () => new List<AbstractCardEvent>{new GainMoneyCardEvent(2)}),
            
            ["double"] = new (
                "Double", 
                "Double damage when attacking from this tile.", 
                new Color(235.0f/255.0f, 124.0f/255.0f, 28.0f/255.0f), 
                true, 
                Rarity.Common,
                TileType.Good,
                (e) =>
                {
                    foreach (AbstractCardEvent cardEvent in e)
                    {
                        if (cardEvent is AttackCardEvent)
                        {
                            ((AttackCardEvent)cardEvent).amount *= 2;
                        }
                    }

                    return e;
                },
                () => new List<AbstractCardEvent>()),
            
            
            ["example_bad"] = new (
                "Example Bad", 
                "Example bad", 
                new Color(235.0f/255.0f, 20.0f/255.0f, 28.0f/255.0f), 
                true, 
                Rarity.Common,
                TileType.Good,
                (e) => e,
                () =>
                {
                    throw new NotImplementedException("Need to implement example_bad");
                }),
        };
        
        public static IEnumerable<TileEntry> GetTilesByType(TileType tileType)
        {
            return tiles.Values.Where(tile => tile.tileType == tileType);
        }

    }
}