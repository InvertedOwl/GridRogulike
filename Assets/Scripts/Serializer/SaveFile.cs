using System;
using System.Collections.Generic;
using Cards;
using Entities;
using Grid;
using Map;
using StateManager;
using Newtonsoft.Json;
using UnityEngine;

namespace Serializer
{
    [System.Serializable]
    public class SaveFile
    {
        public List<Card> deck;
        public RunInfoSaveData runInfo;
        public PlayingStateSaveData stateData;
        public PlayerSaveData player;
        public MapSaveData mapData;
        public MapData boardData;
        public string currentGameState;

        private static JsonSerializerSettings settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            Formatting = Formatting.Indented,
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            PreserveReferencesHandling = PreserveReferencesHandling.All
        };

        public static string currentJSON;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ResetStaticsOnLoad()
        {
            currentJSON = null;
        }

        public static string ToJSON()
        {
            var currentState = GameStateManager.Instance.GetCurrent();
            PlayingStateSaveData stateData = null;

            if (currentState is PlayingState playingState)
            {
                stateData = playingState.CaptureSaveData();
            }

            SaveFile saveFile = new SaveFile
            {
                deck = Deck.Instance.Cards,
                runInfo = RunInfo.Instance.CaptureSaveData(),
                currentGameState = GameStateManager.Instance.GetCurrentStateType()?.FullName,
                stateData = stateData,
                player = Player.Instance.CaptureSaveData(),
                mapData = MapState.Instance.GetSaveData(),
                boardData = HexGridManager.Instance?.CaptureSaveData()
            };

            currentJSON = JsonConvert.SerializeObject(saveFile, settings);
            return currentJSON;
        }

        public static Type FromJSON(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                Debug.LogWarning("Save file is empty or whitespace.");
                return null;
            }

            SaveFile saveFile;
            try
            {
                saveFile = JsonConvert.DeserializeObject<SaveFile>(json, settings);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to deserialize save file: {ex.Message}");
                return null;
            }

            if (saveFile == null)
            {
                Debug.LogWarning("Save file deserialized to null.");
                return null;
            }

            if (saveFile.player != null)
                Player.Instance.RestoreFromSaveData(saveFile.player);

            if (saveFile.deck != null)
                Deck.Instance.Cards = saveFile.deck;

            if (saveFile.runInfo != null)
                RunInfo.Instance.RestoreFromSaveData(saveFile.runInfo);

            currentJSON = json;
            GameState.SaveData = saveFile.stateData;
            MapState.mapSaveData = saveFile.mapData;
            HexGridManager.LoadFromSaveData(saveFile.boardData);

            Type stateType = null;
            if (!string.IsNullOrEmpty(saveFile.currentGameState))
            {
                Debug.Log("Loading state " + saveFile.currentGameState);
                stateType = Type.GetType(saveFile.currentGameState);

                if (stateType == null)
                {
                    Debug.LogError($"Could not resolve game state type: {saveFile.currentGameState}");
                }
            }

            return stateType;
        }
    }
}
