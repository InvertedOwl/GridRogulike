using Entities;
using Types.Statuses;

namespace Cards.CardEvents
{
    public static class StatusApplicationFx
    {
        private const string StunFxKey = "StunExplosion";
        private const string PoisonFxKey = "PoisonExplosion";

        public static void TryPlay(AbstractStatus status, AbstractEntity target)
        {
            if (status == null || target == null || status.Amount <= 0 || FXManager.Instance == null)
                return;

            string fxKey = StatusFxKey(status);
            if (string.IsNullOrEmpty(fxKey))
                return;

            FXManager.Instance.TryPlay(fxKey, target.transform.position);
        }

        private static string StatusFxKey(AbstractStatus status)
        {
            return status switch
            {
                DazedStatus => StunFxKey,
                PoisonStatus => PoisonFxKey,
                _ => null
            };
        }
    }
}
