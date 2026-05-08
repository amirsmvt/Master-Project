using System.Collections.Generic;
using NeuroQuest.Core;
using UnityEngine;

namespace NeuroQuest.MiniGames.Common
{
    [CreateAssetMenu(menuName = "NeuroQuest/MiniGame Definition")]
    public class MiniGameDefinition : ScriptableObject
    {
        [Header("Mini Game")]
        [SerializeField] private MiniGameConfig miniGameConfig;

        [Header("Assessment Domains")]
        [SerializeField] private List<AssessmentDomain> supportedDomains = new();

        [Header("Levels")]
        [SerializeField] private List<DifficultyConfig> difficulties = new();

        [Header("Description")]
        [TextArea]
        [SerializeField] private string description;

        public MiniGameConfig MiniGameConfig => miniGameConfig;
        public IReadOnlyList<AssessmentDomain> SupportedDomains => supportedDomains;
        public IReadOnlyList<DifficultyConfig> Difficulties => difficulties;
        public string Description => description;

        public string MiniGameId
        {
            get
            {
                return miniGameConfig != null ? miniGameConfig.MiniGameId : string.Empty;
            }
        }

        public bool SupportsProfile(AssessmentProfile profile)
        {
            if (profile == null)
            {
                return true;
            }

            return profile.SupportsAnyDomain(supportedDomains);
        }

        public DifficultyConfig GetDifficultyByLevel(int levelNumber)
        {
            DifficultyConfig difficulty = difficulties.Find(item => item.LevelNumber == levelNumber);

            if (difficulty == null)
            {
                Debug.LogError(
                    $"MiniGameDefinition: Difficulty level {levelNumber} not found for mini game: {MiniGameId}"
                );
            }

            return difficulty;
        }

        public bool HasLevel(int levelNumber)
        {
            return difficulties.Exists(item => item.LevelNumber == levelNumber);
        }
    }
}