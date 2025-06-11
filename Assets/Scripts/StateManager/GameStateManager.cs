using System;
using System.Collections.Generic;
using UnityEngine;

namespace StateManager
{
    public class GameStateManager : MonoBehaviour
    {
        public static GameStateManager Instance { get; private set; } = null!;

        private readonly Dictionary<Type, GameState> _states = new();
        private GameState _current;
        public T GetCurrent<T>() where T : GameState => _current as T;
        public bool IsCurrent<T>() where T : GameState => _current is T;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            foreach (var s in GetComponentsInChildren<GameState>(true))
            {
                _states[s.GetType()] = s;
                s.enabled = false;
            }
            
        }

        public void Start()
        {
            // DEBUG, REMOVE LATER WHEN MAKING MENUS
            Deck.Instance.StartGame();
            Change<PlayingState>();
        }

        public void Change<T>() where T : GameState
        {
            if (_current is T) return;

            _current?.Exit();
            if (_current) _current.enabled = false;

            _current = _states[typeof(T)];
            _current.enabled = true;
            _current.Enter();
        }
        public T GetState<T>() where T : GameState =>
            _states.TryGetValue(typeof(T), out var s) ? (T)s : null;
    }
}