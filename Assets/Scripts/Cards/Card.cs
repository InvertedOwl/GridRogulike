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
using Random = System.Random;

namespace Cards
{
    public struct Card
    {
        public List<AbstractAction> Actions;
        public AbstractCardCondition Condition;
        public AbstractCardModifier Modifier;
        public string CardName;
        public Rarity Rarity;
        public string UniqueId;

        public bool isReal;

        public Random cardRandom;

        public int Cost =>
            (int)Mathf.Round(Actions.Sum(action => action.Cost) * 0.75f);

        public static Random guidRandom = RunInfo.NewRandom("guid".GetHashCode());
        public static string GenerateDeterministicId()
        {
            byte[] bytes = new byte[16];
            guidRandom.NextBytes(bytes);
            return new Guid(bytes).ToString();
        }
        
        public Card(bool isReal)
        {
            this.isReal = isReal;
            Actions = new List<AbstractAction>();
            Condition = null;
            Modifier = null;
            CardName = null;
            Rarity = Rarity.Common;
            UniqueId = "";
            cardRandom = RunInfo.NewRandom(UniqueId.GetHashCode());
        }

        public Card(Card card)
        {
            Actions = new List<AbstractAction>(card.Actions);
            CardName = card.CardName;
            Rarity = card.Rarity;
            UniqueId = GenerateDeterministicId();
            isReal = card.isReal;
            Condition = card.Condition;
            Modifier = card.Modifier;
            cardRandom = RunInfo.NewRandom(UniqueId.GetHashCode());
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
            UniqueId = GenerateDeterministicId();
            isReal = true;
            Condition = null;
            Modifier = null;
            // Condition = (AbstractCardCondition) Activator.CreateInstance(CardConditionsData.GetRandomCondition().ConditionType);
            // Modifier = (AbstractCardModifier) Activator.CreateInstance(CardModifiersData.GetRandomModifier(rarity).ModifierType);
            cardRandom = RunInfo.NewRandom(UniqueId.GetHashCode());
        }
    }
}

