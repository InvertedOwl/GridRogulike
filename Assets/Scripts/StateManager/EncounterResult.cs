using System;
using Cards.CardList;

namespace StateManager
{
    public enum EncounterResultType
    {
        Money,
        Health,
        MaxHealth,
        Card
    }

    /// <summary>A pending change. Numeric amounts are signed; card amounts are positive counts.</summary>
    [Serializable]
    public class EncounterResult
    {
        public EncounterResultType type;
        public float amount;
        public string cardDefinitionId;

        public EncounterResult Copy()
        {
            return new EncounterResult { type = type, amount = amount, cardDefinitionId = cardDefinitionId };
        }

        public bool TryValidate(out string error)
        {
            error = null;
            if (!Enum.IsDefined(typeof(EncounterResultType), type) || float.IsNaN(amount) || float.IsInfinity(amount))
                error = "Encounter result has an invalid type or amount.";
            else if ((type == EncounterResultType.Money || type == EncounterResultType.Card) &&
                     (amount != Math.Truncate(amount) || (double)amount < int.MinValue || (double)amount > int.MaxValue))
                error = "Money and card results must have whole-number amounts within the integer range.";
            else if (type == EncounterResultType.Card &&
                     (amount <= 0 || string.IsNullOrEmpty(cardDefinitionId) ||
                      !CardDefinitionRegistry.TryGetDefinition(cardDefinitionId, out _)))
                error = $"Encounter result references an invalid card reward '{cardDefinitionId}'.";

            return error == null;
        }
    }
}
