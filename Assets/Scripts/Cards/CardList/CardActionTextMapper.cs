using System.Collections.Generic;
using UnityEngine;

namespace Cards.CardList
{
    public class CardActionTextMapper : MonoBehaviour
    {
        public const string MONEY_ICON = "<money>";

        public static readonly IReadOnlyDictionary<string, string> icons =
            new Dictionary<string, string>
            {
                ["<attack>"] = "<sprite name=\"Damage4\"> ",
                ["<move>"] = "<sprite name=\"footsteps\"> ",
                ["<poison>"] = "<sprite name=\"droplets\"> ",
                ["<shield>"] = "<sprite name=\"shield\"> ",
                ["<draw>"] = "<sprite name=\"draw\"> ",
                ["<energy>"] = "<sprite name=\"EnergyIcon\"> ",
                ["<frost>"] = "<sprite name=\"Snowflake_1\">",
                ["<arrow>"] = "<sprite name=\"BowAndArrow\">"
            };

    }
}