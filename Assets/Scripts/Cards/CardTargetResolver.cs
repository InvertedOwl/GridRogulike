using System.Collections.Generic;
using System.Linq;
using Cards.CardList;
using Entities;
using Grid;
using StateManager;
using Types.Statuses;
using UnityEngine;

namespace Cards
{
    public static class CardTargetResolver
    {
        public static TargetDefinition GetModifiedTargetDefinition(
            CardMonobehaviour cardMono,
            Card card,
            AbstractEntity sourceEntity,
            PlayingState playingState,
            bool previewMode)
        {
            TargetDefinition targetDefinition = card.TargetDefinition?.Copy() ?? TargetDefinition.None;
            CardPlayContext context = new CardPlayContext(
                cardMono,
                card,
                sourceEntity,
                TargetSelection.Empty(targetDefinition),
                playingState,
                previewMode);

            if (sourceEntity?.statusManager == null)
                return targetDefinition;

            foreach (AbstractStatus status in sourceEntity.statusManager.statusList)
            {
                targetDefinition = status.ModifyTargetDefinition(targetDefinition, context) ?? targetDefinition;
                context = new CardPlayContext(
                    cardMono,
                    card,
                    sourceEntity,
                    TargetSelection.Empty(targetDefinition),
                    playingState,
                    previewMode);
            }

            return targetDefinition;
        }

        public static TargetSelection ResolveAvailableTargets(
            CardMonobehaviour cardMono,
            Card card,
            AbstractEntity sourceEntity,
            PlayingState playingState,
            bool previewMode)
        {
            TargetDefinition targetDefinition = GetModifiedTargetDefinition(
                cardMono,
                card,
                sourceEntity,
                playingState,
                previewMode);

            return ResolveAvailableTargets(targetDefinition, sourceEntity, playingState);
        }

        public static TargetSelection ResolveAvailableTargets(
            TargetDefinition targetDefinition,
            AbstractEntity sourceEntity,
            PlayingState playingState)
        {
            targetDefinition ??= TargetDefinition.None;

            if (sourceEntity == null || playingState == null)
                return TargetSelection.Empty(targetDefinition);

            switch (targetDefinition.TargetType)
            {
                case TargetType.None:
                    return TargetSelection.Empty(targetDefinition);

                case TargetType.Self:
                    return new TargetSelection(
                        targetDefinition,
                        new[] { sourceEntity },
                        new[] { sourceEntity.positionRowCol });

                case TargetType.AnyEnemy:
                case TargetType.EveryEnemy:
                case TargetType.AnyEntity:
                    return ResolveEntityTargets(targetDefinition, sourceEntity, playingState);

                case TargetType.AnyTile:
                case TargetType.EmptyTile:
                    return ResolveTileTargets(targetDefinition, sourceEntity, playingState);

                default:
                    return TargetSelection.Empty(targetDefinition);
            }
        }

        public static bool TryResolveSelectionForClick(
            CardMonobehaviour cardMono,
            Card card,
            AbstractEntity sourceEntity,
            PlayingState playingState,
            Vector2Int clickedPosition,
            out TargetSelection selection)
        {
            TargetSelection available = ResolveAvailableTargets(cardMono, card, sourceEntity, playingState, false);
            selection = TargetSelection.Empty(available.Definition);

            if (!available.TargetPositions.Contains(clickedPosition))
                return false;

            if (available.Definition.TargetType == TargetType.EveryEnemy)
            {
                selection = available;
                return selection.HasTargets;
            }

            List<AbstractEntity> clickedEntities = new List<AbstractEntity>();
            playingState.EntitiesOnHex(clickedPosition, out List<AbstractEntity> entitiesOnHex);
            foreach (AbstractEntity entity in entitiesOnHex)
            {
                if (available.TargetEntities.Contains(entity))
                    clickedEntities.Add(entity);
            }

            if (clickedEntities.Count > 0)
            {
                selection = new TargetSelection(
                    available.Definition,
                    new[] { clickedEntities[0] },
                    new[] { clickedEntities[0].positionRowCol });
                return true;
            }

            if (available.Definition.TargetType is TargetType.AnyTile or TargetType.EmptyTile)
            {
                selection = new TargetSelection(
                    available.Definition,
                    null,
                    new[] { clickedPosition });
                return true;
            }

            return false;
        }

        public static bool TryResolveCardClickSelection(
            CardMonobehaviour cardMono,
            Card card,
            AbstractEntity sourceEntity,
            PlayingState playingState,
            out TargetSelection selection)
        {
            TargetSelection available = ResolveAvailableTargets(cardMono, card, sourceEntity, playingState, false);
            selection = available;

            if (!available.Definition.CanPlayFromCardClick)
                return false;

            if (available.Definition.TargetType == TargetType.None)
            {
                selection = TargetSelection.Empty(available.Definition);
                return true;
            }

            if (available.Definition.TargetType == TargetType.Self)
                return available.HasTargets;

            if (available.Definition.TargetType == TargetType.EveryEnemy)
                return available.HasTargets;

            return false;
        }

        public static bool HasPlayableTargets(
            CardMonobehaviour cardMono,
            Card card,
            AbstractEntity sourceEntity,
            PlayingState playingState)
        {
            TargetSelection available = ResolveAvailableTargets(cardMono, card, sourceEntity, playingState, true);
            TargetType targetType = available.Definition.TargetType;

            if (targetType == TargetType.None)
                return true;

            if (targetType == TargetType.Self)
                return available.HasTargets;

            return available.HasTargets;
        }

        private static TargetSelection ResolveEntityTargets(
            TargetDefinition targetDefinition,
            AbstractEntity sourceEntity,
            PlayingState playingState)
        {
            List<AbstractEntity> targets = new List<AbstractEntity>();
            List<Vector2Int> positions = new List<Vector2Int>();

            foreach (AbstractEntity entity in playingState.GetEntities())
            {
                if (!IsValidEntityTarget(targetDefinition.TargetType, sourceEntity, entity, playingState))
                    continue;

                if (!IsInRange(targetDefinition, sourceEntity, entity.positionRowCol, playingState, entity))
                    continue;

                targets.Add(entity);
                positions.Add(entity.positionRowCol);
            }

            return new TargetSelection(targetDefinition, targets, positions);
        }

        private static TargetSelection ResolveTileTargets(
            TargetDefinition targetDefinition,
            AbstractEntity sourceEntity,
            PlayingState playingState)
        {
            List<Vector2Int> positions = new List<Vector2Int>();
            if (HexGridManager.Instance == null)
                return new TargetSelection(targetDefinition, null, positions);

            foreach (Vector2Int position in HexGridManager.Instance.BoardDictionary.Keys)
            {
                if (targetDefinition.TargetType == TargetType.EmptyTile &&
                    playingState.EntitiesOnHex(position, out List<AbstractEntity> entitiesOnHex) &&
                    entitiesOnHex.Any(entity => entity != null && entity.Health > 0))
                {
                    continue;
                }

                if (IsInRange(targetDefinition, sourceEntity, position, playingState, null))
                    positions.Add(position);
            }

            return new TargetSelection(targetDefinition, null, positions);
        }

        private static bool IsValidEntityTarget(
            TargetType targetType,
            AbstractEntity sourceEntity,
            AbstractEntity candidate,
            PlayingState playingState)
        {
            if (candidate == null || candidate.Health <= 0 || candidate == sourceEntity)
                return false;

            switch (targetType)
            {
                case TargetType.AnyEnemy:
                case TargetType.EveryEnemy:
                    return playingState.IsPlayerAttackTarget(candidate);
                case TargetType.AnyEntity:
                    return true;
                default:
                    return false;
            }
        }

        private static bool IsInRange(
            TargetDefinition targetDefinition,
            AbstractEntity sourceEntity,
            Vector2Int targetPosition,
            PlayingState playingState,
            AbstractEntity candidateTarget)
        {
            if (!targetDefinition.MaxRange.HasValue)
                return true;

            if (HexGridManager.Instance == null || sourceEntity == null)
                return false;

            List<Vector2Int> blockers = new List<Vector2Int>();
            foreach (AbstractEntity entity in playingState.GetEntities())
            {
                if (entity == null ||
                    entity == sourceEntity ||
                    entity == candidateTarget ||
                    entity.Health <= 0)
                {
                    continue;
                }

                blockers.Add(entity.positionRowCol);
            }

            Dictionary<Vector2Int, int> distanceMap =
                HexGridManager.Instance.CalculateDistanceMap(sourceEntity.positionRowCol, blockers);

            return distanceMap.TryGetValue(targetPosition, out int distance) &&
                   distance >= 0 &&
                   distance <= targetDefinition.MaxRange.Value;
        }
    }
}
