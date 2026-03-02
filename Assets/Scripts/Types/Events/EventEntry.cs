using System;
using System.Collections.Generic;

namespace Types.Events
{
    public class EventEntry
    {
        public string Name;
        public string Desc;
        public List<Action> Actions;
        public List<string> ButtonsTexts;

        public EventEntry(string name, string desc, List<Action> actions, List<string> buttonsTexts)
        {
            this.Name = name;
            this.Desc = desc;
            this.Actions = actions;
            this.ButtonsTexts = buttonsTexts;
        }
    }
}