using Grid;
using NUnit.Framework;
using UnityEngine;

namespace EditorTests
{
    public class HexGridManagerTests
    {
        [Test]
        public void TryGetStraightLine_AcceptsEveryHexDirection()
        {
            Vector2Int[] origins =
            {
                new Vector2Int(0, 0),
                new Vector2Int(0, 1)
            };

            foreach (Vector2Int origin in origins)
            {
                foreach (string expectedDirection in HexGridManager.HexDirections)
                {
                    for (int expectedDistance = 1; expectedDistance <= 3; expectedDistance++)
                    {
                        Vector2Int target = HexGridManager.MoveHex(
                            origin,
                            expectedDirection,
                            expectedDistance);

                        bool found = HexGridManager.TryGetStraightLine(
                            origin,
                            target,
                            3,
                            out string actualDirection,
                            out int actualDistance);

                        Assert.That(found, Is.True);
                        Assert.That(actualDirection, Is.EqualTo(expectedDirection));
                        Assert.That(actualDistance, Is.EqualTo(expectedDistance));
                    }
                }
            }
        }

        [Test]
        public void TryGetStraightLine_RejectsTurnWithinRange()
        {
            Vector2Int origin = Vector2Int.zero;
            Vector2Int target = HexGridManager.MoveHex(
                HexGridManager.MoveHex(origin, "ne", 1),
                "e",
                1);

            bool found = HexGridManager.TryGetStraightLine(
                origin,
                target,
                2,
                out _,
                out _);

            Assert.That(found, Is.False);
        }

        [Test]
        public void TryGetStraightLine_RejectsTargetBeyondRange()
        {
            Vector2Int origin = Vector2Int.zero;
            Vector2Int target = HexGridManager.MoveHex(origin, "e", 3);

            bool found = HexGridManager.TryGetStraightLine(
                origin,
                target,
                2,
                out _,
                out _);

            Assert.That(found, Is.False);
        }
    }
}
