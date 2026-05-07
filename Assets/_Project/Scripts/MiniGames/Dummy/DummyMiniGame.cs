using System.Collections;
using NeuroQuest.Data;
using NeuroQuest.MiniGames.Common;
using UnityEngine;

namespace NeuroQuest.MiniGames.Dummy
{
    public class DummyMiniGame : BaseMiniGame
    {
        [Header("Fallback Settings")]
        [SerializeField] private float fallbackFinishDelay = 2f;
        [SerializeField] private int fallbackScore = 100;

        public override void StartGame()
        {
            float finishDelay = Difficulty.GetFloat("finishDelay", fallbackFinishDelay);

            Debug.Log($"DummyMiniGame: Started with difficulty: {Difficulty.DifficultyId}");

            DataLogger.LogSimpleEvent(
                "minigame_start",
                Config.MiniGameId,
                Difficulty.DifficultyId,
                Field.Of("miniGameName", Config.DisplayName),
                Field.Of("difficultyName", Difficulty.DisplayName),
                Field.Of("finishDelay", finishDelay)
            );

            StartCoroutine(FinishAfterDelay(finishDelay));
        }

        private IEnumerator FinishAfterDelay(float finishDelay)
        {
            yield return new WaitForSeconds(finishDelay);

            int score = Difficulty.GetInt("score", fallbackScore);

            DataLogger.LogSimpleEvent(
                "trial",
                Config.MiniGameId,
                Difficulty.DifficultyId,
                Field.Of("trialIndex", 1),
                Field.Of("isCorrect", true),
                Field.Of("reactionTime", finishDelay),
                Field.Of("score", score)
            );

            MiniGameResult result = new MiniGameResult(
                Config.MiniGameId,
                isCompleted: true,
                score: score
            );

            result.AddExtraData("message", "Dummy mini game completed successfully.");
            result.AddExtraData("difficulty", Difficulty.DifficultyId);
            result.AddExtraData("finishDelay", finishDelay);

            DataLogger.LogSimpleEvent(
                "minigame_end",
                Config.MiniGameId,
                Difficulty.DifficultyId,
                Field.Of("score", score),
                Field.Of("completed", true)
            );

            Debug.Log("DummyMiniGame: Completed.");

            CompleteGame(result);
        }
    }
}