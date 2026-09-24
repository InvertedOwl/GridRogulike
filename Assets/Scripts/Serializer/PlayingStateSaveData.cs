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

    [Serializable]
    public class BattleTileSaveData
    {
        public Vector2Int position;
        public string definitionId;
        public int ownerEntityIndex;
        public int power;
        public int remainingTriggers;
    }

    public class PlayingStateSaveData
    {
        public EncounterData encounterData;
        public int rewardMoney;
        public int mapProgressLayer;
        public int mapProgressLayerCount;
        public List<TileCountdownSaveData> tileCountdownStates = new();
        public List<BattleTileSaveData> battleTiles = new();
    }
}
