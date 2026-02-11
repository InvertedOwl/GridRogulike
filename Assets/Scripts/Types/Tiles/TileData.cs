using System;
using System.Collections.Generic;
using System.Linq;
using Cards;
using Cards.CardEvents;
using Types.Passives;
using UnityEngine;

namespace Types.Tiles
{
    public class TileData
    {
        public static readonly Dictionary<string, TileEntry> tiles = new Dictionary<string, TileEntry>()
        {
            ["steps"] = new ( 
                "Agile",
                "Gain 1 step",
                Color.deepSkyBlue,
                true,
                Rarity.Common,
                TileType.Good,
                (e) => e,
                () => new List<AbstractCardEvent>
                {
                    new AddStepsCardEvent(1)
                }),
            
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
            
            // Unused
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
                "Lucky Draw", 
                "Draw a card.", 
                new Color(191.0f/255.0f, 51.0f/255.0f, 195.0f/255.0f), 
                true, 
                Rarity.Common,
                TileType.Good,
                (e) => e,
                () => { return new List<AbstractCardEvent> { new DrawCardEvent(1) }; }
                ), 
            
            ["money"] = new (
                "Pocket Change", 
                "Gain $2.", 
                new Color(252.0f/255.0f, 168.0f/255.0f, 3.0f/255.0f), 
                true, 
                Rarity.Common,
                TileType.Good,
                (e) => e,
                () => new List<AbstractCardEvent>{new GainMoneyCardEvent(2)}),
            
            ["energy"] = new (
                "Battery", 
                "Gain 1 Energy.", 
                Color.gold, 
                true, 
                Rarity.Uncommon,
                TileType.Good,
                (e) => e,
                () => new List<AbstractCardEvent>{new GainEnergyCardEvent(1)}),
            
            ["shield"] = new (
                "Bunker", 
                "Gain 3 Shield.", 
                Color.darkBlue, 
                true, 
                Rarity.Common,
                TileType.Good,
                (e) => e,
                () => new List<AbstractCardEvent>{new ShieldCardEvent(3)}),
            
            ["double"] = new (
                "Empowered", 
                "1.5x damage", 
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
                            ((AttackCardEvent)cardEvent).amount = (int) (((AttackCardEvent)cardEvent).amount * 1.5f);
                        }
                    }

                    return e;
                },
                () => new List<AbstractCardEvent>()),
            ["passive"] = new (
                "Random Passive", // TODO: change name this is boring 
                "Enable a random passive.", 
                Color.navajoWhite, 
                true, 
                Rarity.Common,
                TileType.Good,
                (e) => e,
                () =>
                {
                    
                    return new List<AbstractCardEvent>()
                    {
                        // TODO: make this random
                        new SpawnPassiveEvent(PassiveData.GetPassiveEntry("forest"))
                    };
                }),
            
            // Unused
            ["example_bad"] = new (
                "Example Bad", 
                "Example bad", 
                new Color(235.0f/255.0f, 20.0f/255.0f, 28.0f/255.0f), 
                false, 
                Rarity.Common,
                TileType.Bad,
                (e) => e,
                () =>
                {
                    throw new NotImplementedException("Need to implement example_bad");
                }),
            
            ["scrapRandom"] = new (
                "Sabotage", 
                "Scrap a random card in your hand.", 
                new Color(235.0f/255.0f, 20.0f/255.0f, 28.0f/255.0f), 
                true, 
                Rarity.Common,
                TileType.Bad,
                (e) => e,
                () => new List<AbstractCardEvent>
                {
                    new ScrapCardEvent(Deck.Instance.GetRandomHandCardId())
                }),
            
        };
        
        public static IEnumerable<TileEntry> GetTilesByType(TileType tileType)
        {
            return tiles.Values.Where(tile => tile.tileType == tileType);
        }

    }
}