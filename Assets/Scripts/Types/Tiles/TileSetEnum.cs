using System.Collections.Generic;
using UnityEngine;

namespace Types.Tiles
{
    public enum TileSetEnum
    {
        Blue,
        Red,
        Green,
        Purple,
        Unchanged,
        System
    }

    public static class TileSetColors
    {
        public static readonly Dictionary<TileSetEnum, Color> Colors = new Dictionary<TileSetEnum, Color>
        {
            [TileSetEnum.Blue] = new Color32(52, 152, 219, 255),
            [TileSetEnum.Red] = new Color32(231, 76, 60, 255),
            [TileSetEnum.Green] = new Color32(39, 174, 96, 255),
            [TileSetEnum.Purple] = new Color32(142, 68, 173, 255),
            [TileSetEnum.Unchanged] = new Color32(55, 81, 102, 255),
            [TileSetEnum.System] = new Color32(173, 173, 173, 255)
        };

        public static Color ToColor(this TileSetEnum tileSet)
        {
            return Colors.TryGetValue(tileSet, out Color color) ? color : Color.white;
        }
    }
}
