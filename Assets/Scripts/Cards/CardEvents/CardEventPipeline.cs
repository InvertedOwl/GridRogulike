using System.Collections.Generic;
using Cards;
using Cards.CardStatuses;
using Entities;
using Grid;
using Passives;
using StateManager;
using Types.Statuses;
using Types.Tiles;

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
    }
}
