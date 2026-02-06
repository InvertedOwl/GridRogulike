using System.Collections.Generic;
using Entities;
using Grid;
using StateManager;
using Cards.CardEvents;
using Passives;
using Types.Statuses;
using UnityEngine;
using UnityEngine.UI;

namespace Cards.Actions
{
    public class SpawnPassiveAction: AbstractAction
    {
        private PassiveEntry _passive;
        public SpawnPassiveAction(int baseCost, string color, AbstractEntity entity, PassiveEntry passive) : base(baseCost, color, entity)
        {
            this._passive = passive;
        }

        public override List<AbstractCardEvent> Activate(CardMonobehaviour cardMono)
        {
            return new List<AbstractCardEvent> { new SpawnPassiveEvent(_passive) };
        }

        
        public override void HoverOn() { }

        public override void HoverOff() { }

        public override List<RectTransform> UpdateGraphic(GameObject diagram, GameObject tilePrefab, GameObject arrowPrefab)
        {
            // GameObject basic = GameObject.Instantiate(tilePrefab, diagram.transform);
            // Vector2Int newPos =
            //     HexGridManager.MoveHex(new Vector2Int(0, 0), this.Direction, this.Distance);
            // Vector2 newPosWorld = HexGridManager.GetHexCenter(newPos.x, newPos.y) * 46.2222f;
            //     
            // basic.GetComponent<RectTransform>().localPosition = newPosWorld;
            // basic.GetComponent<Image>().color = new Color(212/255.0f, 81/255.0f, 81/255.0f);
            // basic.GetComponent<RectTransform>();
            return new List<RectTransform>();
        }

        public PassiveEntry GetPassive()
        {
            return _passive;
        }

        public override string GetText()
        {
            return "Spawn Passive";
        }

        public override string ToString()
        {
            return "Spawn Passive";
        }
    }
}