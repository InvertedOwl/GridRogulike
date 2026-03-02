using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cards;
using Cards.CardList;
using TMPro;
using UnityEngine;
using Util;
using Types;
using Types.Events;
using Types.ShopActions;
using UnityEngine.UI;
using Random = System.Random;

namespace StateManager
{
    public class EventState : GameState
    {
        public GameObject window;

        private Random _eventRandom;
        
        public TextMeshProUGUI Name;
        public TextMeshProUGUI Desc;
        
        public Button[] Buttons;

        public void Awake()
        {
            _eventRandom = RunInfo.NewRandom("events".GetHashCode());
        }

        public override void Enter()
        {
            window.GetComponent<EasePosition>().SendToLocation(new Vector2(0, 0));
            
            EventEntry entry = EventData.GetRandomEvent(_eventRandom);
            Name.text = entry.Name;
            Desc.text = entry.Desc;
            for (int i = 0; i < entry.Actions.Count; i++)
            {
                Action action = entry.Actions[i];
                Buttons[Buttons.Length - i - 1].onClick.AddListener(() =>
                {
                    action.Invoke();
                    GameStateManager.Instance.Change<MapState>();
                });
                Buttons[Buttons.Length - i - 1].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text =
                    entry.ButtonsTexts[i];
            }

            foreach (Button button in Buttons)
            {
                button.gameObject.SetActive(true);
            }
            
            for (int i = 0; i < Buttons.Length - entry.Actions.Count; i++)
            {
                Buttons[i].gameObject.SetActive(false);
            }
        }

        public override void Exit()
        {
            window.GetComponent<EasePosition>().SendToLocation(new Vector2(0, 730));
        }
    }
}
