using System.Collections.Generic;
using Cards.CardEvents;
using Entities;
using Grid;
using Types.Tiles;
using UnityEngine;
using UnityEngine.UI;

namespace Cards.Actions
{
    public class MoveRandomAction : AbstractAction
    {
        private const int RandomMoveDistance = 1;

        public override string Icon
        {
            get { return "footsteps"; }
        }

        public override string ToSimpleText()
        {
            return "<sprite name=footsteps>";
        }

        public MoveRandomAction(int baseCost, string color, AbstractEntity entity) : base(baseCost, color, entity)
        {
        }

        public override List<AbstractCardEvent> Activate(CardMonobehaviour cardMono)
        {
            return Activate(cardMono, previewMode: false);
        }

        public override List<AbstractCardEvent> Activate(CardMonobehaviour cardMono, bool previewMode)
        {
            return new List<AbstractCardEvent>
            {
                new RandomMoveCardEvent(RandomMoveDistance, GetStableActionRandom(cardMono, previewMode, "randommove"))
            };
        }

        public override string GetText()
        {
            return "Move in a random direction";
        }

        public override string GetText(CardActionPreview preview)
        {
            return GetText();
        }

        public override List<RectTransform> UpdateGraphic(GameObject diagram, GameObject tilePrefab, GameObject arrowPrefab)
        {
            List<RectTransform> elements = new List<RectTransform>();

            foreach (string direction in HexGridManager.HexDirections)
            {
                GameObject possibleTile = GameObject.Instantiate(tilePrefab, diagram.transform);
                Vector2Int tilePosition = HexGridManager.MoveHex(Vector2Int.zero, direction, RandomMoveDistance);
                Vector2 tileWorldPosition = HexGridManager.GetHexCenter(tilePosition.x, tilePosition.y) * 46.2222f;

                possibleTile.GetComponent<RectTransform>().localPosition = tileWorldPosition;
                Image tileImage = possibleTile.GetComponent<Image>();
                tileImage.color = TileData.tiles["basic"].color;
                tileImage.color = new Color(tileImage.color.r, tileImage.color.g, tileImage.color.b, 0.45f);

                elements.Add(possibleTile.GetComponent<RectTransform>());
            }

            return elements;
        }

        public override string ToString()
        {
            return "Move random direction 1";
        }
    }
}
