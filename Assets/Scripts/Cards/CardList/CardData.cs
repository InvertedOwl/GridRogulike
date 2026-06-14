using System;
using System.Collections.Generic;
using System.Linq;
using Cards.Actions;
using Cards.CardEvents;
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
                    new AttackAllAction(0, "basic", null, 1000),
                    new PushAllEnemiesAwayAction(0, "basic", null, 1),
                }, Rarity.Developer, CardSet.Developer),
                new [] { new StartingDeckEntry(StartingDecks.basic, 0) }, false),
                ["DeveloperSteps"] = () => new(new Card("Developer Steps", new List<AbstractAction>
                    {
                        new GainStepsCardAction(0, "basic", null, 1000)
                    }, Rarity.Developer, CardSet.Developer),
                    new [] { new StartingDeckEntry(StartingDecks.basic, 0) }, false),



                // Common attacks
                ["Quick Strike"] = () => new(new Card("Quick Strike", new List<AbstractAction>
                    {
                        new AttackAction(1, "basic", null, "", 1, 8),
                    }, Rarity.Common, CardSet.Base),
                    new [] { new StartingDeckEntry(StartingDecks.basic, 2) }, false),
                ["Heavy Strike"] = () => new(new Card("Heavy Strike", new List<AbstractAction>
                    {
                        new AttackAction(1, "basic", null, "", 1, 14),
                    }, Rarity.Common, CardSet.Base), new StartingDeckEntry[0], true),
                ["Lance"] = () => new(new Card("Lance", new List<AbstractAction>
                    {
                        new AttackAction(2, "basic", null, "", 2, 8),
                    }, Rarity.Common, CardSet.Base), new StartingDeckEntry[0], true),
                ["Sweep"] = () => new(new Card("Sweep", new List<AbstractAction>
                    {
                        new AttackAllAction(3, "basic", null, 7),
                    }, Rarity.Common, CardSet.Base), new StartingDeckEntry[0], true),
                ["Close Sweep"] = () => new(new Card("Close Sweep", new List<AbstractAction>
                    {
                        new AttackRadiusAction(1, "basic", null, 1, 6),
                    }, Rarity.Common, CardSet.Base), new StartingDeckEntry[0], true),
                ["Tap"] = () => new(new Card("Tap", new List<AbstractAction>
                    {
                        new AttackAction(1, "basic", null, "", 1, 2),
                        new ApplyStatusToEntityAction(0, "basic", null, null, StatusApplicationType.Dizzy, 2)
                    }, Rarity.Common, CardSet.Base), new StartingDeckEntry[0], true),

                // Uncommon attacks
                ["Slash"] = () => new(new Card("Slash", new List<AbstractAction>
                    {
                        new AttackAction(1, "basic", null, "", 1, 8),
                        new AttackRadiusAction(0, "basic", null, 1, 4)
                    }, Rarity.Uncommon, CardSet.Base), new StartingDeckEntry[0], true),
                ["Wild Swing"] = () => new(new Card("Wild Swing", new List<AbstractAction>
                    {
                        new AttackAction(1, "basic", null, "", 1, 12),
                        new MoveRandomAction(0, "basic", null)
                    }, Rarity.Uncommon, CardSet.Base), new StartingDeckEntry[0], true),
                ["Shove"] = () => new(new Card("Shove", new List<AbstractAction>
                    {
                        new AttackAction(1, "basic", null, "", 1, 7),
                        new PushEnemyAwayAction(1,  "basic", null, 1),
                    }, Rarity.Uncommon, CardSet.Base), new StartingDeckEntry[0], true),
                ["Energetic Strike"] = () => new(new Card("Energetic Strike", new List<AbstractAction>
                    {
                        new AttackFromSteps(1, "basic", null, "", 1),
                    }, Rarity.Uncommon, CardSet.Base), new StartingDeckEntry[0], true),
                ["Reaching Guard"] = () => new(new Card("Reaching Guard", new List<AbstractAction>
                    {
                        new AttackFromSteps(1, "basic", null, "", 1),
                    }, Rarity.Uncommon, CardSet.Base), new StartingDeckEntry[0], true),

                // Rare Attack
                ["Cleaver"] = () => new(new Card("Cleaver", new List<AbstractAction>
                    {
                        new AttackAllAction(1, "basic", null, 8),
                    }, Rarity.Rare, CardSet.Base), new StartingDeckEntry[0], true),
                ["Wave"] = () => new(new Card("Wave", new List<AbstractAction>
                    {
                        new AttackAction(1, "basic", null, "", 1, 8),
                        new PushAllEnemiesAwayAction(0, "basic", null, 1)
                    }, Rarity.Rare, CardSet.Base), new StartingDeckEntry[0], true),
                ["Sniper"] = () => new(new Card("Sniper", new List<AbstractAction>
                    {
                        new AttackAction(2, "basic", null, "", 3, 18),
                    }, Rarity.Rare, CardSet.Base), new StartingDeckEntry[0], true),




                // Common Shields
                ["Guard"] = () => new(new Card("Guard", new List<AbstractAction>
                    {
                        new ShieldAction(1, "basic", null, 10),
                    }, Rarity.Common, CardSet.Base),
                    new [] { new StartingDeckEntry(StartingDecks.basic, 2) }, false),
                ["Reinforce"] = () => new(new Card("Reinforce", new List<AbstractAction>
                    {
                        new ShieldAction(1, "basic", null, 15),
                    }, Rarity.Common, CardSet.Base), new StartingDeckEntry[0], true),
                ["Delayed Block"] = () => new(new Card("Delayed Block", new List<AbstractAction>
                    {
                        new DelayedShieldAction(1, "basic", null, 20),
                    }, Rarity.Common, CardSet.Base), new StartingDeckEntry[0], true),
                ["Heavy Guard"] = () => new(new Card("Heavy Guard", new List<AbstractAction>
                    {
                        new ShieldAction(2, "basic", null, 25)
                    }, Rarity.Common, CardSet.Base), new StartingDeckEntry[0], true),
                ["Drunken Guard"] = () => new(new Card("Drunken Guard", new List<AbstractAction>
                    {
                        new ShieldAction(1, "basic", null, 18),
                        new MoveRandomAction(0, "basic", null),
                    }, Rarity.Common, CardSet.Base), new StartingDeckEntry[0], true),

                // Uncommon Shield
                ["Shield Burst"] = () => new(new Card("Shield Burst", new List<AbstractAction>
                    {
                        new ShieldAction(1, "basic", null, 8),
                        new AttackAllAction(2, "basic", null, 8),
                    }, Rarity.Uncommon, CardSet.Base), new StartingDeckEntry[0], true),
                ["Repel"] = () => new(new Card("Repel", new List<AbstractAction>
                    {
                        new ShieldAction(1, "basic", null, 8),
                        new PushNearbyEnemiesAwayAction(1, "basic", null, 1)
                    }, Rarity.Uncommon, CardSet.Base), new StartingDeckEntry[0], true),

                // Rare Shield
                ["Double Double"] = () => new(new Card("Double Double", new List<AbstractAction>
                    {
                        new DoubleShieldAction(1, "basic", null),
                    }, Rarity.Rare, CardSet.Base), new StartingDeckEntry[0], true),




                // Common Steps
                ["Step"] = () => new(new Card("Step", new List<AbstractAction>
                    {
                        new GainStepsCardAction(2, "basic", null, 1),
                    }, Rarity.Common, CardSet.Base),
                    new [] { new StartingDeckEntry(StartingDecks.basic, 2) }, false),
                ["Careful Step"] = () => new(new Card("Careful Step", new List<AbstractAction>
                    {
                        new GainStepsCardAction(1, "basic", null, 1),
                        new DrawCardAction(1, "basic", null, 1),
                    }, Rarity.Common, CardSet.Base),
                    new StartingDeckEntry[0], true),
                ["Sprint"] = () => new(new Card("Sprint", new List<AbstractAction>
                    {
                        new GainStepsCardAction(1, "basic", null, 2),
                        new DiscardHandAction(0, "basic", null),
                    }, Rarity.Common, CardSet.Base),
                    new StartingDeckEntry[0], true),

                // Uncommon Steps
                ["Quick Step"] = () => new(new Card("Quick Step", new List<AbstractAction>
                    {
                        new GainStepsCardAction(1, "basic", null, 1),
                    }, Rarity.Uncommon, CardSet.Base),
                    new StartingDeckEntry[0], true),
                ["Overextend"] = () => new(new Card("Overextend", new List<AbstractAction>
                    {
                        new GainStepsCardAction(1, "basic", null, 4),
                        new DamageSelfAction(1, "basic", null, 6)

                    }, Rarity.Uncommon, CardSet.Base),
                    new StartingDeckEntry[0], true),



                // Card draw
                ["Messy Draw"] = () => new(new Card("Messy Draw", new List<AbstractAction>
                    {
                        new DiscardCardsAction(0, "basic", null, 1),
                        new DrawCardAction(1, "basic", null, 2),
                    }, Rarity.Common, CardSet.Base),
                    new StartingDeckEntry[0], true),
                ["Reset Button"] = () => new(new Card("Reset Button", new List<AbstractAction>
                    {
                        new DiscardHandAction(0, "basic", null),
                        new DrawCardAction(1, "basic", null, 3),
                        new GainEnergyAction(1, "basic", null, 1),
                    }, Rarity.Common, CardSet.Base),
                    new StartingDeckEntry[0], true),
                ["Toss"] = () => new(new Card("Toss", new List<AbstractAction>
                    {
                        new DiscardCardsAction(1, "basic", null, 2),
                        new DrawCardAction(1, "basic", null, 4),
                    }, Rarity.Common, CardSet.Base),
                    new StartingDeckEntry[0], true),
                ["Moment"] = () => new(new Card("Moment", new List<AbstractAction>
                    {
                        new GainEnergyAction(1,  "basic", null, 2),
                        new DamageSelfAction(0, "basic", null, 5),
                    }, Rarity.Common, CardSet.Base),
                    new StartingDeckEntry[0], true),

                // ["Moment"] = () => new(new Card("Moment", new List<AbstractAction>
                //     {
                //         new GainEnergyAction(1,  "basic", null, 2),
                //         new DamageSelfAction(1, "basic", null, 5),
                //     }, Rarity.Common, CardSet.Base),
                //     new StartingDeckEntry[0], true),

                // Misc
                ["Quick Draw"] = () => new(new Card("Quick Draw", new List<AbstractAction>
                    {
                        new RaiseCostAction(0, "basic", null),
                        new DrawCardAction(0, "basic", null, 3),
                    }, Rarity.Uncommon, CardSet.Base)),
                ["Power Surge"] = () => new(new Card("Power Surge", new List<AbstractAction>
                    {
                        new GainEnergyAction(5, "basic", null, 6),
                    }, Rarity.Uncommon, CardSet.Base)),

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
