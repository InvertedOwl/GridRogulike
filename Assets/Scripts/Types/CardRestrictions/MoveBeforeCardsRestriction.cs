using System.Collections.Generic;
using System.Linq;
using Cards;
using Cards.CardEvents;
using StateManager;

namespace Types.CardRestrictions
{
    public class MoveBeforeCardsRestriction : ICardPlayRestriction
    {
        private readonly int _moveCountWhenCreated;
        private readonly bool _attackCardsOnly;

        public MoveBeforeCardsRestriction(int moveCountWhenCreated, bool attackCardsOnly, string reason)
        {
            _moveCountWhenCreated = moveCountWhenCreated;
            _attackCardsOnly = attackCardsOnly;
            Reason = reason;
        }

        public string Reason { get; }

        public bool Blocks(Card card, List<AbstractCardEvent> previewEvents, PlayingState playingState)
        {
            if (playingState == null || playingState.PlayerMovesThisTurn > _moveCountWhenCreated)
                return false;

            if (!_attackCardsOnly)
                return true;

            return previewEvents.Any(cardEvent => cardEvent is AttackCardEvent);
        }
    }
}
