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
                ["<p1oison>"] = "<sprite name=\"droplets\"> ",
                ["<s1hield>"] = "<sprite name=\"shield\"> ",
                ["<draw>"] = "<sprite name=\"draw\"> ",
                ["<energy>"] = "<sprite name=\"EnergyIcon\"> ",
            };

    }
}