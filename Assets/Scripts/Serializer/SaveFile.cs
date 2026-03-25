using System;
using System.Collections.Generic;
using Cards;
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
        public Dictionary<int, RandomState> randoms;

        // Run Info
        public int MaxEnergy;
        public int Difficulty;

        public string currentGameState;

        private static JsonSerializerSettings settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            Formatting = Formatting.Indented,
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            PreserveReferencesHandling = PreserveReferencesHandling.All
        };

        public static string currentJSON;

        public static string ToJSON()
        {
            SaveFile saveFile = new SaveFile
            {
                deck = Deck.Instance.Cards,
                randoms = RunInfo.randoms,
                MaxEnergy = RunInfo.Instance.MaxEnergy,
                Difficulty = RunInfo.Instance.Difficulty,
                currentGameState = GameStateManager.Instance.GetCurrentStateType().FullName
            };

            return JsonConvert.SerializeObject(saveFile, settings);
        }

        public static void FromJSON(string json)
        {
            SaveFile saveFile = JsonConvert.DeserializeObject<SaveFile>(json, settings);

            if (saveFile.deck != null)
                Deck.Instance.Cards = saveFile.deck;
            if (saveFile.randoms != null)
                RunInfo.randoms = saveFile.randoms;
            foreach (KeyValuePair<int, RandomState> keyValuePair in RunInfo.randoms)
            {
                keyValuePair.Value.RebuildRandom();
            }

            if (!string.IsNullOrEmpty(saveFile.currentGameState))
            {
                Debug.Log("Loading state " + saveFile.currentGameState);
                Type stateType = Type.GetType(saveFile.currentGameState);
                if (stateType != null)
                {
                    GameStateManager.Instance.Change(stateType);
                }
                else
                {
                    Debug.LogError($"Could not resolve game state type: {saveFile.currentGameState}");
                }
            }
            // MapState.Instance.currentNode = saveFile.currentNode;
            // MapState.Instance.mapLayers = saveFile.mapLayers;
            //
            RunInfo.Instance.MaxEnergy = saveFile.MaxEnergy;
            // RunInfo.Instance.Di
            RunInfo.Instance.Difficulty = saveFile.Difficulty;
        }
    }
}