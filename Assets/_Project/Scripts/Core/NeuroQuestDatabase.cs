using System.Collections.Generic;
using NeuroQuest.MiniGames.Common;
using NeuroQuest.Story;
using UnityEngine;

namespace NeuroQuest.Core
{
    [CreateAssetMenu(menuName = "NeuroQuest/Database")]
    public class NeuroQuestDatabase : ScriptableObject
    {
        [Header("Mini Game Definitions")]
        [SerializeField] private List<MiniGameDefinition> miniGameDefinitions = new();

        [Header("Story Scenarios")]
        [SerializeField] private List<StoryScenario> storyScenarios = new();

        public MiniGameDefinition GetMiniGameDefinitionById(
            string miniGameId,
            AssessmentProfile profile = null)
        {
            if (string.IsNullOrWhiteSpace(miniGameId))
            {
                Debug.LogError("NeuroQuestDatabase: MiniGameId is null or empty.");
                return null;
            }

            MiniGameDefinition definition = miniGameDefinitions.Find(item =>
                item != null &&
                item.MiniGameConfig != null &&
                item.MiniGameId == miniGameId
            );

            if (definition == null)
            {
                Debug.LogError($"NeuroQuestDatabase: MiniGameDefinition not found for id: {miniGameId}");
                return null;
            }

            if (!definition.SupportsProfile(profile))
            {
                string profileName = profile != null ? profile.DisplayName : "None";

                Debug.LogError(
                    $"NeuroQuestDatabase: MiniGame '{miniGameId}' is not enabled for profile: {profileName}"
                );

                return null;
            }

            return definition;
        }

        public MiniGameConfig GetMiniGameConfigById(
            string miniGameId,
            AssessmentProfile profile = null)
        {
            MiniGameDefinition definition = GetMiniGameDefinitionById(miniGameId, profile);
            return definition != null ? definition.MiniGameConfig : null;
        }

        public DifficultyConfig GetDifficultyByLevel(
            string miniGameId,
            int levelNumber,
            AssessmentProfile profile = null)
        {
            MiniGameDefinition definition = GetMiniGameDefinitionById(miniGameId, profile);

            if (definition == null)
            {
                return null;
            }

            return definition.GetDifficultyByLevel(levelNumber);
        }

        public List<MiniGameDefinition> GetEnabledMiniGameDefinitions(AssessmentProfile profile)
        {
            List<MiniGameDefinition> enabledDefinitions = new();

            foreach (MiniGameDefinition definition in miniGameDefinitions)
            {
                if (definition == null || definition.MiniGameConfig == null)
                {
                    continue;
                }

                if (definition.SupportsProfile(profile))
                {
                    enabledDefinitions.Add(definition);
                }
            }

            return enabledDefinitions;
        }

        public StoryScenario GetStoryScenarioById(string scenarioId)
        {
            if (string.IsNullOrWhiteSpace(scenarioId))
            {
                Debug.LogError("NeuroQuestDatabase: ScenarioId is null or empty.");
                return null;
            }

            StoryScenario scenario = storyScenarios.Find(item =>
                item != null &&
                item.ScenarioId == scenarioId
            );

            if (scenario == null)
            {
                Debug.LogError($"NeuroQuestDatabase: StoryScenario not found for id: {scenarioId}");
            }

            return scenario;
        }
    }
}