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
                // Passives
                ["SpawnPassiveForest"] = () => new(new Card("Spawn Passive", new List<AbstractAction>
                    { 
                        new ScrapCurrentCardAction(0, "basic", null),
                        new SpawnPassiveAction(1, "basic", null, PassiveData.GetPassiveEntry("forest")),
                    }, Rarity.Rare)),
                
                ["SpawnPassiveBloodRitual"] = () => new(new Card("Spawn Passive", new List<AbstractAction>
                    { 
                        new ScrapCurrentCardAction(0, "basic", null),
                        new SpawnPassiveAction(1, "basic", null, PassiveData.GetPassiveEntry("bloodritual")),
                    }, Rarity.Rare)),
                

                // Attacks
                ["AttackLow"] = () => new(new Card("Attack", new List<AbstractAction>
                    { 
                        // Direction does not matter for cards now, since player can choose where to attack.
                        // Distance DOES matter, though
                        new AttackAction(1, "basic", null, "", 1, 10),
                    }, Rarity.Common), 
                    new [] { new StartingDeckEntry(StartingDecks.basic, 4) }),
                
                // Moves
                ["AddMovement"] = () => new(new Card("Dash", new List<AbstractAction>
                    { 
                        new AddStepsCardAction(1, "basic", null, 1),
                    }, Rarity.Common),
                    new [] { new StartingDeckEntry(StartingDecks.basic, 4) }),
                
                
                
                ["Shield5"] = () => new(new Card("Small Shield", new List<AbstractAction>
                    { 
                        new ShieldAction(1, "basic", null, 2)
                    }, Rarity.Common)),
                ["Shield10"] = () => new(new Card("Medium Shield", new List<AbstractAction>
                { 
                    new ShieldAction(2, "basic", null, 4)
                }, Rarity.Common)),
                ["Shield15"] = () => new(new Card("Large Shield", new List<AbstractAction>
                { 
                    new ShieldAction(3, "basic", null, 6)
                }, Rarity.Uncommon)),
                
                
                ["Draw3"] = () => new(new Card("Draw 3", new List<AbstractAction>
                { 
                    new DrawCardAction(1, "basic", null, 3)
                }, Rarity.Uncommon)),
                
                ["Swipe"] = () => new(new Card("Swipe", new List<AbstractAction>
                { 
                    new AttackAction(1, "basic", null, "", 1, 3),
                    new AttackAction(0, "basic", null, "", 1, 3),
                    new AttackAction(0, "basic", null, "", 1, 3),
                }, Rarity.Uncommon)),
                
                ["PoisonStrike"] = () => new(new Card("Poison Strike", new List<AbstractAction>
                { 
                    new PoisonAttackAction(1, "basic", null, "n", 1, 0, 8),
                }, Rarity.Uncommon)),
                
                ["Obliterate"] = () => new(new Card("Obliterate", new List<AbstractAction>
                { 
                    new DestroyHandCardAction(4, "basic", null),
                }, Rarity.Legendary)),
                
                ["GainMoney"] = () => new(new Card("Gain Money", new List<AbstractAction>
                    { 
                        new GainMoneyForCardAction(1, "basic", null),
                    }, Rarity.Uncommon)),
                ["GainBounty"] = () => new(new Card("Gain Money", new List<AbstractAction>
                { 
                    new GainMoneyAction(1, "basic", null, 5),
                    new DrawCardAction(1, "basic", null, 1),
                }, Rarity.Rare)),
                
                
                ["Refresh"] = () => new(new Card("Refresh", new List<AbstractAction>
                    { 
                        new DiscardHandCardAction(0, "basic", null),
                        new DrawCardAction(1, "basic", null, 5),
                    }, Rarity.Uncommon)),
                
                ["DrawFew"] = () => new(new Card("Draw Few", new List<AbstractAction>
                    { 
                        new DrawCardAction(1, "basic", null, 3),
                    }, Rarity.Common)),
                ["DrawSome"] = () => new(new Card("Draw Some", new List<AbstractAction>
                { 
                    new DrawCardAction(1, "basic", null, 5),
                }, Rarity.Uncommon)),
                ["DrawMany"] = () => new(new Card("Draw Many", new List<AbstractAction>
                { 
                    new DrawCardAction(1, "basic", null, 8),
                }, Rarity.Rare)),
                
                // Potions
                ["HealPotion"] = () => new(new Card("Healing Potion", new List<AbstractAction>
                { 
                    new HealAction(0, "basic", null, 25),
                    new DestroyCurrentCardAction(0, "basic", null),
                }, Rarity.Rare)),
                ["DrawPotion"] = () => new(new Card("Draw Potion", new List<AbstractAction>
                { 
                    new DrawCardAction(0, "basic", null, 5),
                    new DestroyCurrentCardAction(0, "basic", null),
                }, Rarity.Rare)),
                ["GainEnergyPotion"] = () => new(new Card("Energy Potion", new List<AbstractAction>
                { 
                    new GainEnergyAction(0, "basic", null, 4),
                    new DestroyCurrentCardAction(0, "basic", null),
                }, Rarity.Rare)),
                
                
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
                        .SelectMany(sd => Enumerable.Repeat(entry.LocalCard, sd.numberOfCards))
                )
                .ToList();

        
        public static List<Card> GetCardsByRarity(Rarity rarity) =>
            defs.Values
                .Where(entry => entry().LocalCard.Rarity == rarity)
                .Select(entry => entry().LocalCard)
                .ToList();

    }
}