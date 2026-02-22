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
                // - Common -
                    // Basic Starting Deck x2
                ["AttackLow"] = () => new(new Card("Attack", new List<AbstractAction>
                    { 
                        new AttackAction(1, "basic", null, "", 1, 5),
                    }, Rarity.Common),
                    new [] { new StartingDeckEntry(StartingDecks.basic, 4) }),
                
                ["PoisonEase"] = () => new(new Card("Poison", new List<AbstractAction>
                    { 
                        new PoisonAttackAction(1, "basic", null, "", 2, 0, 3),
                    }, Rarity.Common)),
                
                ["Invest"] = () => new(new Card("Invest", new List<AbstractAction>
                    {   
                        new GainMoneyAction(2, "basic", null, 3),
                    }, Rarity.Common)),

                ["Crack"] = () => new(new Card("Crack", new List<AbstractAction>
                { 
                    new AttackAction(1, "basic", null, "", 1, 4),
                    new AttackAction(0, "basic", null, "", 1, 4),
                }, Rarity.Common)),

                ["Offensive Move"] = () => new(new Card("Offensive Move", new List<AbstractAction>
                { 
                    new AttackAction(1, "basic", null, "", 1, 4),
                    new GainStepsCardAction(0, "basic", null, 1)
                }, Rarity.Common)),

                ["Stomp"] = () => new(new Card("Stomp", new List<AbstractAction>
                { 
                    new AttackAllAction(1, "basic", null, 4),
                }, Rarity.Common)),

                    // Basic Starting Deck x2
                ["AddMovement"] = () => new(new Card("Walk", new List<AbstractAction>
                    { 
                        new GainStepsCardAction(1, "basic", null, 1),
                    }, Rarity.Common),
                    new [] { new StartingDeckEntry(StartingDecks.basic, 2) }),

                ["Shield5"] = () => new(new Card("Small Shield", new List<AbstractAction>
                { 
                    new ShieldAction(1, "basic", null, 5)
                }, Rarity.Common),
                new [] { new StartingDeckEntry(StartingDecks.basic, 2)}),

                ["ShieldTradeOff"] = () => new(new Card("Trade Off", new List<AbstractAction>
                { 
                    new ShieldAction(1, "basic", null, 12),
                    new DiscardHandCardAction(1, "basic", null),
                }, Rarity.Common)),

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

                ["QuickDraw"] = () => new(new Card("Quick Draw", new List<AbstractAction>
                { 
                    new RaiseCostAction(0, "basic", null),
                    new DrawCardAction(0, "basic", null, 3),
                }, Rarity.Common)),

                
                // - Uncommon - 
                ["AttackHigh"] = () => new(new Card("Smash", new List<AbstractAction>
                { 
                    new AttackAction(1, "basic", null, "", 1, 12),
                }, Rarity.Uncommon)),

                ["FrostStrike"] = () => new(new Card("Frost Blast", new List<AbstractAction>
                { 
                    new FrostAttackAction(1, "basic", null, "n", 5, 0, 5),
                }, Rarity.Uncommon)),

                ["Swipe"] = () => new(new Card("Ambush", new List<AbstractAction>
                { 
                    new AttackAction(1, "basic", null, "", 1, 3),
                    new AttackAction(0, "basic", null, "", 1, 3),
                    new AttackAction(0, "basic", null, "", 1, 3),
                }, Rarity.Uncommon)),

                ["Leap"] = () => new(new Card("Leap", new List<AbstractAction>
                { 
                    new GainStepsCardAction(1, "basic", null, 2),
                }, Rarity.Uncommon)),
                
                ["Flee"] = () => new(new Card("Flee", new List<AbstractAction>
                { 
                    new GainStepsCardAction(1, "basic", null, 2),
                    new ShieldAction(1, "basic", null, 5),
                }, Rarity.Uncommon)),

                ["Revitalize"] = () => new(new Card("Revitalize", new List<AbstractAction>
                { 
                    new ShieldAction(1, "basic", null, 5),
                    new DrawCardAction(0, "basic", null, 1),
                }, Rarity.Uncommon)),

                ["MedShield"] = () => new(new Card("Shield", new List<AbstractAction>
                { 
                    new ShieldAction(1, "basic", null, 12)
                }, Rarity.Uncommon)),

                ["Draw3"] = () => new(new Card("Mulligan", new List<AbstractAction>
                { 
                    new DiscardCardsAction(0, "basic", null, 3),
                    new DrawCardAction(1, "basic", null, 3),
                }, Rarity.Uncommon)),

                ["DrawSome"] = () => new(new Card("Picky", new List<AbstractAction>
                { 
                    new DrawCardAction(1, "basic", null, 2),
                }, Rarity.Uncommon)),
                ["SpawnPassivePoison"] = () => new(new Card("Spawn Passive", new List<AbstractAction>
                { 
                    new ScrapCurrentCardAction(0, "basic", null, false),
                    new SpawnPassiveAction(2, "basic", null, PassiveData.GetPassiveEntry("poisonswamp")),
                }, Rarity.Uncommon)),


                // - Rare - 
                ["SpawnPassiveForest"] = () => new(new Card("Spawn Passive", new List<AbstractAction>
                { 
                    new ScrapCurrentCardAction(0, "basic", null, false),
                    new SpawnPassiveAction(1, "basic", null, PassiveData.GetPassiveEntry("forest")),
                }, Rarity.Rare)),
                
                ["GainMoney"] = () => new(new Card("401k", new List<AbstractAction>
                { 
                    new GainMoneyForCardAction(1, "basic", null),
                }, Rarity.Rare)),

                ["SpawnPassiveBloodRitual"] = () => new(new Card("Spawn Passive", new List<AbstractAction>
                { 
                    new ScrapCurrentCardAction(0, "basic", null, false),
                    new SpawnPassiveAction(1, "basic", null, PassiveData.GetPassiveEntry("bloodritual")),
                }, Rarity.Rare)),

                ["AttackMedium"] = () => new(new Card("Slice", new List<AbstractAction>
                { 
                    new AttackAction(1, "basic", null, "", 1, 30),
                    new DiscardCardsAction(1, "basic", null, 2),
                }, Rarity.Rare)),

                ["PoisonStrike"] = () => new(new Card("Poison Strike", new List<AbstractAction>
                { 
                    new PoisonAttackAction(1, "basic", null, "", 1, 0, 6),
                }, Rarity.Rare)),

                ["BigShield"] = () => new(new Card("Large Shield", new List<AbstractAction>
                { 
                    new ShieldAction(2, "basic", null, 20)
                }, Rarity.Rare)),

                ["DoubleDouble"] = () => new(new Card("Armory", new List<AbstractAction>
                { 
                    new DoubleShieldAction(2, "basic", null),
                }, Rarity.Rare)),

                ["DrawMany"] = () => new(new Card("Draw Many", new List<AbstractAction>
                { 
                    new DrawCardAction(3, "basic", null, 6),
                }, Rarity.Rare)),

                ["GainBounty"] = () => new(new Card("Gain Money", new List<AbstractAction>
                { 
                    new GainMoneyAction(1, "basic", null, 2),
                    new DrawCardAction(1, "basic", null, 2),
                }, Rarity.Rare)),

                ["Obliterate"] = () => new(new Card("Obliterate", new List<AbstractAction>
                { 
                    new AttackAction(4, "basic", null, "", 1, 20),
                    new DestroyHandCardAction(1, "basic", null),
                }, Rarity.Epic)),
                
                    // Potions!
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
                    new AttackAllAction(0, "basic", null, 20),
                    new DestroyCurrentCardAction(0, "basic", null),
                }, Rarity.Rare)),

                ["FreeCardsPotion"] = () => new(new Card("Free Cards Potion", new List<AbstractAction>
                { 
                    new RandomNoCostAction(0, "basic", null),
                    new RandomNoCostAction(0, "basic", null),
                    new RandomNoCostAction(0, "basic", null),
                }, Rarity.Rare)),
                // - Epic - 
                ["DrawFewAndEnergy"] = () => new(new Card("Anticipate", new List<AbstractAction>
                { 
                    new GainEnergyAction(0, "basic", null, 1),
                    new DrawCardAction(0, "basic", null, 2),
                }, Rarity.Epic)),

                // - Legendary - 
                ["Assault"] = () => new(new Card("Meteor Strike", new List<AbstractAction>
                { 
                    new AttackAllAction(3, "basic", null, 20),
                }, Rarity.Legendary)),



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