namespace Util
{
    using UnityEngine;

    public static class HexColorUtility
    {
        public static Color HexToColor(string hex)
        {
            if (string.IsNullOrWhiteSpace(hex))
            {
                Debug.LogWarning("Hex color string is null or empty.");
                return Color.white;
            }

            if (!hex.StartsWith("#"))
            {
                hex = "#" + hex;
            }

            if (ColorUtility.TryParseHtmlString(hex, out Color color))
            {
                return color;
            }

            Debug.LogWarning($"Invalid hex color: {hex}");
            return Color.white;
        }
    }
}