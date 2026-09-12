using System.Collections.Generic;
using Cards.CardEvents;
using Entities;
using Types.Tiles;
using UnityEngine;

namespace Cards.Actions
{
    public class PlaceBattleTileAction : AbstractAction
    {
        public string DefinitionId { get; }
        public Vector2Int TargetPosition { get; }
        public int Power { get; }
        public bool ReplaceExisting { get; }

        public override string Icon
        {
            get
            {
                return BattleTileData.TryGet(DefinitionId, out BattleTileDefinition definition)
                    ? definition.Icon
                    : "Question";
            }
        }

        public PlaceBattleTileAction(
            int baseCost,
            string color,
            AbstractEntity entity,
            string definitionId,
            Vector2Int targetPosition,
            int power = -1,
            bool replaceExisting = true) : base(baseCost, color, entity)
        {
            DefinitionId = definitionId;
            TargetPosition = targetPosition;
            Power = power < 0 && BattleTileData.TryGet(definitionId, out BattleTileDefinition definition)
                ? definition.DefaultPower
                : power;
            ReplaceExisting = replaceExisting;
        }

        public override List<AbstractCardEvent> Activate(CardMonobehaviour cardMono)
        {
            return new List<AbstractCardEvent>
            {
                new PlaceBattleTileCardEvent(DefinitionId, TargetPosition, Power, ReplaceExisting)
            };
        }

        public override string GetText()
        {
            string tileName = BattleTileData.TryGet(DefinitionId, out BattleTileDefinition definition)
                ? definition.Name
                : DefinitionId;
            return $"Place {tileName} at {TargetPosition}";
        }

        public override string ToSimpleText()
        {
            string icon = Icon;
            string powerText = Power >= 0 ? $" {Power}" : string.Empty;
            return $"Place <sprite name={icon}>{powerText}";
        }

        public override List<RectTransform> UpdateGraphic(
            GameObject diagram,
            GameObject tilePrefab,
            GameObject arrowPrefab)
        {
            return new List<RectTransform>();
        }

        public override string ToString()
        {
            return $"Place {DefinitionId} at {TargetPosition}";
        }
    }
}
