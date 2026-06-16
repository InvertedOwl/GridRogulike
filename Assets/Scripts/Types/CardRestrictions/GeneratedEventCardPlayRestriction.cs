using System.Collections.Generic;
using System.Linq;
using Cards;
using Cards.CardEvents;
using StateManager;

namespace Types.CardRestrictions
{
    public enum RestrictedCardEventKind
    {
        Attack,
        Shield,
        Movement
    }

    public class GeneratedEventCardPlayRestriction : ICardPlayRestriction
    {
        private readonly RestrictedCardEventKind _eventKind;

        public GeneratedEventCardPlayRestriction(RestrictedCardEventKind eventKind, string reason)
        {
            _eventKind = eventKind;
            Reason = reason;
        }

        public string Reason { get; }

        public bool Blocks(Card card, List<AbstractCardEvent> previewEvents, PlayingState playingState)
        {
            return previewEvents != null && previewEvents.Any(BlocksEvent);
        }

        private bool BlocksEvent(AbstractCardEvent cardEvent)
        {
            return _eventKind switch
            {
                RestrictedCardEventKind.Attack => cardEvent is AttackCardEvent,
                RestrictedCardEventKind.Shield => cardEvent is ShieldCardEvent shieldCardEvent &&
                                                  shieldCardEvent.amount > 0,
                RestrictedCardEventKind.Movement => cardEvent is GainStepsCardEvent or
                                                    MoveCardEvent or
                                                    RandomMoveCardEvent or
                                                    TeleportPlayerToStartingTileCardEvent,
                _ => false
            };
        }
    }
}
