using System;
using System.Collections.Generic;
using Cards;
using Cards.CardEvents;
using UnityEngine;

namespace Passives
{
    public class PassiveEntry
    {
		public string Name;
        public string Desc;
        public Color Color;
        public List<string> decor;
        public List<PassiveEntitySpawn> EntitySpawns;
        public Func<List<AbstractCardEvent>, Card?, PassiveContext, List<AbstractCardEvent>> ModifyEvents;

        public PassiveEntry(
            string name,
            string desc,
            Color color,
            Func<List<AbstractCardEvent>, Card?, PassiveContext, List<AbstractCardEvent>> modifyEvents,
            List<string> decor,
            List<PassiveEntitySpawn> entitySpawns = null)
        {
            this.Name = name;
            this.Desc = desc;
            this.Color = color;
            this.ModifyEvents = modifyEvents ?? ((events, card, context) => events);
            this.decor = decor;
            this.EntitySpawns = entitySpawns ?? new List<PassiveEntitySpawn>();
        }
    }
}
