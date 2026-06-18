using System;

namespace Passives
{
    [Serializable]
    public class PassiveEntitySpawn
    {
        public string EntityName;
        public int Count;

        public PassiveEntitySpawn(string entityName, int count = 1)
        {
            EntityName = entityName;
            Count = count;
        }
    }
}
