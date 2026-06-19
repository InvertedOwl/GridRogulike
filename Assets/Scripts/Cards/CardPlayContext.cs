using Cards.CardList;
using Entities;
using StateManager;

namespace Cards
{
    public class CardPlayContext
    {
        public CardMonobehaviour CardMono { get; }
        public Card Card { get; }
        public AbstractEntity SourceEntity { get; }
        public TargetSelection Targets { get; }
        public PlayingState PlayingState { get; }
        public bool PreviewMode { get; }
        public int ActionIndex { get; }

        public TargetDefinition TargetDefinition => Targets?.Definition ?? Card.TargetDefinition ?? Cards.CardList.TargetDefinition.None;

        public CardPlayContext(
            CardMonobehaviour cardMono,
            Card card,
            AbstractEntity sourceEntity,
            TargetSelection targets,
            PlayingState playingState,
            bool previewMode,
            int actionIndex = -1)
        {
            CardMono = cardMono;
            Card = card;
            SourceEntity = sourceEntity;
            Targets = targets ?? TargetSelection.Empty(card.TargetDefinition);
            PlayingState = playingState;
            PreviewMode = previewMode;
            ActionIndex = actionIndex;
        }

        public CardPlayContext WithActionIndex(int actionIndex)
        {
            return new CardPlayContext(
                CardMono,
                Card,
                SourceEntity,
                Targets,
                PlayingState,
                PreviewMode,
                actionIndex);
        }

        public RandomState GetStableActionRandom(string actionTypeName, string stream = "default")
        {
            string safeStream = string.IsNullOrEmpty(stream) ? "default" : stream;
            string safeActionType = string.IsNullOrEmpty(actionTypeName) ? "action" : actionTypeName;
            string key = $"card:{Card.UniqueId}:action:{ActionIndex}:{safeActionType}:{safeStream}";
            RandomState random = RunInfo.NewRandom(key);
            return PreviewMode ? random.Clone() : random;
        }
    }
}
