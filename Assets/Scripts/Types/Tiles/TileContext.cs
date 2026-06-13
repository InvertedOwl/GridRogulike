using Entities;
using UnityEngine;

namespace Types.Tiles
{
    public readonly struct TileContext
    {
        public readonly Vector2Int Position;
        public readonly string TileId;
        public readonly AbstractEntity Entity;
        public readonly bool PreviewMode;

        public TileContext(Vector2Int position, string tileId, AbstractEntity entity, bool previewMode)
        {
            Position = position;
            TileId = tileId;
            Entity = entity;
            PreviewMode = previewMode;
        }

        public RandomState GetRandom(string stream = "default")
        {
            string safeTileId = string.IsNullOrEmpty(TileId) ? "none" : TileId;
            string safeStream = string.IsNullOrEmpty(stream) ? "default" : stream;
            RandomState random = RunInfo.NewRandom(
                $"tile:{safeTileId}:{Position.x},{Position.y}:{safeStream}");

            return PreviewMode ? random.Clone() : random;
        }
    }
}
