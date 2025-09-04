using System;
using System.Collections.Generic;
using System.Linq;
using Cards;
using UnityEngine;
using Types.Actions;
public readonly struct Card
{
    public readonly List<AbstractAction> Actions;
    public readonly string CardName;
    public readonly Rarity Rarity;
    public readonly string UniqueId;

    public int Cost =>
        (int)Mathf.Round(Actions.Sum(action => action.Cost) * 0.75f);

    public Card(string cardName, List<AbstractAction> actions, Rarity rarity)
    {
        Actions = actions;
        CardName = cardName;
        Rarity = rarity;
        UniqueId = Guid.NewGuid().ToString();
    }
}

