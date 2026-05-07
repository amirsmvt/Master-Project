using System.Collections.Generic;
using NeuroQuest.MiniGames.Common;
using UnityEngine;

namespace NeuroQuest.Core
{
    [CreateAssetMenu(menuName = "NeuroQuest/Database")]
    public class NeuroQuestDatabase : ScriptableObject
    {
        [Header("Mini Games")]
        [SerializeField] private List<MiniGameConfig> miniGameConfigs = new();

        [Header("Difficulties")]
        [SerializeField] private List<DifficultyConfig> difficultyConfigs = new();

        public MiniGameConfig GetMiniGameById(string miniGameId)
        {
            if (string.IsNullOrWhiteSpace(miniGameId))
            {
                Debug.LogError("NeuroQuestDatabase: MiniGameId is null or empty.");
                return null;
            }

            MiniGameConfig config = miniGameConfigs.Find(item => item.MiniGameId == miniGameId);

            if (config == null)
            {
                Debug.LogError($"NeuroQuestDatabase: MiniGameConfig not found for id: {miniGameId}");
            }

            return config;
        }

        public DifficultyConfig GetDifficultyById(string difficultyId)
        {
            if (string.IsNullOrWhiteSpace(difficultyId))
            {
                Debug.LogError("NeuroQuestDatabase: DifficultyId is null or empty.");
                return null;
            }

            DifficultyConfig config = difficultyConfigs.Find(item => item.DifficultyId == difficultyId);

            if (config == null)
            {
                Debug.LogError($"NeuroQuestDatabase: DifficultyConfig not found for id: {difficultyId}");
            }

            return config;
        }

        public bool HasMiniGame(string miniGameId)
        {
            return miniGameConfigs.Exists(item => item.MiniGameId == miniGameId);
        }

        public bool HasDifficulty(string difficultyId)
        {
            return difficultyConfigs.Exists(item => item.DifficultyId == difficultyId);
        }
    }
}