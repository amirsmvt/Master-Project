using System;
using System.Collections.Generic;

namespace NeuroQuest.Data
{
    [Serializable]
    public class SessionData
    {
        public string sessionId;
        public string participantId;
        public string groupLabel;
        public string sessionStartTime;

        public List<GameEventData> events = new();
    }
}