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
                    }, Rarity.Rare),
                    new [] { new StartingDeckEntry(StartingDecks.basic, 1) }),
                

                // Attacks
                ["AttackLow"] = () => new(new Card("Attack", new List<AbstractAction>
                    { 
                        // Direction does not matter for cards now, since player can choose where to attack.
                        // Distance DOES matter, though
                        new AttackAction(1, "basic", null, "", 1, 5),
                    }, Rarity.Common),
                    new [] { new StartingDeckEntry(StartingDecks.basic, 4) }),
                ["AttackMedium"] = () => new(new Card("Slice", new List<AbstractAction>
                    { 
                        new AttackAction(1, "basic", null, "", 1, 8),
                        new DiscardCardsAction(1, "basic", null, 1),
                    }, Rarity.Rare)),
                ["AttackHigh"] = () => new(new Card("Assault", new List<AbstractAction>
                { 
                    new AttackAction(3, "basic", null, "", 1, 15),
                }, Rarity.Legendary)),
                ["Obliterate"] = () => new(new Card("Obliterate", new List<AbstractAction>
                { 
                    new AttackAction(4, "basic", null, "", 1, 20),
                    new DestroyHandCardAction(1, "basic", null),
                }, Rarity.Legendary)),
                ["PoisonStrike"] = () => new(new Card("Poison Strike", new List<AbstractAction>
                { 
                    new PoisonAttackAction(1, "basic", null, "n", 1, 0, 8),
                }, Rarity.Rare)),
                ["Swipe"] = () => new(new Card("Ambush", new List<AbstractAction>
                { 
                    new AttackAction(1, "basic", null, "", 1, 3),
                    new AttackAction(0, "basic", null, "", 1, 3),
                    new AttackAction(0, "basic", null, "", 1, 3),
                }, Rarity.Uncommon)),
                ["Stomp"] = () => new(new Card("Stomp", new List<AbstractAction>
                { 
                    new AttackAllAction(1, "basic", null, 3),
                }, Rarity.Common)),
                
                
                // Moves
                ["AddMovement"] = () => new(new Card("Walk", new List<AbstractAction>
                    { 
                        new GainStepsCardAction(1, "basic", null, 1),
                    }, Rarity.Common),
                    new [] { new StartingDeckEntry(StartingDecks.basic, 4) }),
                ["Leap"] = () => new(new Card("Leap", new List<AbstractAction>
                    { 
                        new GainStepsCardAction(2, "basic", null, 2),
                    }, Rarity.Uncommon)),
                
                    
                
                
                ["Shield5"] = () => new(new Card("Small Shield", new List<AbstractAction>
                    { 
                        new ShieldAction(1, "basic", null, 2)
                    }, Rarity.Common)),
                ["ShieldTradeOff"] = () => new(new Card("Trade Off", new List<AbstractAction>
                { 
                    new ShieldAction(1, "basic", null, 8),
                    new DiscardHandCardAction(0, "basic", null),
                }, Rarity.Common)),
                ["Revitalize"] = () => new(new Card("Revitalize", new List<AbstractAction>
                { 
                    new ShieldAction(1, "basic", null, 5),
                    new DrawCardAction(1, "basic", null, 1),
                }, Rarity.Uncommon)),

                ["Shield10"] = () => new(new Card("Medium Shield", new List<AbstractAction>
                { 
                    new ShieldAction(2, "basic", null, 4)
                }, Rarity.Uncommon)),
                ["Shield15"] = () => new(new Card("Large Shield", new List<AbstractAction>
                { 
                    new ShieldAction(3, "basic", null, 6)
                }, Rarity.Rare)),
                ["Defensive"] = () => new(new Card("Defensive Strike", new List<AbstractAction>
                { 
                    new ShieldAction(1, "basic", null, 5),
                    new AttackAction(1, "basic", null, "", 1, 5)
                }, Rarity.Common)),
                ["Equipped"] = () => new(new Card("Equipped", new List<AbstractAction>
                { 
                    new ShieldAction(1, "basic", null, 3),
                    new ShieldAction(0, "basic", null, 3),
                    new ShieldAction(0, "basic", null, 3),
                }, Rarity.Common)),
                ["DoubleDouble"] = () => new(new Card("Armory", new List<AbstractAction>
                { 
                    new DoubleShieldAction(2, "basic", null),
                }, Rarity.Rare)),
                
                

                ["Draw2"] = () => new(new Card("Draw 2", new List<AbstractAction>
                { 
                    new RaiseCostAction(0, "basic", null),
                    new DrawCardAction(0, "basic", null, 2),
                }, Rarity.Common)),
                
                ["Draw3"] = () => new(new Card("Retry", new List<AbstractAction>
                { 
                    new DiscardCardsAction(0, "basic", null, 2),
                    new DrawCardAction(1, "basic", null, 3),
                }, Rarity.Uncommon)),
                
                ["GainMoney"] = () => new(new Card("401k", new List<AbstractAction>
                { 
                    new GainMoneyForCardAction(1, "basic", null),
                }, Rarity.Uncommon)),
                ["GainBounty"] = () => new(new Card("Gain Money", new List<AbstractAction>
                { 
                    new GainMoneyAction(1, "basic", null, 2),
                    new DrawCardAction(1, "basic", null, 1),
                }, Rarity.Rare)),
                
                ["DrawFewAndEnergy"] = () => new(new Card("Anticipate", new List<AbstractAction>
                { 
                    new GainEnergyAction(0, "basic", null, 1),
                    new DrawCardAction(0, "basic", null, 2),
                }, Rarity.Epic)),
                ["DrawSome"] = () => new(new Card("Draw Some", new List<AbstractAction>
                { 
                    new DrawCardAction(1, "basic", null, 4),
                }, Rarity.Uncommon)),
                ["DrawMany"] = () => new(new Card("Draw Many", new List<AbstractAction>
                { 
                    new DrawCardAction(1, "basic", null, 6),
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
                ["GainMovePotion"] = () => new(new Card("Move Potion", new List<AbstractAction>
                { 
                    new GainStepsCardAction(0, "basic", null, 3),
                    new DestroyCurrentCardAction(0, "basic", null),
                }, Rarity.Rare)),
                ["AttackAllPotion"] = () => new(new Card("Attack Potion", new List<AbstractAction>
                { 
                    new AttackAllAction(0, "basic", null, 5),
                    new DestroyCurrentCardAction(0, "basic", null),
                }, Rarity.Rare)),
                ["FreeCardsPotion"] = () => new(new Card("Free Cards Potion", new List<AbstractAction>
                { 
                    new RandomNoCostAction(0, "basic", null),
                    new RandomNoCostAction(0, "basic", null),
                    new RandomNoCostAction(0, "basic", null),
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