using System;
using System.Collections.Generic;
using System.Linq;
using Cards.Actions;
using Types;
using Types.Passives;

namespace Cards.CardList
{
    public class CardData
    {
        private static readonly IReadOnlyDictionary<string, Func<CardEntry>> defs =
            new Dictionary<string, Func<CardEntry>>
            {
                // Developer
                ["DeveloperAttack"] = () => new(new Card("Developer Attack", new List<AbstractAction>
                { 
                    new AttackAllAction(0, "basic", null, 500),
                }, Rarity.Developer, CardSet.Developer),
                new [] { new StartingDeckEntry(StartingDecks.basic, 0) }, false),
                

                ["bstarting1"] = () => new(new Card("Starting Attack", new List<AbstractAction>
                    { 
                        new AttackAction(1, "basic", null, "", 1, 8),
                    }, Rarity.Common, CardSet.Base),
                    new [] { new StartingDeckEntry(StartingDecks.basic, 2) }, false),
                ["bstarting2"] = () => new(new Card("Starting Defend", new List<AbstractAction>
                    { 
                        new ShieldAction(1, "basic", null, 8),
                    }, Rarity.Common, CardSet.Base),
                    new [] { new StartingDeckEntry(StartingDecks.basic, 2) }, false),
                ["bstarting3"] = () => new(new Card("Starting Move", new List<AbstractAction>
                    { 
                        new GainStepsCardAction(1, "basic", null, 1),
                    }, Rarity.Common, CardSet.Base),
                    new [] { new StartingDeckEntry(StartingDecks.basic, 2) }, false),
                
                ["1"] = () => new(new Card("Smash", new List<AbstractAction>
                    { 
                        new AttackAction(1, "basic", null, "", 1, 12),
                    }, Rarity.Common, CardSet.Base)),
                ["2"] = () => new(new Card("Lance", new List<AbstractAction>
                    { 
                        new AttackAction(1, "basic", null, "", 2, 10),
                    }, Rarity.Uncommon, CardSet.Base)),
                ["3"] = () => new(new Card("Explosive", new List<AbstractAction>
                    { 
                        new AttackAllAction(1, "basic", null, 4),
                    }, Rarity.Uncommon, CardSet.Base)),
                ["4"] = () => new(new Card("Swipe", new List<AbstractAction>
                { 
                    new AttackRadiusAction(1, "basic", null, 1, 6),
                }, Rarity.Common, CardSet.Base)),
                
                ["5"] = () => new(new Card("Suit Up", new List<AbstractAction>
                { 
                    new ShieldAction(1, "basic", null, 3),
                    new ShieldAction(1, "basic", null, 3),
                    new ShieldAction(1, "basic", null, 3),
                }, Rarity.Common, CardSet.Base)),
                ["6"] = () => new(new Card("Block", new List<AbstractAction>
                    { 
                        new ShieldAction(1, "basic", null, 12)
                    }, Rarity.Common, CardSet.Base)),
                ["7"] = () => new(new Card("Prepare", new List<AbstractAction>
                { 
                    new DelayedShieldAction(1, "basic", null, 20)
                }, Rarity.Uncommon, CardSet.Base)),
                // ["8"] = () => new(new Card("Suit Up", new List<AbstractAction>
                //     { 
                //         new DelayedShieldAction(1, "basic", null, 20)
                //     }, Rarity.Common, CardSet.Base)),
                // ["9"] = () => new(new Card("Suit Up", new List<AbstractAction>
                //     { 
                //         new DelayedShieldAction(1, "basic", null, 20)
                //     }, Rarity.Common, CardSet.Base)),
                
                
                ["10"] = () => new(new Card("Move", new List<AbstractAction>
                    { 
                        new GainStepsCardAction(1, "basic", null, 1)
                    }, Rarity.Common, CardSet.Base)),
                // ["11"] = () => new(new Card("Move", new List<AbstractAction>
                    // { 
                    //     new GainStepsCardAction(1, "basic", null, 1)
                    // }, Rarity.Common, CardSet.Base)),
                
                ["12"] = () => new(new Card("Spawn Passive", new List<AbstractAction>
                    { 
                        new SpawnPassiveAction(1, "basic", null, "12"),
                        new ScrapCurrentCardAction(1, "basic", null, visible:false),
                    }, Rarity.Common, CardSet.Base)),
                
                ["14"] = () => new(new Card("Spawn Passive", new List<AbstractAction>
                    { 
                        new SpawnPassiveAction(1, "basic", null, "14"),
                        new ScrapCurrentCardAction(1, "basic", null, visible:false),
                    }, Rarity.Uncommon, CardSet.Base)),
                ["15"] = () => new(new Card("Spawn Passive", new List<AbstractAction>
                    { 
                        new SpawnPassiveAction(1, "basic", null, "15"),
                        new ScrapCurrentCardAction(1, "basic", null, visible:false),
                    }, Rarity.Uncommon, CardSet.Base)),
                
                
                ["27"] = () => new(new Card("Quick Draw", new List<AbstractAction>
                { 
                    new RaiseCostAction(0, "basic", null),
                    new DrawCardAction(0, "basic", null, 3),
                }, Rarity.Common, CardSet.Base)),
                
            };
        
        public static CardEntry Get(string id) => defs[id]();

        public static IEnumerable<string> AllIds => defs.Keys;

        public static IEnumerable<string> GetIdsFor(StartingDecks deck) =>
            defs
                .Where(kv =>
                    kv.Value().StartingDecks?.Any(sd => sd.startingDeck == deck) == true
                )
                .Select(kv => kv.Key);


    public static List<Card> GetStarter(StartingDecks deck) =>
        defs.Values
            .Select(def => def())
            .Where(entry => entry.StartingDecks?.Any(sd => sd.startingDeck == deck) == true)
            .SelectMany(entry =>
                entry.StartingDecks!
                    .Where(sd => sd.startingDeck == deck)
                    .SelectMany(sd =>
                        Enumerable.Range(0, sd.numberOfCards)
                            .Select(_ => new Card(entry.LocalCard))
                    )
            )
            .ToList();
        
        public static List<Card> GetCardsByRarity(Rarity rarity) =>
            defs.Values
                .Where(entry => entry().LocalCard.Rarity == rarity)
                .Select(entry => entry().LocalCard)
                .ToList();
        public static List<Card> GetShopCards() =>
            defs.Values
                .Select(def => def())
                .Where(entry => entry.ShowUpInShop)
                .Select(entry => entry.LocalCard)
                .ToList();
    }
    
}