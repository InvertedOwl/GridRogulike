using System;
using System.Collections.Generic;
using Entities.Enemies;
using UnityEngine;

namespace Serializer
{
    [Serializable]
    public class TileCountdownSaveData
    {
        public Vector2Int position;
        public int turnsRemaining;
        public bool exploded;
        public bool iconCleared;
    }

    public class PlayingStateSaveData
    {
        public EncounterData encounterData;
        public int mapProgressLayer;
        public int mapProgressLayerCount;
        public List<TileCountdownSaveData> tileCountdownStates = new();
    }
}
