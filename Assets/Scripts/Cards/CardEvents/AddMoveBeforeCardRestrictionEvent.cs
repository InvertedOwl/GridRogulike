using System.Collections.Generic;
using Entities;
using StateManager;
using Types.CardRestrictions;

namespace Cards.CardEvents
{
    public class AddMoveBeforeCardRestrictionEvent : AbstractCardEvent
    {
        private readonly bool _attackCardsOnly;
        private readonly string _reason;

        public AddMoveBeforeCardRestrictionEvent(
            bool attackCardsOnly = false,
            string reason = "Must move before playing cards.")
        {
            _attackCardsOnly = attackCardsOnly;
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

            playingState.AddTemporaryCardPlayRestriction(
                new MoveBeforeCardsRestriction(
                    playingState.PlayerMovesThisTurn,
                    _attackCardsOnly,
                    _reason));
        }
    }
}
