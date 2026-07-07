using System;
using System.Collections.Generic;
using System.Linq;
using Serializer;
using UnityEngine;

namespace StateManager
{
    public class GameStateManager : MonoBehaviour
    {
        private static readonly Dictionary<Type, string> StateIdsByType = new()
        {
            [typeof(MapState)] = "map",
            [typeof(PlayingState)] = "playing",
            [typeof(TilePickState)] = "tile_pick",
            [typeof(ShopState)] = "shop",
            [typeof(CampfireState)] = "campfire",
            [typeof(EventState)] = "event",
            [typeof(GameOverState)] = "game_over",
            [typeof(GameFinishState)] = "game_finish"
        };

        private static readonly Dictionary<string, Type> StateTypesById =
            StateIdsByType.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

        public static GameStateManager Instance { get; private set; } = null!;

        public static bool GameLoaded = false;

        private readonly Dictionary<Type, GameState> _states = new();
        private GameState _current;
        private RandomPitchSound _randomPitchSound;

        [Header("Window Sounds")]
        [SerializeField] private bool playWindowSounds = true;
        [SerializeField] private string windowInSoundKey = "window_in";
        [SerializeField] private string windowOutSoundKey = "window_out";

        public T GetCurrent<T>() where T : GameState => _current as T;

        public GameState GetCurrent()
        {
            return _current;
        }
        public bool IsCurrent<T>() where T : GameState => _current is T;
        public Type GetCurrentStateType()
        {
            return _current?.GetType();
        }

        public string GetCurrentStateId()
        {
            return TryGetStateId(GetCurrentStateType(), out string stateId) ? stateId : null;
        }

        public static bool TryGetStateId(Type stateType, out string stateId)
        {
            if (stateType == null)
            {
                stateId = null;
                return false;
            }

            return StateIdsByType.TryGetValue(stateType, out stateId);
        }

        public static bool TryGetStateType(string stateId, out Type stateType)
        {
            if (string.IsNullOrWhiteSpace(stateId))
            {
                stateType = null;
                return false;
            }

            return StateTypesById.TryGetValue(stateId, out stateType);
        }

        public static bool TryGetStateIdFromLegacyTypeName(string legacyTypeName, out string stateId)
        {
            stateId = null;
            if (string.IsNullOrWhiteSpace(legacyTypeName))
                return false;

            foreach (KeyValuePair<Type, string> kvp in StateIdsByType)
            {
                Type stateType = kvp.Key;
                if (legacyTypeName == stateType.FullName ||
                    legacyTypeName == stateType.AssemblyQualifiedName ||
                    legacyTypeName == stateType.Name)
                {
                    stateId = kvp.Value;
                    return true;
                }
            }

            Type resolvedType = Type.GetType(legacyTypeName);
            return resolvedType != null && TryGetStateId(resolvedType, out stateId);
        }

        public void Awake ()
        {
            Instance = this;
            _randomPitchSound = GetWindowSound();

            foreach (var s in GetComponentsInChildren<GameState>(true))
            {
                _states[s.GetType()] = s;
                s.enabled = false;
            }
            GameLoaded = true;
        }

        public void Change(Type stateType)
        {
            Debug.Log("attempting change " + stateType.FullName + " with current " + _current?.GetType().FullName);
            
            _current?.Exit();
            if (_current) _current.enabled = false;

            _current = _states[stateType];
            _current.enabled = true;
            _current.Enter();
            Debug.Log("Entered game state " + stateType.FullName);
        }
        
        void Start()
        {

            
            // DEBUG, REMOVE LATER WHEN MAKING MENUS
            Debug.Log("Enter Game StateManager");
            if (_current == null)
            {
                Change<MapState>();
            }
            Deck.Instance.StartGame();
            
        }
        
        public void Change<T>() where T : GameState
        {
            if (_current is T) return;

            _current?.Exit();
            if (_current) _current.enabled = false;

            _current = _states[typeof(T)];
            _current.enabled = true;
            SaveFile.currentJSON = SaveFile.ToJSON();
            SaveGameObject.PersistCheckpoint();
            _current.Enter();
        }
        public T GetState<T>() where T : GameState =>
            _states.TryGetValue(typeof(T), out var s) ? (T)s : null;

        public void PlayWindowInSound()
        {
            PlayWindowSound(windowInSoundKey);
        }

        public void PlayWindowOutSound()
        {
            PlayWindowSound(windowOutSoundKey);
        }

        private void PlayWindowSound(string soundKey)
        {
            if (!playWindowSounds || string.IsNullOrEmpty(soundKey))
                return;

            if (_randomPitchSound == null)
                _randomPitchSound = GetWindowSound();

            if (_randomPitchSound == null)
                return;

            _randomPitchSound.PlaySound(soundKey);
        }

        private RandomPitchSound GetWindowSound()
        {
            RandomPitchSound sound = GetComponent<RandomPitchSound>();
            if (sound != null)
                return sound;

            return GetComponentInChildren<RandomPitchSound>(true);
        }
        
    }
}
