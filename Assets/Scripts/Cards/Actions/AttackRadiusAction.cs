using System.Collections.Generic;
using Cards.CardEvents;
using Entities;
using Grid;
using StateManager;
using UnityEngine;

namespace Cards.Actions
{
    public class AttackRadiusAction : AbstractAction
    {
        public override string Icon
        {
            get
            {
                return "DamageAll";
            }
        }

        public int Radius;
        public int Amount;

        public AttackRadiusAction(int baseCost, string color, AbstractEntity entity, int radius, int amount)
            : base(baseCost, color, entity)
        {
            Radius = radius;
            Amount = amount;
        }

        public override List<AbstractCardEvent> Activate(CardMonobehaviour cardMono)
        {
            List<AbstractCardEvent> cardEvents = new List<AbstractCardEvent>();

            if (!GameStateManager.Instance.IsCurrent<PlayingState>())
                return cardEvents;

            Dictionary<Vector2Int, int> distanceMap =
                HexGridManager.Instance.CalculateDistanceMap(entity.positionRowCol, new List<Vector2Int>());

            PlayingState playing = GameStateManager.Instance.GetCurrent<PlayingState>();
            foreach (AbstractEntity ent in playing.GetEntities())
            {
                if (!playing.IsPlayerAttackTarget(ent))
                    continue;

                if (!distanceMap.TryGetValue(ent.positionRowCol, out int distance))
                    continue;

                if (distance >= 0 && distance <= Radius)
                    cardEvents.Add(new AttackCardEvent(ent.positionRowCol, Amount, manual: false));
            }

            return cardEvents;
        }

        public override List<AbstractCardEvent> Preview(CardMonobehaviour cardMono)
        {
            List<AbstractCardEvent> cardEvents = new List<AbstractCardEvent>();

            if (!GameStateManager.Instance.IsCurrent<PlayingState>())
                return cardEvents;

            Dictionary<Vector2Int, int> distanceMap =
                HexGridManager.Instance.CalculateDistanceMap(entity.positionRowCol, new List<Vector2Int>());

            foreach (KeyValuePair<Vector2Int, int> entry in distanceMap)
            {
                if (entry.Value >= 0 && entry.Value <= Radius)
                    cardEvents.Add(new AttackCardEvent(entry.Key, Amount, manual: false));
            }

            return cardEvents;
        }

        public override void HoverOn()
        {
        }

        public override void HoverOff()
        {
        }

        public override List<RectTransform> UpdateGraphic(GameObject diagram, GameObject tilePrefab, GameObject arrowPrefab)
        {
            GameObject basic = GameObject.Instantiate(tilePrefab, diagram.transform);
            return new List<RectTransform> { basic.GetComponent<RectTransform>() };
        }

        public override string GetText()
        {
            return "Attack enemies within " + Radius + " <arrow> for " + Amount + " <attack>";
        }

        public override string GetText(CardActionPreview preview)
        {
            int finalAmount = preview.GetFirstFinalValue(CardPreviewKeys.Damage, Amount);
            return "Attack enemies within " + Radius + " <arrow> for " + preview.FormatValue("<attack>", Amount, finalAmount);
        }

        public override string ToString()
        {
            return "Attack enemies within " + Radius + " for " + Amount;
        }
    }
}
