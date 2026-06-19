using System;

namespace Cards.CardList
{
    public enum TargetType
    {
        None,
        Self,
        AnyEnemy,
        EveryEnemy,
        AnyEntity,
        AnyTile,
        EmptyTile
    }

    [Serializable]
    public class TargetDefinition
    {
        public TargetType TargetType;
        public int? MaxRange;

        public TargetDefinition(TargetType targetType, int? maxRange = null)
        {
            TargetType = targetType;
            MaxRange = maxRange;
        }

        public static TargetDefinition None => new TargetDefinition(TargetType.None);

        public bool RequiresWorldTarget =>
            TargetType == TargetType.AnyEnemy ||
            TargetType == TargetType.EveryEnemy ||
            TargetType == TargetType.AnyEntity ||
            TargetType == TargetType.AnyTile ||
            TargetType == TargetType.EmptyTile;

        public bool CanPlayFromCardClick =>
            TargetType == TargetType.None ||
            TargetType == TargetType.Self ||
            TargetType == TargetType.EveryEnemy;

        public TargetDefinition Copy()
        {
            return new TargetDefinition(TargetType, MaxRange);
        }
    }
}
