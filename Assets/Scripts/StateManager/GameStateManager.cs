using System;
using System.Collections.Generic;
using System.Linq;
using Serializer;
using UnityEngine;

namespace StateManager
{
    public class GameStateManager : MonoBehaviour
    {
        public static GameStateManager Instance { get; private set; } = null!;

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

        public void Awake ()
        {
            Instance = this;
            _randomPitchSound = GetWindowSound();

            foreach (var s in GetComponentsInChildren<GameState>(true))
            {
                _states[s.GetType()] = s;
                s.enabled = false;
            }
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
