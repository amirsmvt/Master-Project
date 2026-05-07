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
        [SerializeField] private MiniGameRunner miniGameRunner;
        [SerializeField] private DataLogger dataLogger;

        [Header("Temporary Test Settings")]
        [SerializeField] private string testMiniGameId = "dummy";
        [SerializeField] private string testDifficultyId = "dummy_level_10";

        private void Awake()
        {
            ServiceLocator.Register(this);
        }

        private void Start()
        {
            if (!ValidateReferences())
            {
                return;
            }

            dataLogger.SetParticipantInfo("P001", "test");

            RunMiniGameById(testMiniGameId, testDifficultyId);
        }

        public void RunMiniGameById(string miniGameId, string difficultyId)
        {
            MiniGameConfig miniGameConfig = database.GetMiniGameById(miniGameId);
            DifficultyConfig difficultyConfig = database.GetDifficultyById(difficultyId);

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
                Field.Of("difficultyName", difficultyConfig.DisplayName),
                Field.Of("levelNumber", difficultyConfig.LevelNumber),
                Field.Of("categoryLabel", difficultyConfig.CategoryLabel)
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