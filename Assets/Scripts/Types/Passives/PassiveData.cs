using System;
using System.Collections.Generic;
using Cards.CardEvents;
using Passives;
using UnityEngine;
using Util;

namespace Types.Passives
{
    public class PassiveData
    {
        private static readonly IReadOnlyDictionary<string, Func<PassiveEntry>> defs =
            new Dictionary<string, Func<PassiveEntry>>
            {
                ["Forest"] = () => new("Forest",
                    "Spring forth trees.",
                    HexColorUtility.HexToColor("#142e15"),
                    (events, card, context) =>
                    {
                        return events;
                    },
                    new List<string>
                    {
                        "Tree"
                    },
                    new List<PassiveEntitySpawn>
                    {
                        new PassiveEntitySpawn("Rock", 6)
                    }),

            };
        
        public static PassiveEntry GetPassiveEntry(string name)
        {
            return defs[name]();
        }

        public static bool TryGetPassiveEntry(string name, out PassiveEntry entry)
        {
            entry = null;

            if (string.IsNullOrWhiteSpace(name))
                return false;

            if (!defs.TryGetValue(name, out Func<PassiveEntry> factory))
                return false;

            entry = factory();
            return true;
        }
    }
}
