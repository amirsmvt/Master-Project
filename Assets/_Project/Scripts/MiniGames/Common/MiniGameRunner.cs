using System;
using UnityEngine;

namespace NeuroQuest.MiniGames.Common
{
    public class MiniGameRunner : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform miniGameParent;

        private BaseMiniGame currentMiniGame;

        public void RunMiniGame(
            MiniGameConfig config,
            Action<MiniGameResult> onCompleted)
        {
            if (config == null)
            {
                Debug.LogError("MiniGameRunner: MiniGameConfig is null.");
                return;
            }

            if (config.MiniGamePrefab == null)
            {
                Debug.LogError($"MiniGameRunner: MiniGamePrefab is null for config: {config.name}");
                return;
            }

            if (currentMiniGame != null)
            {
                Destroy(currentMiniGame.gameObject);
                currentMiniGame = null;
            }

            Transform parent = miniGameParent != null ? miniGameParent : transform;

            currentMiniGame = Instantiate(config.MiniGamePrefab, parent);
            currentMiniGame.Setup(config, result =>
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