using UnityEngine;

namespace Types.Statuses
{
    public class DizzyStatus : MissChanceStatus
    {
        public DizzyStatus(int amount, RandomState random = null)
            : base(amount, random, "dizzy-status")
        {
        }

        protected override int MissChancePercent => Mathf.Clamp(Amount, 0, 100);
    }
}
