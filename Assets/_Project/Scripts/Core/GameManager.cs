using NeuroQuest.Data;
using NeuroQuest.MiniGames.Common;
using NeuroQuest.Services;
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

        [Header("Temporary Test Settings")]
        [SerializeField] private string testMiniGameId = "dummy";
        [SerializeField] private int testLevelNumber = 10;

        private bool sessionStarted;

        private void Awake()
        {
            ServiceLocator.Register(this);
        }

        public void StartGameSession(string participantId, string groupLabel)
        {
            if (!ValidateReferences())
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

        public void RunConfiguredTestMiniGame()
        {
            if (!sessionStarted)
            {
                Debug.LogError("GameManager: Cannot run mini game before session starts.");
                return;
            }

            RunMiniGameByLevel(testMiniGameId, testLevelNumber);
        }

        public void RunMiniGameByLevel(string miniGameId, int levelNumber)
        {
            if (!ValidateReferences())
            {
                return;
            }

            MiniGameConfig miniGameConfig = database.GetMiniGameConfigById(
                miniGameId,
                activeProfile
            );

            DifficultyConfig difficultyConfig = database.GetDifficultyByLevel(
                miniGameId,
                levelNumber,
                activeProfile
            );

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
                OnMiniGameCompleted
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

            dataLogger.PrintSessionSummary();
        }

        private bool ValidateReferences()
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
                Debug.LogError("GameManager: MiniGameRunner is not assigned.");
                return false;
            }

            if (dataLogger == null)
            {
                Debug.LogError("GameManager: DataLogger is not assigned.");
                return false;
            }

            return true;
        }
    }
}