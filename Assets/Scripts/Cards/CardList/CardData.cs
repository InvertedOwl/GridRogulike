using System.Collections.Generic;
using System.Linq;
using Types;

namespace Cards.CardList
{
    public class CardData
    {
        public static CardEntry Get(string id)
        {
            CardDefinition definition = CardDefinitionRegistry.GetDefinition(id);
            return new CardEntry(
                definition.CreateCard(),
                definition.GetStartingDeckEntries(),
                definition.CanShowInShop);
        }

        public static IEnumerable<string> AllIds => CardDefinitionRegistry.AllIds;

        public static IEnumerable<string> GetIdsFor(StartingDecks deck)
        {
            if (deck == StartingDecks.allCards)
                return CardDefinitionRegistry.AllIds;

            return CardDefinitionRegistry.Definitions
                .Where(kv => kv.Value.GetStartingDeckEntries().Any(sd => sd.startingDeck == deck))
                .Select(kv => kv.Key);
        }

        public static List<Card> GetStarter(StartingDecks deck)
        {
            if (deck == StartingDecks.allCards)
            {
                return CardDefinitionRegistry.Definitions.Values
                    .Select(definition => definition.CreateCard())
                    .ToList();
            }

            return CardDefinitionRegistry.Definitions.Values
                .SelectMany(definition =>
                    definition.GetStartingDeckEntries()
                        .Where(sd => sd.startingDeck == deck)
                        .SelectMany(sd =>
                            Enumerable.Range(0, sd.numberOfCards)
                                .Select(_ => definition.CreateCard())))
                .ToList();
        }

        public static List<Card> GetCardsByRarity(Rarity rarity)
        {
            return CardDefinitionRegistry.Definitions.Values
                .Where(definition => definition.Rarity == rarity)
                .Select(definition => definition.CreateCard())
                .ToList();
        }

        public static List<Card> GetShopCards()
        {
            return CardDefinitionRegistry.Definitions.Values
                .Where(definition => definition.CanShowInShop)
                .Select(definition => definition.CreateCard())
                .ToList();
        }
    }
}
