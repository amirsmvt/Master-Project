using System;
using System.Collections.Generic;

namespace NeuroQuest.MiniGames.Common
{
    [Serializable]
    public class MiniGameResult
    {
        public string MiniGameId { get; private set; }
        public bool IsCompleted { get; private set; }
        public int Score { get; private set; }

        private readonly Dictionary<string, string> extraData = new();

        public IReadOnlyDictionary<string, string> ExtraData => extraData;

        public MiniGameResult(string miniGameId, bool isCompleted, int score)
        {
            MiniGameId = miniGameId;
            IsCompleted = isCompleted;
            Score = score;
        }

        public void AddExtraData(string key, object value)
        {
            if (string.IsNullOrWhiteSpace(key))
                return;

            extraData[key] = value != null ? value.ToString() : string.Empty;
        }
    }
}