using System.Collections.Generic;
using System.Linq;
using Types.Actions;

namespace Cards.CardList
{
    public class CardData
    {
        private static readonly IReadOnlyDictionary<CardIds, CardEntry> defs =
            new Dictionary<CardIds, CardEntry>
            {
                // -  Basic Deck starting hand  - 
                // attack cards
                [CardIds.Attack1] = new(new Card("Attack Up", new List<AbstractAction>
                { 
                    new AttackAction(1, "basic", null, "n", 1, 15)
                }, CardRarity.Common), 
                    new [] {StartingDecks.basic}),
                
                [CardIds.Attack2] = new(new Card("Attack Up Right", new List<AbstractAction>
                    { 
                        new AttackAction(1, "basic", null, "ne", 1, 15)
                    }, CardRarity.Common), 
                    new [] {StartingDecks.basic}),
                
                [CardIds.Attack3] = new(new Card("Attack Up Left", new List<AbstractAction>
                    { 
                        new AttackAction(1, "basic", null, "nw", 1, 15)
                    }, CardRarity.Common), 
                    new [] {StartingDecks.basic}),
                
                [CardIds.Attack4] = new(new Card("Attack Down", new List<AbstractAction>
                    { 
                        new AttackAction(1, "basic", null, "s", 1, 15)
                    }, CardRarity.Common), 
                    new [] {StartingDecks.basic}),
                
                [CardIds.Attack5] = new(new Card("Attack Down Right", new List<AbstractAction>
                    { 
                        new AttackAction(1, "basic", null, "se", 1, 15)
                    }, CardRarity.Common), 
                    new [] {StartingDecks.basic}),
                
                [CardIds.Attack6] = new(new Card("Attack Down Left", new List<AbstractAction>
                    { 
                        new AttackAction(1, "basic", null, "sw", 1, 15)
                    }, CardRarity.Common), 
                    new [] {StartingDecks.basic}),
                // move cards
                [CardIds.Move1] = new(new Card("Move Up", new List<AbstractAction>
                    { 
                        new MoveAction(1, "basic", null, "n", 1)
                    }, CardRarity.Common), 
                    new [] {StartingDecks.basic}),
                
                [CardIds.Move2] = new(new Card("Move Up Right", new List<AbstractAction>
                    { 
                        new MoveAction(1, "basic", null, "ne", 1)
                    }, CardRarity.Common), 
                    new [] {StartingDecks.basic}),
                
                [CardIds.Move3] = new(new Card("Move Up Left", new List<AbstractAction>
                    { 
                        new MoveAction(1, "basic", null, "nw", 1)
                    }, CardRarity.Common), 
                    new [] {StartingDecks.basic}),
                
                [CardIds.Move4] = new(new Card("Move Down", new List<AbstractAction>
                    { 
                        new MoveAction(1, "basic", null, "s", 1)
                    }, CardRarity.Common), 
                    new [] {StartingDecks.basic}),
                
                [CardIds.Move5] = new(new Card("Move Down Right", new List<AbstractAction>
                    { 
                        new MoveAction(1, "basic", null, "se", 1)
                    }, CardRarity.Common), 
                    new [] {StartingDecks.basic}),
                
                [CardIds.Move6] = new(new Card("Move Down Left", new List<AbstractAction>
                    { 
                        new MoveAction(1, "basic", null, "sw", 1)
                    }, CardRarity.Common), 
                    new [] {StartingDecks.basic}),
                
                [CardIds.Special] = new(new Card("Swipe", new List<AbstractAction>
                    { 
                        new AttackAction(1, "basic", null, "n", 1, 10),
                        new AttackAction(1, "basic", null, "ne", 1, 10),
                        new AttackAction(1, "basic", null, "nw", 1, 10),
                    }, CardRarity.Uncommon), 
                    new [] {StartingDecks.basic}),
                
                
            };
        
        public static CardEntry Get(CardIds id) => defs[id];

        public static IEnumerable<CardIds> AllIds => defs.Keys;

        public static IEnumerable<CardIds> GetIdsFor(StartingDecks deck) =>
            defs.Where(kv => kv.Value.StartingDecks?.Contains(deck) == true)
                .Select(kv => kv.Key);

        public static List<Card> GetStarter(StartingDecks deck) =>
            defs.Where(kv => kv.Value.StartingDecks?.Contains(deck) == true)
                .Select(kv => kv.Value.LocalCard)
                .ToList();

    }
}