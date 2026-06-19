using Cards;
using Cards.CardList;

namespace Serializer
{
    [System.Serializable]
    public class CardSaveData
    {
        public string definitionId;
        public string uniqueId;
        public bool isReal;

        public static CardSaveData FromCard(Card card)
        {
            return new CardSaveData
            {
                definitionId = card.DefinitionId,
                uniqueId = card.UniqueId,
                isReal = card.isReal
            };
        }

        public bool TryCreateCard(out Card card)
        {
            card = default;
            if (string.IsNullOrEmpty(definitionId) ||
                !CardDefinitionRegistry.TryGetDefinition(definitionId, out CardDefinition definition))
            {
                return false;
            }

            card = definition.CreateCard(uniqueId);
            card.isReal = isReal;
            return true;
        }
    }
}
