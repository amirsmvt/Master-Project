using System;
using NeuroQuest.Data;
using NeuroQuest.Services;
using UnityEngine;

namespace NeuroQuest.MiniGames.Common
{
    public class MiniGameRunner : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform miniGameParent;
        [SerializeField] private DataLogger dataLogger;

        private BaseMiniGame currentMiniGame;

        private void Awake()
        {
            ServiceLocator.Register(this);
        }

        public void RunMiniGame(
            MiniGameConfig config,
            DifficultyConfig difficulty,
            Action<MiniGameResult> onCompleted)
        {
            if (config == null)
            {
                Debug.LogError("MiniGameRunner: MiniGameConfig is null.");
                return;
            }

            if (difficulty == null)
            {
                Debug.LogError($"MiniGameRunner: DifficultyConfig is null for mini game: {config.name}");
                return;
            }

            if (config.MiniGamePrefab == null)
            {
                Debug.LogError($"MiniGameRunner: MiniGamePrefab is null for config: {config.name}");
                return;
            }

            if (dataLogger == null)
            {
                dataLogger = ServiceLocator.Get<DataLogger>();
            }

            if (dataLogger == null)
            {
                Debug.LogError("MiniGameRunner: DataLogger is not assigned and not registered.");
                return;
            }

            if (currentMiniGame != null)
            {
                Destroy(currentMiniGame.gameObject);
                currentMiniGame = null;
            }

            Transform parent = miniGameParent != null ? miniGameParent : transform;

            currentMiniGame = Instantiate(config.MiniGamePrefab, parent);

            dataLogger.LogSimpleEvent(
                "minigame_created",
                config.MiniGameId,
                difficulty.DifficultyId,
                Field.Of("displayName", config.DisplayName),
                Field.Of("difficultyName", difficulty.DisplayName)
            );

            currentMiniGame.Setup(config, difficulty, dataLogger, result =>
            {
                if (currentMiniGame != null)
                {
                    Destroy(currentMiniGame.gameObject);
                    currentMiniGame = null;
                }

                onCompleted?.Invoke(result);
            });

            currentMiniGame.StartGame();
        }
    }
}