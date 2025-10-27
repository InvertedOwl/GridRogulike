using System;
using System.Collections.Generic;
using System.Linq;
using Cards;
using Cards.CardList;
using TMPro;
using UnityEngine;
using Util;
using Types;
using Random = System.Random;

namespace StateManager
{
    public class ShopState : GameState
    {
        private readonly Dictionary<Rarity, double> rarityWeights = new()
        {
            { Rarity.Common,    50 },
            { Rarity.Uncommon,  25 },
            { Rarity.Rare,      15 },
            { Rarity.Epic,       7 },
            { Rarity.Legendary,  2.5 },
            { Rarity.Mythic,     0.5 }
        };

        private readonly Dictionary<Rarity, int[]> costRanges = new()
        {
            { Rarity.Common,     new[] {1, 4} },
            { Rarity.Uncommon,   new[] {3, 6} },
            { Rarity.Rare,       new[] {6, 10} },
            { Rarity.Epic,       new[] {10, 15} },
            { Rarity.Legendary,  new[] {12, 16} },
            { Rarity.Mythic,     new[] {15, 20} }
        };

        public List<CardMonobehaviour> cardOptions;
        public List<TextMeshProUGUI> cardCostTexts;

        private List<int> _cardCostValues = new();
        private List<bool> _cardPurchased = new();
        private List<Card> _cardData = new();

        public GameObject window;
        public List<Card> CardOptions = new();
        public TextMeshProUGUI buyEnergyText;

        public GameObject CardCombine;
        private bool isCardCombine;

        private Random _shopRandom;

        public void Awake()
        {
            _shopRandom = RunInfo.NewRandom("shop".GetHashCode());
        }

        public override void Enter()
        {
            window.GetComponent<EasePosition>().SendToLocation(new Vector2(0, 0));

            CardOptions = CardData.AllIds
                .Select(id => CardData.Get(id).LocalCard)
                .ToList();
            
            buyEnergyText.text = EnergyCost(RunInfo.Instance.MaxEnergy) + "$";

            PickCards();
        }
        public override void Exit()
        {
            window.GetComponent<EasePosition>().SendToLocation(new Vector2(0, 730));
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
            Debug.Log("Done with shop");
            GameStateManager.Instance.Change<MapState>();
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
                card.RandomizeModifiers();

                int cost = _shopRandom.Next(costRanges[card.Rarity][0], costRanges[card.Rarity][1]);
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

        public void SetShopState()
        {
            GameStateManager.Instance.Change<ShopState>();
        }

        public void BuyHealFull()
        {
            
        }

        public void BuyCombineCards()
        {
            if (RunInfo.Instance.Money < RunInfo.Instance.combineCost)
                return;
            RunInfo.Instance.Money -= RunInfo.Instance.combineCost;
            
            CardCombine.GetComponent<EasePosition>().targetLocation = new Vector3(0, 0);
            isCardCombine = true;
        }

        public void CancelCombineCards()
        {
            RunInfo.Instance.Money += RunInfo.Instance.combineCost;
            CardCombine.GetComponent<EasePosition>().SendToLocation(new Vector3(0, 750), () =>
            {
                CardCombine.GetComponent<CardCombine>().CancelCombine();
            });
            
            isCardCombine = false;
        }

        public void ConfirmCombineCards()
        {
            CardCombine.GetComponent<EasePosition>().SendToLocation(new Vector3(0, 750), () =>
            {
                CardCombine.GetComponent<CardCombine>().ConfirmCombine();
            });
            isCardCombine = false;
        }
        

        public void BuyEnergy()
        {
            Debug.Log("Buying energy");
            if (RunInfo.Instance.Money < EnergyCost(RunInfo.Instance.MaxEnergy))
                return;
            
            RunInfo.Instance.Money -= EnergyCost(RunInfo.Instance.MaxEnergy);
            RunInfo.Instance.MaxEnergy += 1;
            RunInfo.Instance.CurrentEnergy = RunInfo.Instance.MaxEnergy;
            buyEnergyText.text = EnergyCost(RunInfo.Instance.MaxEnergy) + "$";
        }


        public int EnergyCost(int currentEnergy)
        {
            return Mathf.RoundToInt(4 * Mathf.Pow(currentEnergy, 1.0f/3));
        }


        public Card GetRandomItem()
        {
            var eligibleWeights = rarityWeights
                .Where(kvp => CardOptions.Any(c => c.Rarity == kvp.Key))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            if (eligibleWeights.Count == 0)
                return new Card();

            double totalWeight = eligibleWeights.Values.Sum();
            double roll = _shopRandom.NextDouble() * totalWeight;

            Rarity selectedRarity = Rarity.Common;
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

            int index = _shopRandom.Next(itemsOfRarity.Count);
            return itemsOfRarity[index];
        }
    }
}
