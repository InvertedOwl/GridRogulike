using System;
using System.Collections.Generic;
using System.Linq;
using Types.Tiles;
using UnityEngine;

namespace Types.Events
{
    public class EventData
    {
        public static readonly Dictionary<string, EventEntry> Events = new Dictionary<string, EventEntry>()
        {
            ["TestEvent"] = new  EventEntry("Test Event", "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Aenean iaculis pharetra luctus. Mauris vitae neque velit.", new List<Action>()
            {
                () =>
                {
                    Debug.Log("Button 2 pushed");
                },
                () =>
                {
                    Debug.Log("Button 1 pushed");
                },

            }, new List<string>()
            {
                "Button 2",
                "Button 1",
            })
        };

        public static EventEntry GetRandomEvent(System.Random random)
        {
            return Events[Events.Keys.ToList()[random.Next(Events.Count)]];
        }
    }
}