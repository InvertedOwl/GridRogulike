using Cards;
using Cards.Actions;
using Entities;
using Grid;
using Map;
using Serializer;
using StateManager;
using Types.CardModifiers;
using Types.CardModifiers.Conditions;
using Types.CardModifiers.Modifiers;
using Types.ShopActions;
using UnityEngine;
using Util;

public class StaticResetManager : MonoBehaviour
{
    private void Awake()
    {
        BattleStats.ResetStatics();
    }

    public static void ResetRunStatics()
    {
        RunInfo.ResetRunDefaults();
        SaveFile.ResetStatics();
        GameState.ResetStatics();
        PlayingState.ResetStatics();
        MapState.ResetStatics();
        MapManager.ResetStatics();
        HexGridManager.ResetStatics();
        HexClickPlayerController.ResetStatics();
        HexPreviewHandler.ResetStatics();
        BattleStats.ResetStatics();

        Deck.ResetStatics();
        Player.ResetStatics();
        Card.ResetStatics();
        AbstractAction.ResetStatics();
        AbstractEntity.ResetStatics();
        MapNode.ResetStatics();
        AbstractCardCondition.ResetStatics();
        AbstractCardModifier.ResetStatics();
        CardConditionsData.ResetStatics();
        CardModifiersData.ResetStatics();
        ShopActionData.ResetStatics();
    }
}
