using System;
using System.Collections.Generic;
using Cards;
using Cards.CardEvents;

namespace Types.Tiles
{
    public class PairedTilesEntry
    {
        public TileSetEnum TileSet;
        public int PairCount;
        public Dictionary<TriggerEventTime, Func<TileContext, List<AbstractCardEvent>>> triggerEvents;
        public Func<List<AbstractCardEvent>, Card, TileContext, List<AbstractCardEvent>> cardModifier;
        public Func<List<AbstractCardEvent>, TileContext, List<AbstractCardEvent>> incomingEventModifier;
        public string description;
        
        public PairedTilesEntry(TileSetEnum tileSet, int pairCount,  
            Dictionary<TriggerEventTime, Func<TileContext, List<AbstractCardEvent>>> triggerEvents, 
            Func<List<AbstractCardEvent>, Card, TileContext, List<AbstractCardEvent>> cardModifier,
            Func<List<AbstractCardEvent>, TileContext, List<AbstractCardEvent>> incomingEventModifier,
            string description)
        {
            TileSet = tileSet;
            PairCount = pairCount;
            this.triggerEvents = triggerEvents;
            this.cardModifier = cardModifier;
            this.incomingEventModifier = incomingEventModifier;
            this.description = description;
        }
        
    }
}