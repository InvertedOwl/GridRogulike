namespace Types.Statuses
{
    public class BlindStatus : MissChanceStatus
    {
        public BlindStatus(int amount, RandomState random = null)
            : base(amount, random, "blind-status")
        {
        }

        protected override int MissChancePercent => 30;
    }
}
