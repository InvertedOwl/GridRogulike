using System;
using System.Collections.Generic;
using System.Linq;
using Cards.CardList;
using Entities;
using Serializer;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Util;

namespace StateManager
{
    public class EncounterResultState : GameState
    {
        public GameObject window;

        [SerializeField] private Vector3 visiblePosition = Vector3.zero;
        [SerializeField] private Vector3 hiddenPosition = new Vector3(0f, 730f, 0f);

        private EncounterResultStateSaveData _resultData;
        private bool _confirming;

        [SerializeField] private GameObject MoneyGainedPrefab;
        [SerializeField] private GameObject HealthGainedPrefab;
        [SerializeField] private GameObject MaxHealthGainedPrefab;
        [SerializeField] private GameObject CardPacksGainedPrefab;
        [SerializeField] private GameObject ListRewards;

        public IReadOnlyList<EncounterResult> Results =>
            _resultData != null ? _resultData.results : Array.Empty<EncounterResult>();
        public bool TimedOut => _resultData != null && _resultData.timedOut;
        public bool IsFinalEncounter => _resultData != null && _resultData.finalEncounter;

        public void SetResults(IEnumerable<EncounterResult> results, bool timedOut, bool finalEncounter)
        {
            var data = new EncounterResultStateSaveData
            {
                results = results?.Select(result => result?.Copy()).ToList(),
                timedOut = timedOut,
                finalEncounter = finalEncounter
            };
            if (!data.TryValidate(out string error))
                throw new ArgumentException(error, nameof(results));

            _resultData = data;
            _confirming = false;
        }

        public override void Enter()
        {
            if (SaveData is EncounterResultStateSaveData savedResults)
            {
                _resultData = savedResults;
                SaveData = null;
            }

            _confirming = false;
            PlayWindowInSound();
            MoveWindow(visiblePosition);
            PopulateUI();
        }

        private void PopulateUI()
        {
            Debug.Log(_resultData.results.Count + " Number of results");
            
            for (int i = 0; i < ListRewards.transform.childCount; i++)
            {
                Destroy(ListRewards.transform.GetChild(i).gameObject);
            }

            foreach (EncounterResult encounterResult in _resultData.results)
            {
                if (encounterResult.type == EncounterResultType.Money)
                {
                    GameObject moneyGained = Instantiate(MoneyGainedPrefab, ListRewards.transform);
                    moneyGained.GetComponentInChildren<TextMeshProUGUI>().text =
                        (Mathf.Sign(encounterResult.amount) == 1)?"+":"" + encounterResult.amount + " <color=\"yellow\">Money";
                }
                if (encounterResult.type == EncounterResultType.Health)
                {
                    GameObject moneyGained = Instantiate(HealthGainedPrefab, ListRewards.transform);
                    moneyGained.GetComponentInChildren<TextMeshProUGUI>().text =
                        (Mathf.Sign(encounterResult.amount) == 1)?"+":"" + encounterResult.amount + " <color=\"red\">Health";
                }
                if (encounterResult.type == EncounterResultType.MaxHealth)
                {
                    GameObject moneyGained = Instantiate(MaxHealthGainedPrefab, ListRewards.transform);
                    moneyGained.GetComponentInChildren<TextMeshProUGUI>().text =
                        (Mathf.Sign(encounterResult.amount) == 1)?"+":"" + encounterResult.amount + " <color=#aa0000>Max Health";
                }
                if (encounterResult.type == EncounterResultType.Card)
                {
                    GameObject moneyGained = Instantiate(CardPacksGainedPrefab, ListRewards.transform);
                    moneyGained.GetComponentInChildren<TextMeshProUGUI>().text =
                        (Mathf.Sign(encounterResult.amount) == 1)?"+":"" + encounterResult.amount + " <color=\"lightblue\">Card Pack" + ((encounterResult.amount != 1)? "s" : "");
                }
            }
        }

        public override void Exit()
        {
            PlayWindowOutSound();
            MoveWindow(hiddenPosition);
        }

        private void MoveWindow(Vector3 targetPosition)
        {
            if (window == null)
                return;

            if (window.TryGetComponent(out EasePosition easePosition))
            {
                easePosition.SendToLocation(targetPosition);
                return;
            }

            if (window.TryGetComponent(out LerpPosition lerpPosition))
            {
                lerpPosition.targetLocation = targetPosition;
                return;
            }

            window.transform.localPosition = targetPosition;
        }

        public EncounterResultStateSaveData CaptureResultSaveData() => _resultData;

        public void Okay()
        {
            if (!Manager.IsCurrent<EncounterResultState>() || _confirming || _resultData == null)
                return;

            if (!_resultData.TryValidate(out string error))
            {
                Debug.LogError(error);
                return;
            }

            _confirming = true;
            if (!_resultData.applied)
            {
                foreach (EncounterResult result in _resultData.results)
                    ApplyResult(result);

                _resultData.applied = true;
            }

            // Change checkpoints the updated resources and destination together. Repeated clicks
            // cannot reapply results, and reloading the pending checkpoint still requires Okay.
            if (Player.Instance.Health <= 0 || (TimedOut && IsFinalEncounter))
                Manager.Change<GameOverState>();
            else if (IsFinalEncounter)
                Manager.Change<GameFinishState>();
            else
                Manager.Change<ShopState>();
        }

        private static void ApplyResult(EncounterResult result)
        {
            switch (result.type)
            {
                case EncounterResultType.Money:
                    RunInfo.Instance.AddMoney((int)result.amount);
                    break;
                case EncounterResultType.Health:
                    Player.Instance.Health = Mathf.Max(0f, Player.Instance.Health + result.amount);
                    break;
                case EncounterResultType.MaxHealth:
                    // Healing is a separate Health entry so the UI can show both changes.
                    Player.Instance.initialHealth = Mathf.Max(1f, Player.Instance.initialHealth + result.amount);
                    Player.Instance.Health = Mathf.Min(Player.Instance.Health, Player.Instance.initialHealth);
                    break;
                case EncounterResultType.Card:
                    for (int i = 0; i < (int)result.amount; i++)
                        Deck.Instance.Cards.Add(CardDefinitionRegistry.CreateCard(result.cardDefinitionId));
                    // PlayingState rebuilds the combat deck from Cards on the next encounter.
                    break;
            }
        }
    }
}
