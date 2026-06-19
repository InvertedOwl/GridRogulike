using System.Collections.Generic;
using Cards.CardList;
using Entities;
using UnityEngine;

namespace Cards
{
    public class TargetSelection
    {
        public TargetDefinition Definition { get; }
        public IReadOnlyList<AbstractEntity> TargetEntities => _targetEntities;
        public IReadOnlyList<Vector2Int> TargetPositions => _targetPositions;
        public bool HasTargets => _targetEntities.Count > 0 || _targetPositions.Count > 0;

        private readonly List<AbstractEntity> _targetEntities;
        private readonly List<Vector2Int> _targetPositions;

        public TargetSelection(
            TargetDefinition definition,
            IEnumerable<AbstractEntity> targetEntities = null,
            IEnumerable<Vector2Int> targetPositions = null)
        {
            Definition = definition ?? TargetDefinition.None;
            _targetEntities = targetEntities != null
                ? new List<AbstractEntity>(targetEntities)
                : new List<AbstractEntity>();
            _targetPositions = targetPositions != null
                ? new List<Vector2Int>(targetPositions)
                : new List<Vector2Int>();
        }

        public static TargetSelection Empty(TargetDefinition definition = null)
        {
            return new TargetSelection(definition ?? TargetDefinition.None);
        }

        public bool TryGetFirstEntity(out AbstractEntity entity)
        {
            foreach (AbstractEntity target in _targetEntities)
            {
                if (target != null && target.Health > 0)
                {
                    entity = target;
                    return true;
                }
            }

            entity = null;
            return false;
        }

        public bool TryGetFirstPosition(out Vector2Int position)
        {
            if (_targetPositions.Count > 0)
            {
                position = _targetPositions[0];
                return true;
            }

            if (TryGetFirstEntity(out AbstractEntity entity))
            {
                position = entity.positionRowCol;
                return true;
            }

            position = default;
            return false;
        }
    }
}
