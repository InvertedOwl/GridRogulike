using System;
using System.Collections.Generic;

namespace Util
{
    public static class BattleStats
    {
        public static int MoneyGainedThisBattle;
        public static int MoneyGainedThisTurn;
        public static int CardsPlayedThisBattle;
        public static int CardsPlayedThisTurn;
        public static int DamageDoneThisBattle;
        public static int DamageDoneThisTurn;
        public static int ShieldDoneThisBattle;
        public static int ShieldDoneThisTurn;
        public static int HealDoneThisBattle;
        public static int HealDoneThisTurn;
        public static int TilesMovedThisBattle;
        public static int TilesMovedThisTurn;

        public static Dictionary<string, Func<string>> names = new Dictionary<string, Func<string>>
        {
            { "$moneybattle$", () => "<color=#bfbfbf>(" + MoneyGainedThisBattle + ")</color>" },
            { "$moneyturn$", () => "<color=#bfbfbf>(" + MoneyGainedThisTurn + ")</color>" },

            { "$cardbattle$", () => "<color=#bfbfbf>(" + CardsPlayedThisBattle + ")</color>" },
            { "$cardturn$", () => "<color=#bfbfbf>(" + CardsPlayedThisTurn + ")</color>" },

            { "$damagebattle$", () => "<color=#bfbfbf>(" + DamageDoneThisBattle + ")</color>" },
            { "$damageturn$", () => "<color=#bfbfbf>(" + DamageDoneThisTurn + ")</color>" },

            { "$shieldbattle$", () => "<color=#bfbfbf>(" + ShieldDoneThisBattle + ")</color>" },
            { "$shieldturn$", () => "<color=#bfbfbf>(" + ShieldDoneThisTurn + ")</color>" },

            { "$healbattle$", () => "<color=#bfbfbf>(" + HealDoneThisBattle + ")</color>" },
            { "$healturn$", () => "<color=#bfbfbf>(" + HealDoneThisTurn + ")</color>" },

            { "$tilebattle$", () => "<color=#bfbfbf>(" + TilesMovedThisBattle + ")</color>" },
            { "$tileturn$", () => "<color=#bfbfbf>(" + TilesMovedThisTurn + ")</color>" },
        };
        
        
        
        public static void ResetStatsBattle()
        {
            MoneyGainedThisBattle = 0;
            CardsPlayedThisBattle = 0;
            TilesMovedThisBattle = 0;
            DamageDoneThisBattle = 0;
            HealDoneThisBattle = 0;
            ShieldDoneThisBattle = 0;
            ResetStatsTurn();
        }
        
        public static void ResetStatsTurn()
        {
            MoneyGainedThisTurn = 0;
            CardsPlayedThisTurn = 0;
            TilesMovedThisTurn = 0;
            DamageDoneThisTurn = 0;
            HealDoneThisTurn = 0;
            ShieldDoneThisTurn = 0;
        }
    }
}