using System;
using System.Collections.Generic;
using Cards.CardEvents;
using Entities;
using StateManager;
using UnityEngine;

namespace Types.Tiles
{
    public sealed class BattleTileTriggerContext
    {
        public PlayingState State { get; }
        public BattleTileInstance Tile { get; }
        public Vector2Int Position => Tile.Position;
        public AbstractEntity Owner => Tile.Owner;
        public AbstractEntity EnteringEntity { get; }
        public int Power => Tile.Power;

        public BattleTileTriggerContext(
            PlayingState state,
            BattleTileInstance tile,
            AbstractEntity enteringEntity)
        {
            State = state;
            Tile = tile;
            EnteringEntity = enteringEntity;
        }
    }

    public sealed class BattleTileDefinition
    {
        private readonly Func<BattleTileTriggerContext, List<AbstractCardEvent>> _landEventFactory;

        public string Id { get; }
        public string Name { get; }
        public string Description { get; }
        public string Icon { get; }
        public int DefaultPower { get; }
        public int DefaultTriggerCount { get; }
        public bool RemoveWhenOwnerDies { get; }

        public BattleTileDefinition(
            string id,
            string name,
            string description,
            string icon,
            int defaultPower,
            int defaultTriggerCount,
            bool removeWhenOwnerDies,
            Func<BattleTileTriggerContext, List<AbstractCardEvent>> landEventFactory)
        {
            Id = id;
            Name = name;
            Description = description;
            Icon = icon;
            DefaultPower = defaultPower;
            DefaultTriggerCount = defaultTriggerCount;
            RemoveWhenOwnerDies = removeWhenOwnerDies;
            _landEventFactory = landEventFactory;
        }

        public List<AbstractCardEvent> CreateLandEvents(BattleTileTriggerContext context)
        {
            return _landEventFactory?.Invoke(context) ?? new List<AbstractCardEvent>();
        }
    }

    public sealed class BattleTileInstance
    {
        public string DefinitionId { get; }
        public Vector2Int Position { get; }
        public AbstractEntity Owner { get; }
        public int Power { get; }
        public int RemainingTriggers { get; private set; }
        public bool CanTrigger => RemainingTriggers != 0;

        public BattleTileInstance(
            string definitionId,
            Vector2Int position,
            AbstractEntity owner,
            int power,
            int remainingTriggers)
        {
            DefinitionId = definitionId;
            Position = position;
            Owner = owner;
            Power = power;
            RemainingTriggers = remainingTriggers;
        }

        public void ConsumeTrigger()
        {
            if (RemainingTriggers > 0)
                RemainingTriggers--;
        }
    }
}
