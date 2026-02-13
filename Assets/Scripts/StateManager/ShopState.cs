using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cards;
using Cards.CardList;
using TMPro;
using UnityEngine;
using Util;
using Types;
using Types.ShopActions;
using UnityEngine.UI;
using Random = System.Random;

namespace StateManager
{
    public class ShopState : GameState
    {
        private readonly Dictionary<Rarity, double> rarityWeights = new()
        {
            { Rarity.Common,     60 },
            { Rarity.Uncommon,   30 },
            { Rarity.Rare,       6 },
            { Rarity.Epic,       3 },
            { Rarity.Legendary, .8 },
            { Rarity.Mythic,    .2 }
        };

        private readonly Dictionary<Rarity, int[]> costRanges = new()
        {
            { Rarity.Common,     new[] {1, 3} },
            { Rarity.Uncommon,   new[] {4, 6} },
            { Rarity.Rare,       new[] {7, 10} },
            { Rarity.Epic,       new[] {11, 15} },
            { Rarity.Legendary,  new[] {16, 18} },
            { Rarity.Mythic,     new[] {19, 23} }
        };

        public List<GameObject> shopActions;
        public List<CardMonobehaviour> cardOptions;
        public List<TextMeshProUGUI> cardCostTexts;

        private List<int> _cardCostValues = new();
        private List<bool> _cardPurchased = new();
        private List<Card> _cardData = new();

        public GameObject window;
        public List<Card> CardOptions = new();

        public GameObject CardCombine;
        private bool isCardCombine;

        private Random _shopRandom;
        public TextMeshProUGUI refreshCostText;
        private int _refreshCost = 5;
        public Button refreshButton;
        
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
            
            // buyEnergyText.text = EnergyCost(RunInfo.Instance.MaxEnergy) + "$";

            PickCards();
            PickActions();
            _refreshCost = 5;
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


        public void Refresh()
        {
            if (RunInfo.Instance.Money < _refreshCost)
            {
                // TODO: animation for POOR. 
                return;
            }
            RunInfo.Instance.Money -= _refreshCost;
            
            StartCoroutine(SwapCards());
            _refreshCost += 1;
            refreshCostText.text = "$" + _refreshCost;
            refreshCostText.GetComponent<RectTransform>().localPosition += new Vector3(0, 5, 0);
            refreshButton.interactable = false;
        }

        IEnumerator SwapCards()
        {

            for (int i = 0; i < CardOptions.Count; i++)
            {
                if (i >= cardOptions.Count)
                    continue;

                yield return new WaitForSeconds(0.1f);
                cardOptions[i].GetComponent<LerpPosition>().speed *= 1.2f;
                cardOptions[i].GetComponent<LerpPosition>().targetRotation = Quaternion.Euler(0, -180, 0);
                shopActions[i].transform.GetChild(0).GetComponent<LerpPosition>().targetRotation = Quaternion.Euler(-180, 0, 0);
            }
            
            yield return new WaitForSeconds(0.4f);
            PickCards();
            PickActions();
            
            for (int i = 0; i < CardOptions.Count; i++)
            {
                if (i >= cardOptions.Count)
                    continue;
                yield return new WaitForSeconds(0.1f);
                cardCostTexts[i].GetComponent<RectTransform>().localPosition += new Vector3(0, 5, 0);
                shopActions[i].transform.GetChild(1).GetComponent<RectTransform>().localPosition += new Vector3(0, 5, 0);
                cardOptions[i].GetComponent<LerpPosition>().targetRotation = Quaternion.Euler(0, 0, 0);
                shopActions[i].transform.GetChild(0).GetComponent<LerpPosition>().targetRotation = Quaternion.Euler(0, 0, 0);
            }
            yield return new WaitForSeconds(0.25f);
            for (int i = 0; i < CardOptions.Count; i++)
            {
                if (i >= cardOptions.Count)
                    continue;
                cardOptions[i].GetComponent<LerpPosition>().speed /= 1.2f;
            }
            
            refreshButton.interactable = true;
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

                int cost = _shopRandom.Next(costRanges[card.Rarity][0], costRanges[card.Rarity][1]);
                _cardCostValues.Add(cost);

                cardOptions[i].SetCard(card);
                cardCostTexts[i].text = "$" + cost;

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
        

        public void BuyCombineCards()
        {
            CardCombine.GetComponent<EasePosition>().targetLocation = new Vector3(0, 0);
            isCardCombine = true;
        }

        public void PickActions()
        {
            List<ShopActionEntry> shopActionsData = ShopActionData.GetThreeActions();

            for (int i = 0; i < shopActions.Count; i++)
            {
                shopActions[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "$" + shopActionsData[i].cost;
                shopActions[i].transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text =
                    shopActionsData[i].title;
                shopActions[i].transform.GetChild(0).GetChild(0).GetComponent<Button>().onClick.RemoveAllListeners();
                shopActions[i].transform.GetChild(0).GetChild(0).GetComponent<Button>().interactable = true;
                
                int j = i; 
                shopActions[i].transform.GetChild(0).GetChild(0).GetComponent<Button>().onClick.AddListener(() =>
                {
                    if (RunInfo.Instance.Money < shopActionsData[j].cost)
                        return;
                    
                    RunInfo.Instance.Money -= shopActionsData[j].cost;
                    
                    shopActionsData[j].callback.Invoke();
                    shopActions[j].transform.GetChild(0).GetChild(0).GetComponent<Button>().interactable = false;
                    shopActions[j].transform.GetChild(0).GetComponent<LerpPosition>().targetRotation = Quaternion.Euler(-180, 0, 0);
                });
            }
        }

        public void CancelCombineCards()
        {
            RunInfo.Instance.Money += RunInfo.Instance.combineCost;
            CardCombine.GetComponent<EasePosition>().SendToLocation(new Vector3(0, 750), () =>
            {
                if (RunInfo.Instance.Money < RunInfo.Instance.combineCost)
                    return;
                RunInfo.Instance.Money -= RunInfo.Instance.combineCost;
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
