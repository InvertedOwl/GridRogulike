using System.Collections.Generic;

namespace Serializer
{
    public class RunInfoSaveData
    {
        public int MaxEnergy;
        public int CurrentEnergy;
        public int Money;
        public int Difficulty;
        public int CurrentSteps;

        public string Seed;

        public Dictionary<int, RandomState> randoms;
    }
}