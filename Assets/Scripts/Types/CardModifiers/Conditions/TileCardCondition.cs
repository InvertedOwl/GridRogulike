using System.Linq;
using Cards;
using Entities;
using Grid;
using StateManager;
using Types.Tiles;

namespace Types.CardModifiers.Conditions
{
    public class TileCardCondition: AbstractCardCondition
    {

        private string _tile;
        public TileCardCondition()
        {
            _tile = TileData.tiles.Keys.ElementAt(cardConditionRandom.Next(0, TileData.tiles.Count));
            this.ConditionText = "On " + TileData.tiles[_tile].name + " Tile: ";
        }
        
        public override bool Condition(Card card)
        {
            Player player = GameStateManager.Instance.GetCurrent<PlayingState>().player;
            HexGridManager hexGridManager = HexGridManager.Instance;
            return hexGridManager.HexType(player.positionRowCol) == _tile;
        }
    }
}