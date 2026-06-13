using System.Collections.Generic;
using Cards.CardEvents;
using StateManager;

namespace Types.CardRestrictions
{
    public static class CardPlayRestrictionSystem
    {
        public static bool CanPlay(global::CardMonobehaviour cardMono, out string blockedReason)
        {
            blockedReason = "";

            if (cardMono == null ||
                GameStateManager.Instance == null ||
                !GameStateManager.Instance.IsCurrent<PlayingState>())
            {
                return true;
            }

            PlayingState playingState = GameStateManager.Instance.GetCurrent<PlayingState>();
            if (playingState == null)
                return true;

            List<ICardPlayRestriction> restrictions = playingState.GetActiveCardPlayRestrictions();
            if (restrictions.Count == 0)
                return true;

            List<AbstractCardEvent> previewEvents = cardMono.BuildRestrictionPreviewEvents();

            foreach (ICardPlayRestriction restriction in restrictions)
            {
                if (restriction == null)
                    continue;

                if (restriction.Blocks(cardMono.Card, previewEvents, playingState))
                {
                    blockedReason = restriction.Reason;
                    return false;
                }
            }

            return true;
        }
    }
}
