using System;
using System.Collections.Generic;

namespace NeuroQuest.Data
{
    [Serializable]
    public class GameEventData
    {
        public string eventType;
        public string miniGameId;
        public string difficultyId;
        public float timeFromSessionStart;

        public List<DataField> fields = new();
    }
}