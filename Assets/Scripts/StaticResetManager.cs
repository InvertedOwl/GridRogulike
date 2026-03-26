using Grid;
using Map;
using StateManager;
using UnityEngine;
using Util;

public class StaticResetManager : MonoBehaviour
{
    private void Awake()
    {
        BattleStats.ResetStatics();
    }
}