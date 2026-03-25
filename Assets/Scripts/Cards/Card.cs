using System;
using System.Collections.Generic;
using System.Linq;
using Cards.Actions;
using Newtonsoft.Json;
using UnityEngine;
using Types;

namespace Cards
{
    [System.Serializable]
    public struct Card
    {
        [SerializeField] public List<AbstractAction> Actions;
        [SerializeField] public string CardName;
        [SerializeField] public Rarity Rarity;
        [SerializeField] public string UniqueId;
        [SerializeField] public CardSet CardSet;

        [SerializeField] public bool isReal;

        [SerializeField] public RandomState cardRandom;

        [JsonIgnore]
        public int Cost =>
            (int)Mathf.Round(Actions.Sum(action => action.Cost) * 0.75f);

        public static RandomState guidRandom = RunInfo.NewRandom("guid".GetHashCode());
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
            CardName = null;
            Rarity = Rarity.Common;
            UniqueId = "";
            cardRandom = RunInfo.NewRandom(UniqueId.GetHashCode());
            CardSet = CardSet.Base;
        }

        public Card(Card card)
        {
            Actions = new List<AbstractAction>(card.Actions);
            CardName = card.CardName;
            Rarity = card.Rarity;
            UniqueId = GenerateDeterministicId();
            isReal = card.isReal;
            cardRandom = RunInfo.NewRandom(UniqueId.GetHashCode());
            CardSet = card.CardSet;
        }

        public Card(string cardName, List<AbstractAction> actions, Rarity rarity, CardSet cardSet)
        {
            Actions = actions;
            CardName = cardName;
            Rarity = rarity;
            UniqueId = GenerateDeterministicId();
            isReal = true;
            cardRandom = RunInfo.NewRandom(UniqueId.GetHashCode());
            CardSet = cardSet;
        }
    }
}

