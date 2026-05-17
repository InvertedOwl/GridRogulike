using System.Collections.Generic;
using Cards;
using Cards.CardStatuses;
using Entities;
using Grid;
using Passives;
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
                TileEntry tile = TileData.tiles[HexGridManager.Instance.HexType(sourceEntity.positionRowCol)];
                modifiedEvents = tile.cardModifier.Invoke(modifiedEvents);
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
