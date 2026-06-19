using System;
using System.Collections.Generic;
using System.Linq;
using Cards.Actions;
using Types;

namespace Cards.CardList
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CardDefinitionAttribute : Attribute
    {
        public string Id { get; }

        public CardDefinitionAttribute(string id)
        {
            Id = id;
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class StartingDeckAttribute : Attribute
    {
        public StartingDecks StartingDeck { get; }
        public int Count { get; }

        public StartingDeckAttribute(StartingDecks startingDeck, int count)
        {
            StartingDeck = startingDeck;
            Count = count;
        }
    }

    public abstract class CardDefinition
    {
        public string Id { get; internal set; }
        public virtual string DisplayName => Id;
        public abstract Rarity Rarity { get; }
        public abstract CardSet CardSet { get; }
        public virtual TargetDefinition TargetDefinition => Cards.CardList.TargetDefinition.None;
        public virtual bool CanShowInShop => true;

        public abstract List<AbstractAction> BuildActions();

        public Card CreateCard(string uniqueId = null)
        {
            return new Card(
                Id,
                DisplayName,
                BuildActions(),
                Rarity,
                CardSet,
                TargetDefinition.Copy(),
                true,
                uniqueId);
        }

        public StartingDeckEntry[] GetStartingDeckEntries()
        {
            return GetType()
                .GetCustomAttributes(typeof(StartingDeckAttribute), false)
                .OfType<StartingDeckAttribute>()
                .Select(attribute => new StartingDeckEntry(attribute.StartingDeck, attribute.Count))
                .ToArray();
        }

        protected static List<AbstractAction> Actions(params AbstractAction[] actions)
        {
            return new List<AbstractAction>(actions);
        }
    }
}
