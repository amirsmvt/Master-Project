using System;
using NeuroQuest.Data;
using NeuroQuest.MiniGames.Common;
using NeuroQuest.Services;
using NeuroQuest.Story;
using UnityEngine;

namespace NeuroQuest.Core
{
    public class GameManager : MonoBehaviour
    {
        [Header("Core References")]
        [SerializeField] private NeuroQuestDatabase database;
        [SerializeField] private AssessmentProfile activeProfile;
        [SerializeField] private MiniGameRunner miniGameRunner;
        [SerializeField] private DataLogger dataLogger;
        [SerializeField] private StoryRunner storyRunner;

        [Header("Story Settings")]
        [SerializeField] private StoryScenario startingScenario;

        private bool sessionStarted;

        private void Awake()
        {
            ServiceLocator.Register(this);
        }

        private void ResolveServices()
        {
            if (miniGameRunner == null)
            {
                miniGameRunner = ServiceLocator.Get<MiniGameRunner>();
            }

            if (dataLogger == null)
            {
                dataLogger = ServiceLocator.Get<DataLogger>();
            }

            if (storyRunner == null)
            {
                storyRunner = ServiceLocator.Get<StoryRunner>();
            }
        }

        public void StartGameSession(string participantId, string groupLabel)
        {
            ResolveServices();

            if (!ValidateCoreReferences())
            {
                return;
            }

            if (sessionStarted)
            {
                Debug.LogWarning("GameManager: Session already started.");
                return;
            }

            sessionStarted = true;

            dataLogger.SetParticipantInfo(participantId, groupLabel);

            dataLogger.LogSimpleEvent(
                "assessment_profile_selected",
                "",
                "",
                Field.Of("profileId", activeProfile.ProfileId),
                Field.Of("profileName", activeProfile.DisplayName)
            );

            Debug.Log("GameManager: Game session started.");
        }

        public void StartStory()
        {
            if (!sessionStarted)
            {
                Debug.LogError("GameManager: Cannot start story before session starts.");
                return;
            }

            if (startingScenario == null)
            {
                Debug.LogError("GameManager: StartingScenario is not assigned.");
                return;
            }

            StartStory(startingScenario);
        }

        public void StartStory(StoryScenario scenario)
        {
            ResolveServices();

            if (!sessionStarted)
            {
                Debug.LogError("GameManager: Cannot start story before session starts.");
                return;
            }

            if (storyRunner == null)
            {
                Debug.LogError("GameManager: StoryRunner is not assigned and not registered.");
                return;
            }

            storyRunner.StartStory(scenario);
        }

        public void RunMiniGame(
            MiniGameDefinition miniGameDefinition,
            int levelNumber,
            Action<MiniGameResult> onCompleted = null)
        {
            ResolveServices();

            if (!ValidateCoreReferences())
            {
                return;
            }

            if (miniGameDefinition == null)
            {
                Debug.LogError("GameManager: MiniGameDefinition is null.");
                return;
            }

            if (!miniGameDefinition.SupportsProfile(activeProfile))
            {
                Debug.LogError(
                    $"GameManager: MiniGame '{miniGameDefinition.MiniGameId}' is not enabled for profile '{activeProfile.DisplayName}'."
                );
                return;
            }

            MiniGameConfig miniGameConfig = miniGameDefinition.MiniGameConfig;
            DifficultyConfig difficultyConfig = miniGameDefinition.GetDifficultyByLevel(levelNumber);

            if (miniGameConfig == null || difficultyConfig == null)
            {
                Debug.LogError("GameManager: Cannot run mini game because config or difficulty is missing.");
                return;
            }

            dataLogger.LogSimpleEvent(
                "game_manager_run_minigame",
                miniGameConfig.MiniGameId,
                difficultyConfig.DifficultyId,
                Field.Of("miniGameName", miniGameConfig.DisplayName),
                Field.Of("levelNumber", difficultyConfig.LevelNumber),
                Field.Of("difficultyName", difficultyConfig.DisplayName),
                Field.Of("categoryLabel", difficultyConfig.CategoryLabel),
                Field.Of("profileId", activeProfile.ProfileId)
            );

            miniGameRunner.RunMiniGame(
                miniGameConfig,
                difficultyConfig,
                result =>
                {
                    OnMiniGameCompleted(result);
                    onCompleted?.Invoke(result);
                }
            );
        }

        private void OnMiniGameCompleted(MiniGameResult result)
        {
            Debug.Log("GameManager: Mini game completed.");
            Debug.Log($"MiniGame ID: {result.MiniGameId}");
            Debug.Log($"Completed: {result.IsCompleted}");
            Debug.Log($"Score: {result.Score}");

            foreach (var item in result.ExtraData)
            {
                Debug.Log($"Extra Data | {item.Key}: {item.Value}");
            }
        }

        private bool ValidateCoreReferences()
        {
            if (database == null)
            {
                Debug.LogError("GameManager: NeuroQuestDatabase is not assigned.");
                return false;
            }

            if (activeProfile == null)
            {
                Debug.LogError("GameManager: AssessmentProfile is not assigned.");
                return false;
            }

            if (miniGameRunner == null)
            {
                Debug.LogError("GameManager: MiniGameRunner is not assigned and not registered.");
                return false;
            }

            if (dataLogger == null)
            {
                Debug.LogError("GameManager: DataLogger is not assigned and not registered.");
                return false;
            }

            return true;
        }
    }
}