using System;
using System.Collections.Generic;
using System.Linq;
using Cards;
using Cards.CardList;
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
        public const int CurrentVersion = 2;

        public int version = CurrentVersion;
        public List<CardSaveData> deck;
        public RunInfoSaveData runInfo;
        public PlayingStateSaveData stateData;
        public PlayerSaveData player;
        public MapSaveData mapData;
        public MapData boardData;
        public string stateId;
        public string currentGameState;

        private static JsonSerializerSettings settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            Formatting = Formatting.None,
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            PreserveReferencesHandling = PreserveReferencesHandling.All,
            NullValueHandling = NullValueHandling.Ignore
        };

        public static string currentJSON;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ResetStaticsOnLoad()
        {
            ResetStatics();
        }

        public static void ResetStatics()
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
                version = CurrentVersion,
                deck = Deck.Instance.Cards.Select(CardSaveData.FromCard).ToList(),
                runInfo = RunInfo.Instance.CaptureSaveData(),
                stateId = GameStateManager.Instance.GetCurrentStateId(),
                stateData = stateData,
                player = Player.Instance.CaptureSaveData(),
                mapData = MapState.Instance.GetSaveData(),
                boardData = HexGridManager.Instance?.CaptureSaveData()
            };

            currentJSON = JsonConvert.SerializeObject(saveFile, settings);
            return currentJSON;
        }

        public static bool TryValidateJSON(string json, out string error)
        {
            return TryDeserializeAndValidate(json, out _, out error);
        }

        public static Type FromJSON(string json)
        {
            if (!TryDeserializeAndValidate(json, out SaveFile saveFile, out string error))
            {
                Debug.LogError($"Failed to load save file: {error}");
                return null;
            }

            if (saveFile.runInfo != null)
                RunInfo.Instance.RestoreFromSaveData(saveFile.runInfo);

            if (saveFile.deck != null)
            {
                Deck.Instance.Cards = new List<Card>();
                foreach (CardSaveData cardSaveData in saveFile.deck)
                {
                    if (cardSaveData != null && cardSaveData.TryCreateCard(out Card card))
                        Deck.Instance.Cards.Add(card);
                }
            }

            if (saveFile.player != null)
                Player.Instance.RestoreFromSaveData(saveFile.player);

            currentJSON = JsonConvert.SerializeObject(saveFile, settings);
            GameState.SaveData = saveFile.stateData;
            MapState.mapSaveData = saveFile.mapData;
            HexGridManager.LoadFromSaveData(saveFile.boardData);

            GameStateManager.TryGetStateType(saveFile.stateId, out Type stateType);
            Debug.Log("Loading state " + saveFile.stateId);
            return stateType;
        }

        private static bool TryDeserializeAndValidate(string json, out SaveFile saveFile, out string error)
        {
            saveFile = null;
            error = null;

            if (string.IsNullOrWhiteSpace(json))
            {
                error = "Save file is empty or whitespace.";
                return false;
            }

            try
            {
                saveFile = JsonConvert.DeserializeObject<SaveFile>(json, settings);
            }
            catch (Exception ex)
            {
                error = $"Save file JSON could not be deserialized: {ex.Message}";
                return false;
            }

            if (saveFile == null)
            {
                error = "Save file deserialized to null.";
                return false;
            }

            Migrate(saveFile);
            return Validate(saveFile, out error);
        }

        private static void Migrate(SaveFile saveFile)
        {
            if (saveFile.version <= 0)
            {
                saveFile.version = 1;
            }

            if (string.IsNullOrWhiteSpace(saveFile.stateId) &&
                GameStateManager.TryGetStateIdFromLegacyTypeName(saveFile.currentGameState, out string migratedStateId))
            {
                saveFile.stateId = migratedStateId;
            }

            if (saveFile.version < CurrentVersion)
            {
                saveFile.version = CurrentVersion;
            }

            saveFile.currentGameState = null;
        }

        private static bool Validate(SaveFile saveFile, out string error)
        {
            error = null;

            if (saveFile.version > CurrentVersion)
            {
                error = $"Save version {saveFile.version} is newer than supported version {CurrentVersion}.";
                return false;
            }

            if (!GameStateManager.TryGetStateType(saveFile.stateId, out _))
            {
                error = $"Save file has unknown state id '{saveFile.stateId}'.";
                return false;
            }

            if (saveFile.runInfo == null)
            {
                error = "Save file is missing run info.";
                return false;
            }

            if (saveFile.player == null)
            {
                error = "Save file is missing player data.";
                return false;
            }

            if (saveFile.deck == null)
            {
                error = "Save file is missing deck data.";
                return false;
            }

            foreach (CardSaveData card in saveFile.deck)
            {
                if (card == null)
                {
                    error = "Save file contains a null card entry.";
                    return false;
                }

                if (string.IsNullOrEmpty(card.definitionId) ||
                    !CardDefinitionRegistry.TryGetDefinition(card.definitionId, out _))
                {
                    error = $"Save file references unknown card definition '{card.definitionId}'.";
                    return false;
                }
            }

            return true;
        }
    }
}
