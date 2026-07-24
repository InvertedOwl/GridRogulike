using Grid;
using UnityEngine;

namespace Util
{
    public static class HoverStateInvalidator
    {
        private const float DefaultSuppressionSeconds = 0.5f;
        private static float _suppressedUntilUnscaledTime;

        public static bool IsSuppressed => Time.unscaledTime < _suppressedUntilUnscaledTime;

        public static void ReleaseAll(float suppressionSeconds = DefaultSuppressionSeconds)
        {
            _suppressedUntilUnscaledTime = Mathf.Max(
                _suppressedUntilUnscaledTime,
                Time.unscaledTime + Mathf.Max(0f, suppressionSeconds));

            foreach (TileHover tileHover in Object.FindObjectsByType<TileHover>(
                         FindObjectsInactive.Include,
                         FindObjectsSortMode.None))
            {
                tileHover.ForceReleaseHover();
            }

            foreach (ButtonHoverScale buttonHover in Object.FindObjectsByType<ButtonHoverScale>(
                         FindObjectsInactive.Include,
                         FindObjectsSortMode.None))
            {
                buttonHover.ForceReleaseHover();
            }

            foreach (HexHoverForwarder hexHover in Object.FindObjectsByType<HexHoverForwarder>(
                         FindObjectsInactive.Include,
                         FindObjectsSortMode.None))
            {
                hexHover.ForcePointerExit();
            }
        }
    }
}
