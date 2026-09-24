using System;
using System.Collections.Generic;
using StateManager;

namespace Serializer
{
    [Serializable]
    public class EncounterResultStateSaveData
    {
        public List<EncounterResult> results = new();
        public bool timedOut;
        public bool finalEncounter;
        public bool applied;

        public bool TryValidate(out string error)
        {
            error = null;
            if (results == null)
            {
                error = "Encounter result save is missing its result list.";
                return false;
            }

            foreach (EncounterResult result in results)
            {
                if (result == null)
                {
                    error = "Encounter result save contains a null result.";
                    return false;
                }

                if (!result.TryValidate(out error))
                    return false;
            }

            return true;
        }
    }
}
