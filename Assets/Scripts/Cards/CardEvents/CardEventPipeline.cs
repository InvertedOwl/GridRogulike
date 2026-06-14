using System.Collections.Generic;
using Cards;
using Cards.CardStatuses;
using Entities;
using Grid;
using Passives;
using StateManager;
using Types.Statuses;
using Types.Tiles;
using UnityEngine;

namespace Cards.CardEvents
{
    public static class CardEventPipeline
    {
        public static List<AbstractCardEvent> Apply(
            List<AbstractCardEvent> eventQueue,
            AbstractEntity sourceEntity,
            Card? card = null,
            CardStatusDatabase.CardStatus cardStatus = null,
            bool previewMode = false)
        {
            List<AbstractCardEvent> modifiedEvents = new List<AbstractCardEvent>(eventQueue);

            if (card.HasValue && cardStatus != null && cardStatus.ModifyPlay != null)
            {
                modifiedEvents = cardStatus.ModifyPlay(modifiedEvents, card.Value);
            }

            if (sourceEntity is Player)
            {
                modifiedEvents = ApplyEnvironmentModifiers(modifiedEvents, card, previewMode);
            }

            if (sourceEntity?.statusManager != null)
            {
                foreach (AbstractStatus status in sourceEntity.statusManager.statusList)
                {
                    modifiedEvents = status.Modify(modifiedEvents, previewMode);
                }
            }

            if (sourceEntity is Player && HexGridManager.Instance != null)
            {
                string tileId = HexGridManager.Instance.HexType(sourceEntity.positionRowCol);
                TileEntry tile = TileData.tiles[tileId];
                TileContext tileContext = new TileContext(
                    sourceEntity.positionRowCol,
                    tileId,
                    sourceEntity,
                    previewMode);
                PlayingState playingState = GameStateManager.Instance.GetCurrent<PlayingState>();
                if (playingState.CanTriggerTile(sourceEntity.positionRowCol, tile))
                {
                    if (card != null)
                    {
                        modifiedEvents = tile.cardModifier.Invoke(modifiedEvents, card.Value, tileContext);
                        if (!previewMode &&
                            tile.shouldMarkAsTriggered != null &&
                            tile.shouldMarkAsTriggered.Invoke(modifiedEvents, card.Value, tileContext))
                        {
                            playingState.MarkTileTriggered(sourceEntity.positionRowCol, tile);     
                        }
                    }

                }
            }

            if (sourceEntity != null &&
                sourceEntity.entityType == EntityType.Enemy &&
                GameStateManager.Instance != null &&
                GameStateManager.Instance.IsCurrent<PlayingState>())
            {
                modifiedEvents = GameStateManager.Instance
                    .GetCurrent<PlayingState>()
                    .ApplyEnemyDamageScaling(modifiedEvents);
            }

            modifiedEvents = ApplyIncomingTileModifiers(modifiedEvents, sourceEntity, previewMode);

            return modifiedEvents;
        }

        public static void Activate(
            List<AbstractCardEvent> eventQueue,
            AbstractEntity sourceEntity,
            Card? card = null,
            CardStatusDatabase.CardStatus cardStatus = null)
        {
            foreach (AbstractCardEvent cardEvent in Apply(eventQueue, sourceEntity, card, cardStatus))
            {
                cardEvent.Activate(sourceEntity);
            }
        }

        private static List<AbstractCardEvent> ApplyEnvironmentModifiers(
            List<AbstractCardEvent> eventQueue,
            Card? card,
            bool previewMode)
        {
            if (EnvironmentManager.instance == null)
            {
                return eventQueue;
            }

            foreach (PassiveEntry entry in EnvironmentManager.instance.GetPassiveEntries())
            {
                if (previewMode && !entry.Condition.CanPreview)
                {
                    continue;
                }

                if (entry.Condition.Condition(card.GetValueOrDefault(), eventQueue))
                {
                    eventQueue = entry.CardModifier.Modify(eventQueue, previewMode);
                }
            }

            return eventQueue;
        }

        private static List<AbstractCardEvent> ApplyIncomingTileModifiers(
            List<AbstractCardEvent> eventQueue,
            AbstractEntity sourceEntity,
            bool previewMode)
        {
            List<AbstractCardEvent> modifiedEvents = new List<AbstractCardEvent>();

            foreach (AbstractCardEvent cardEvent in eventQueue)
            {
                if (TryGetIncomingTarget(cardEvent, sourceEntity, out Vector2Int targetPosition))
                {
                    modifiedEvents.AddRange(ApplyIncomingTileModifiersForTarget(
                        new List<AbstractCardEvent> { cardEvent },
                        sourceEntity,
                        targetPosition,
                        previewMode));
                }
                else
                {
                    modifiedEvents.Add(cardEvent);
                }
            }

            return modifiedEvents;
        }

        public static List<AbstractCardEvent> ApplyIncomingTileModifiersForTarget(
            List<AbstractCardEvent> eventQueue,
            AbstractEntity sourceEntity,
            Vector2Int targetPosition,
            bool previewMode = false)
        {
            if (eventQueue == null ||
                HexGridManager.Instance == null ||
                GameStateManager.Instance == null ||
                !GameStateManager.Instance.IsCurrent<PlayingState>())
            {
                return eventQueue;
            }

            string tileId = HexGridManager.Instance.HexType(targetPosition);
            if (!TileData.tiles.TryGetValue(tileId, out TileEntry tile) || tile.incomingEventModifier == null)
                return eventQueue;

            AbstractEntity targetEntity = null;
            PlayingState playingState = GameStateManager.Instance.GetCurrent<PlayingState>();
            if (playingState.EntitiesOnHex(targetPosition, out List<AbstractEntity> entities) && entities.Count > 0)
            {
                targetEntity = entities[0];
            }

            TileContext tileContext = new TileContext(targetPosition, tileId, targetEntity, previewMode);
            return tile.incomingEventModifier.Invoke(eventQueue, tileContext) ?? eventQueue;
        }

        private static bool TryGetIncomingTarget(
            AbstractCardEvent cardEvent,
            AbstractEntity sourceEntity,
            out Vector2Int targetPosition)
        {
            targetPosition = default;

            if (cardEvent is not AttackCardEvent attackCardEvent)
                return false;

            if (attackCardEvent.usePosition)
            {
                targetPosition = attackCardEvent.position;
                return true;
            }

            if (sourceEntity is Player && attackCardEvent.manual)
                return false;

            if (sourceEntity == null || string.IsNullOrEmpty(attackCardEvent.direction))
                return false;

            targetPosition = HexGridManager.MoveHex(
                sourceEntity.positionRowCol,
                attackCardEvent.direction,
                attackCardEvent.distance);
            return true;
        }
    }
}
