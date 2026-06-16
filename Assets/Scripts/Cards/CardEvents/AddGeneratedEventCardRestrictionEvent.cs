using System.Collections.Generic;
using Entities;
using StateManager;
using Types.CardRestrictions;

namespace Cards.CardEvents
{
    public enum CardPlayRestrictionDuration
    {
        Turn,
        Combat
    }

    public class AddGeneratedEventCardRestrictionEvent : AbstractCardEvent
    {
        private readonly RestrictedCardEventKind _eventKind;
        private readonly CardPlayRestrictionDuration _duration;
        private readonly string _reason;

        public AddGeneratedEventCardRestrictionEvent(
            RestrictedCardEventKind eventKind,
            CardPlayRestrictionDuration duration,
            string reason)
        {
            _eventKind = eventKind;
            _duration = duration;
            _reason = reason;
        }

        public override Dictionary<string, PreviewValue> GetPreviewValues()
        {
            return new Dictionary<string, PreviewValue>();
        }

        public override void Activate(AbstractEntity entity)
        {
            if (entity == null || entity.entityType != EntityType.Player)
                return;

            if (GameStateManager.Instance.GetCurrent<PlayingState>() is not { } playingState)
                return;

            GeneratedEventCardPlayRestriction restriction =
                new GeneratedEventCardPlayRestriction(_eventKind, _reason);

            if (_duration == CardPlayRestrictionDuration.Combat)
                playingState.AddCombatCardPlayRestriction(restriction);
            else
                playingState.AddTemporaryCardPlayRestriction(restriction);
        }
    }
}
