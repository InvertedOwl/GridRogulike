using System.Collections.Generic;
using Cards.CardEvents;

namespace Types.Tiles
{
    public static class BattleTileData
    {
        public const string PrimedAttackId = "PrimedAttack";

        private static readonly Dictionary<string, BattleTileDefinition> Definitions =
            new Dictionary<string, BattleTileDefinition>();

        public static IReadOnlyDictionary<string, BattleTileDefinition> Tiles => Definitions;

        static BattleTileData()
        {
            Register(new BattleTileDefinition(
                PrimedAttackId,
                "Primed Attack",
                "When the player enters this hex, its owner attacks that hex immediately.",
                "Damage4",
                10,
                1,
                true,
                context => new List<AbstractCardEvent>
                {
                    new AttackCardEvent(context.Position, context.Power, manual: false)
                }));
        }

        public static void Register(BattleTileDefinition definition)
        {
            if (definition == null || string.IsNullOrWhiteSpace(definition.Id))
                return;

            Definitions[definition.Id] = definition;
        }

        public static bool TryGet(string id, out BattleTileDefinition definition)
        {
            definition = null;
            return !string.IsNullOrWhiteSpace(id) && Definitions.TryGetValue(id, out definition);
        }
    }
}
