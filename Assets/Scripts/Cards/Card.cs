using System;
using System.Collections.Generic;
using System.Linq;
using Cards.Actions;
using Cards.CardList;
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
        [SerializeField] public string DefinitionId;

        [SerializeField] public bool isReal;

        [SerializeField] public RandomState cardRandom;

        [JsonIgnore]
        public TargetDefinition TargetDefinition;

        [JsonIgnore]
        public int Cost =>
            (int)Mathf.Round(Actions.Sum(action => action.Cost) * 0.75f);

        public static RandomState guidRandom = RunInfo.NewRandom("guid");
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ResetStaticsOnLoad()
        {
            ResetStatics();
        }

        public static void ResetStatics()
        {
            guidRandom = RunInfo.NewRandom("guid");
        }
        
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
            DefinitionId = "";
            cardRandom = RunInfo.NewRandom(UniqueId);
            CardSet = CardSet.Base;
            TargetDefinition = Cards.CardList.TargetDefinition.None;
        }

        public Card(Card card)
        {
            if (!string.IsNullOrEmpty(card.DefinitionId) &&
                CardDefinitionRegistry.TryGetDefinition(card.DefinitionId, out CardDefinition definition))
            {
                Actions = definition.BuildActions();
                CardName = definition.DisplayName;
                Rarity = definition.Rarity;
                CardSet = definition.CardSet;
                DefinitionId = definition.Id;
                TargetDefinition = definition.TargetDefinition.Copy();
            }
            else
            {
                Actions = new List<AbstractAction>(card.Actions);
                CardName = card.CardName;
                Rarity = card.Rarity;
                CardSet = card.CardSet;
                DefinitionId = card.DefinitionId;
                TargetDefinition = card.TargetDefinition?.Copy() ?? Cards.CardList.TargetDefinition.None;
            }

            UniqueId = GenerateDeterministicId();
            isReal = card.isReal;
            cardRandom = RunInfo.NewRandom(UniqueId);
        }

        public Card(string cardName, List<AbstractAction> actions, Rarity rarity, CardSet cardSet)
            : this(cardName, cardName, actions, rarity, cardSet, Cards.CardList.TargetDefinition.None)
        {
        }

        public Card(
            string definitionId,
            string cardName,
            List<AbstractAction> actions,
            Rarity rarity,
            CardSet cardSet,
            TargetDefinition targetDefinition,
            bool isReal = true,
            string uniqueId = null)
        {
            Actions = actions;
            CardName = cardName;
            Rarity = rarity;
            UniqueId = string.IsNullOrEmpty(uniqueId) ? GenerateDeterministicId() : uniqueId;
            this.isReal = isReal;
            cardRandom = RunInfo.NewRandom(UniqueId);
            CardSet = cardSet;
            DefinitionId = definitionId;
            TargetDefinition = targetDefinition ?? Cards.CardList.TargetDefinition.None;
        }

        public override bool Equals(object obj)
        {
            if (obj is Card objCard)
            {
                return objCard.UniqueId == UniqueId;
            }
            else
            {
                return false;
            }
        }
    }
}

