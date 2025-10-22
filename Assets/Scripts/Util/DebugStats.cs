using TMPro;
using UnityEngine;

namespace Util
{
    public class DebugStats : MonoBehaviour
    {

        public TextMeshProUGUI text;
        public void Update()
        {
            text.text = "DamageDoneThisTurn: " + BattleStats.DamageDoneThisTurn + "\n";
            text.text += "CardsPlayedThisTurn: " + BattleStats.CardsPlayedThisTurn + "\n";
            text.text += "MoneyGainedThisTurn: " + BattleStats.MoneyGainedThisTurn + "\n";
            text.text += "ShieldDoneThisTurn: " + BattleStats.ShieldDoneThisTurn + "\n";
            text.text += "HealDoneThisTurn: " + BattleStats.HealDoneThisTurn + "\n";
            text.text += "TilesMovedThisTurn: " + BattleStats.TilesMovedThisTurn + "\n";
            text.text += "MoneyGainedThisBattle: " + BattleStats.MoneyGainedThisBattle + "\n";
            text.text += "CardsPlayedThisBattle: " + BattleStats.CardsPlayedThisBattle + "\n";
            text.text += "DamageDoneThisBattle: " + BattleStats.DamageDoneThisBattle + "\n";
            text.text += "ShieldDoneThisBattle: " + BattleStats.ShieldDoneThisBattle + "\n";
            text.text += "HealDoneThisBattle: " + BattleStats.HealDoneThisBattle + "\n";
            text.text += "TilesMovedThisBattle: " + BattleStats.TilesMovedThisBattle + "\n";
        }
        
    }
}