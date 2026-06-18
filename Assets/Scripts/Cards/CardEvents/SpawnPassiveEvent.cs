using System.Collections.Generic;
using Entities;
using Passives;
using StateManager;
using Types.Passives;

namespace Cards.CardEvents
{
    public class SpawnPassiveEvent : AbstractCardEvent
    {
        private string _passive;

        public SpawnPassiveEvent(string passive)
        {
            this._passive = passive;
        }

        public override Dictionary<string, PreviewValue> GetPreviewValues()
        {
            return new Dictionary<string, PreviewValue>
            {
                [CardPreviewKeys.PassiveName] = PreviewValue.Text(_passive)
            };
        }

        public override void Activate(AbstractEntity entity)
        {
            if (GameStateManager.Instance.GetCurrent<PlayingState>() is { } playingState)
            {
                playingState.AddEnvironmentPassive(PassiveData.GetPassiveEntry(_passive));
            }
        }
    }
}
