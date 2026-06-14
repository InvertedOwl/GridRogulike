using System.Collections.Generic;
using Entities;
using Grid;
using StateManager;
using UnityEngine;

namespace Cards.CardEvents
{
    public class RandomMoveCardEvent : AbstractCardEvent
    {
        public int distance;
        private readonly RandomState _random;

        public RandomMoveCardEvent(int distance, RandomState random)
        {
            this.distance = distance;
            _random = random;
        }

        public override Dictionary<string, PreviewValue> GetPreviewValues()
        {
            return new Dictionary<string, PreviewValue>
            {
                [CardPreviewKeys.Distance] = PreviewValue.Int(distance),
                [CardPreviewKeys.Direction] = PreviewValue.Text("random")
            };
        }

        public override void Activate(AbstractEntity entity)
        {
            if (entity == null)
                return;

            if (GameStateManager.Instance.GetCurrent<PlayingState>() is not { } playing)
                return;

            List<string> validDirections = GetValidDirections(entity, playing);
            if (validDirections.Count == 0)
                return;

            RandomState random = _random ?? RunInfo.NewRandom("randommoveevent");
            string direction = validDirections[random.Next(0, validDirections.Count)];
            playing.MoveEntity(entity, direction, distance);
        }

        private List<string> GetValidDirections(AbstractEntity entity, PlayingState playing)
        {
            List<string> validDirections = new List<string>();
            int moveDistance = Mathf.Max(1, distance);

            foreach (string direction in HexGridManager.HexDirections)
            {
                Vector2Int target = HexGridManager.MoveHex(entity.positionRowCol, direction, moveDistance);
                if (playing.IsValidHex(target))
                {
                    validDirections.Add(direction);
                }
            }

            return validDirections;
        }
    }
}
