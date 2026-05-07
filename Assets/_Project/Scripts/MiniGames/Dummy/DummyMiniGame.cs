using System.Collections;
using NeuroQuest.MiniGames.Common;
using UnityEngine;

namespace NeuroQuest.MiniGames.Dummy
{
    public class DummyMiniGame : BaseMiniGame
    {
        [Header("Dummy Settings")]
        [SerializeField] private float finishDelay = 2f;
        [SerializeField] private int testScore = 100;

        public override void StartGame()
        {
            Debug.Log("DummyMiniGame: Started.");
            StartCoroutine(FinishAfterDelay());
        }

        private IEnumerator FinishAfterDelay()
        {
            yield return new WaitForSeconds(finishDelay);

            MiniGameResult result = new MiniGameResult(
                Config.MiniGameId,
                isCompleted: true,
                score: testScore
            );

            result.AddExtraData("message", "Dummy mini game completed successfully.");
            result.AddExtraData("finishDelay", finishDelay);

            Debug.Log("DummyMiniGame: Completed.");

            CompleteGame(result);
        }
    }
}