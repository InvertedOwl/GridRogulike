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
using Types.ShopActions;
using UnityEngine.UI;
using Random = System.Random;

namespace StateManager
{
    public class EventState : GameState
    {
        public GameObject window;

        private Random _eventRandom;

        public void Awake()
        {
            _eventRandom = RunInfo.NewRandom("events".GetHashCode());
        }

        public override void Enter()
        {
            window.GetComponent<EasePosition>().SendToLocation(new Vector2(0, 0));
        }

        public override void Exit()
        {
            window.GetComponent<EasePosition>().SendToLocation(new Vector2(0, 730));
        }
    }
}
