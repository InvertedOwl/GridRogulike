using Entities;

namespace Passives
{
    public readonly struct PassiveContext
    {
        public readonly string PassiveName;
        public readonly AbstractEntity Entity;
        public readonly bool PreviewMode;

        public PassiveContext(string passiveName, AbstractEntity entity, bool previewMode)
        {
            PassiveName = passiveName;
            Entity = entity;
            PreviewMode = previewMode;
        }

        public RandomState GetRandom(string stream = "default")
        {
            string safePassiveName = string.IsNullOrEmpty(PassiveName) ? "none" : PassiveName;
            string safeStream = string.IsNullOrEmpty(stream) ? "default" : stream;
            RandomState random = RunInfo.NewRandom($"passive:{safePassiveName}:{safeStream}");

            return PreviewMode ? random.Clone() : random;
        }
    }
}
