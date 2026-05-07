using NeuroQuest.MiniGames.Common;
using UnityEngine;

namespace NeuroQuest.Core
{
    public class TestFlowManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MiniGameRunner miniGameRunner;

        [Header("Test Config")]
        [SerializeField] private MiniGameConfig dummyMiniGameConfig;

        private void Start()
        {
            if (miniGameRunner == null)
            {
                Debug.LogError("TestFlowManager: MiniGameRunner is not assigned.");
                return;
            }

            if (dummyMiniGameConfig == null)
            {
                Debug.LogError("TestFlowManager: DummyMiniGameConfig is not assigned.");
                return;
            }

            Debug.Log("TestFlowManager: Starting dummy mini game...");

            miniGameRunner.RunMiniGame(dummyMiniGameConfig, OnMiniGameCompleted);
        }

        private void OnMiniGameCompleted(MiniGameResult result)
        {
            Debug.Log("TestFlowManager: Mini game completed.");
            Debug.Log($"MiniGame ID: {result.MiniGameId}");
            Debug.Log($"Completed: {result.IsCompleted}");
            Debug.Log($"Score: {result.Score}");

            foreach (var item in result.ExtraData)
            {
                Debug.Log($"Extra Data | {item.Key}: {item.Value}");
            }
        }
    }
}