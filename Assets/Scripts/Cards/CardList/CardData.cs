using System;
using System.Collections.Generic;
using System.Linq;
using Cards.Actions;
using Types;

namespace Cards.CardList
{
    public class CardData
    {
        private static readonly IReadOnlyDictionary<string, Func<CardEntry>> defs =
            new Dictionary<string, Func<CardEntry>>
            {
                // -  Basic Deck starting hand  - 
                // attack cards
                ["AttackUp"] = () => new(new Card("Attack Up", new List<AbstractAction>
                { 
                    new AttackAction(1, "basic", null, "n", 1, 15)
                }, Rarity.Common),
                            new [] { StartingDecks.basic }),
                
                ["AttackUpRight"] = () => new(new Card("Attack Up Right", new List<AbstractAction>
                    { 
                        new AttackAction(1, "basic", null, "ne", 1, 15)
                    }, Rarity.Common), 
                    new [] {StartingDecks.basic}),
                
                ["AttackUpLeft"] = () => new(new Card("Attack Up Left", new List<AbstractAction>
                    { 
                        new AttackAction(1, "basic", null, "nw", 1, 15)
                    }, Rarity.Common), 
                    new [] {StartingDecks.basic}),
                
                ["AttackDown"] = () => new(new Card("Attack Down", new List<AbstractAction>
                    { 
                        new AttackAction(1, "basic", null, "s", 1, 15)
                    }, Rarity.Common), 
                    new [] {StartingDecks.basic}),
                
                ["AttackDownRight"] = () => new(new Card("Attack Down Right", new List<AbstractAction>
                    { 
                        new AttackAction(1, "basic", null, "se", 1, 15)
                    }, Rarity.Common), 
                    new [] {StartingDecks.basic}),
                
                ["AttackDownLeft"] = () => new(new Card("Attack Down Left", new List<AbstractAction>
                    { 
                        new AttackAction(1, "basic", null, "sw", 1, 15)
                    }, Rarity.Common), 
                    new [] {StartingDecks.basic}),
                // move cards
                ["MoveUp"] = () => new(new Card("Move Up", new List<AbstractAction>
                    { 
                        new MoveAction(1, "basic", null, "n", 1)
                    }, Rarity.Common), 
                    new [] {StartingDecks.basic}),
                
                ["MoveUpRight"] = () => new(new Card("Move Up Right", new List<AbstractAction>
                    { 
                        new MoveAction(1, "basic", null, "ne", 1)
                    }, Rarity.Common), 
                    new [] {StartingDecks.basic}),
                
                ["MoveUpLeft"] = () => new(new Card("Move Up Left", new List<AbstractAction>
                    { 
                        new MoveAction(1, "basic", null, "nw", 1)
                    }, Rarity.Common), 
                    new [] {StartingDecks.basic}),
                
                ["MoveDown"] = () => new(new Card("Move Down", new List<AbstractAction>
                    { 
                        new MoveAction(1, "basic", null, "s", 1)
                    }, Rarity.Common), 
                    new [] {StartingDecks.basic}),
                
                ["MoveDownRight"] = () => new(new Card("Move Down Right", new List<AbstractAction>
                    { 
                        new MoveAction(1, "basic", null, "se", 1)
                    }, Rarity.Common), 
                    new [] {StartingDecks.basic}),
                
                ["MoveDownLeft"] = () => new(new Card("Move Down Left", new List<AbstractAction>
                    { 
                        new MoveAction(1, "basic", null, "sw", 1)
                    }, Rarity.Common), 
                    new [] {StartingDecks.basic}),
                
                
                
                ["Shield5"] = () => new(new Card("Small Shield", new List<AbstractAction>
                    { 
                        new ShieldAction(1, "basic", null, 5)
                    }, Rarity.Common)),
                ["Shield10"] = () => new(new Card("Medium Shield", new List<AbstractAction>
                { 
                    new ShieldAction(2, "basic", null, 10)
                }, Rarity.Common)),
                ["Shield15"] = () => new(new Card("Large Shield", new List<AbstractAction>
                { 
                    new ShieldAction(3, "basic", null, 15)
                }, Rarity.Uncommon)),
                
                
                ["Draw3"] = () => new(new Card("Draw 3", new List<AbstractAction>
                { 
                    new DrawCardAction(1, "basic", null, 3)
                }, Rarity.Uncommon)),
                
                ["Swipe"] = () => new(new Card("Swipe", new List<AbstractAction>
                    { 
                        new AttackAction(1, "basic", null, "n", 1, 10),
                        new AttackAction(1, "basic", null, "ne", 1, 10),
                        new AttackAction(1, "basic", null, "nw", 1, 10),
                    }, Rarity.Uncommon)),
                
                ["PoisonStrike"] = () => new(new Card("Poison Strike", new List<AbstractAction>
                { 
                    new AttackAction(1, "basic", null, "n", 1, 10),
                    new PoisonAttackAction(1, "basic", null, "n", 1, 0, 10),
                }, Rarity.Uncommon)),
                
                ["Obliterate"] = () => new(new Card("Obliterate", new List<AbstractAction>
                { 
                    new DestroyHandCardAction(4, "basic", null),
                }, Rarity.Uncommon)),
                
                ["GainMoney"] = () => new(new Card("Gain Money", new List<AbstractAction>
                    { 
                        new GainMoneyForCardAction(1, "basic", null),
                    }, Rarity.Uncommon)),
                
                
                ["Refresh"] = () => new(new Card("Refresh", new List<AbstractAction>
                    { 
                        new DiscardHandCardAction(0, "basic", null),
                        new DrawCardAction(1, "basic", null, 3),
                    }, Rarity.Uncommon)),
                
                
            };
        
        public static CardEntry Get(string id) => defs[id]();

        public static IEnumerable<string> AllIds => defs.Keys;

        public static IEnumerable<string> GetIdsFor(StartingDecks deck) =>
            defs.Where(kv => kv.Value().StartingDecks?.Contains(deck) == true)
                .Select(kv => kv.Key);

        public static List<Card> GetStarter(StartingDecks deck) =>
            defs.Where(kv => kv.Value().StartingDecks?.Contains(deck) == true)
                .Select(kv => kv.Value().LocalCard)
                .ToList();
        
        public static List<Card> GetCardsByRarity(Rarity rarity) =>
            defs.Values
                .Where(entry => entry().LocalCard.Rarity == rarity)
                .Select(entry => entry().LocalCard)
                .ToList();

    }
}