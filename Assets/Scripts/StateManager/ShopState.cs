using System.Collections.Generic;
using System.Linq;
using Cards;
using Cards.CardList;
using TMPro;
using Types.Actions;
using UnityEngine;
using Util;
using Random = System.Random;

namespace StateManager
{
    public class ShopState : GameState
    {
        private readonly Dictionary<CardRarity, double> rarityWeights = new()
        {
            { CardRarity.Common,    50 },
            { CardRarity.Uncommon,  25 },
            { CardRarity.Rare,      15 },
            { CardRarity.Epic,       7 },
            { CardRarity.Legendary,  2.5 },
            { CardRarity.Mythic,     0.5 }
        };

        private readonly Dictionary<CardRarity, int[]> costRanges = new()
        {
            { CardRarity.Common,     new[] {1, 4} },
            { CardRarity.Uncommon,   new[] {3, 6} },
            { CardRarity.Rare,       new[] {6, 10} },
            { CardRarity.Epic,       new[] {10, 15} },
            { CardRarity.Legendary,  new[] {12, 16} },
            { CardRarity.Mythic,     new[] {15, 20} }
        };

        public List<CardMonobehaviour> cardOptions;
        public List<TextMeshProUGUI> cardCostTexts;

        private List<int> _cardCostValues = new();
        private List<bool> _cardPurchased = new();
        private List<Card> _cardData = new();

        public GameObject window;
        public List<Card> CardOptions = new();
        private Random _random;

        public override void Enter()
        {
            _random = new Random();
            window.GetComponent<LerpPosition>().targetLocation = new Vector2(0, 0);

            CardOptions = CardData.AllIds
                .Select(id => CardData.Get(id).LocalCard)
                .ToList();

            PickCards();
        }
        public override void Exit()
        {
            window.GetComponent<LerpPosition>().targetLocation = new Vector2(0, 730);
            for (int i = 0; i < cardOptions.Count; i++)
            {
                if (_cardPurchased[i])
                {
                    cardOptions[i].GetComponent<LerpPosition>().targetLocation =
                        cardOptions[i].transform.localPosition - new Vector3(0, 450, 0);
                }
            }
        }

        public void Done()
        {
            GameStateManager.Instance.Change<PlayingState>();
        }

        private void PickCards()
        {
            _cardData.Clear();
            _cardCostValues.Clear();
            _cardPurchased = new List<bool> { false, false, false };

            for (int i = 0; i < 3; i++)
            {
                Card card = GetRandomItem();
                _cardData.Add(card);

                int cost = _random.Next(costRanges[card.Rarity][0], costRanges[card.Rarity][1]);
                _cardCostValues.Add(cost);

                cardOptions[i].SetCard(card);
                cardCostTexts[i].text = cost + "$";

                int index = i;
                
                // PURCHASE CALLBACK
                cardOptions[i].CardClickedCallback = () =>
                {
                    if (_cardPurchased[index]) return;

                    if (RunInfo.Instance.Money < _cardCostValues[index])
                        return;
                    
                    RunInfo.Instance.Money -= _cardCostValues[index];
                        
                    Deck.Instance.CreateCard(card);
                    cardOptions[index].GetComponent<LerpPosition>().speed = 10;
                    cardOptions[index].GetComponent<LerpPosition>().targetLocation =
                        cardOptions[index].transform.localPosition + new Vector3(0, 450, 0);

                    _cardPurchased[index] = true;
                };
            }
        }



        public Card GetRandomItem()
        {
            var eligibleWeights = rarityWeights
                .Where(kvp => CardOptions.Any(c => c.Rarity == kvp.Key))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            if (eligibleWeights.Count == 0)
                return new Card();

            double totalWeight = eligibleWeights.Values.Sum();
            double roll = _random.NextDouble() * totalWeight;

            CardRarity selectedRarity = CardRarity.Common;
            double cumulative = 0;

            foreach (var entry in eligibleWeights)
            {
                cumulative += entry.Value;
                if (roll <= cumulative)
                {
                    selectedRarity = entry.Key;
                    break;
                }
            }

            var itemsOfRarity = CardOptions
                .Where(c => c.Rarity == selectedRarity)
                .ToList();

            int index = _random.Next(itemsOfRarity.Count);
            return itemsOfRarity[index];
        }
    }
}
