using System;
using System.Collections.Generic;
using System.Linq;
using Cards;
using Cards.Actions;
using Types.CardModifiers;
using Types.CardModifiers.Conditions;
using UnityEngine;
using Types;
using Types.CardModifiers.Modifiers;

namespace Cards
{
    public struct Card
    {
        public readonly List<AbstractAction> Actions;
        public AbstractCardCondition Condition;
        public AbstractCardModifier Modifier;
        public readonly string CardName;
        public readonly Rarity Rarity;
        public readonly string UniqueId;

        public readonly bool isReal;

        public int Cost =>
            (int)Mathf.Round(Actions.Sum(action => action.Cost) * 0.75f);

        public Card(bool isReal)
        {
            this.isReal = isReal;
            Actions = new List<AbstractAction>();
            Condition = null;
            Modifier = null;
            CardName = null;
            Rarity = Rarity.Common;
            UniqueId = "";
        }

        public Card(Card card)
        {
            Actions = new List<AbstractAction>(card.Actions);
            CardName = card.CardName;
            Rarity = card.Rarity;
            UniqueId = card.UniqueId;
            isReal = card.isReal;
            Condition = card.Condition;
            Modifier = card.Modifier;
        }

        public void RandomizeModifiers()
        {
            Condition = (AbstractCardCondition) Activator.CreateInstance(CardConditionsData.GetRandomCondition().ConditionType);
            Modifier = (AbstractCardModifier) Activator.CreateInstance(CardModifiersData.GetRandomModifier(Rarity).ModifierType);
            
        }

        public Card(string cardName, List<AbstractAction> actions, Rarity rarity)
        {
            Actions = actions;
            CardName = cardName;
            Rarity = rarity;
            UniqueId = Guid.NewGuid().ToString();
            isReal = true;
            Condition = (AbstractCardCondition) Activator.CreateInstance(CardConditionsData.GetRandomCondition().ConditionType);
            Modifier = (AbstractCardModifier) Activator.CreateInstance(CardModifiersData.GetRandomModifier(rarity).ModifierType);
        }
    }
}

