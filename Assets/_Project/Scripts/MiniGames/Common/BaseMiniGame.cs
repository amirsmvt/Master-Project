using System;
using UnityEngine;

namespace NeuroQuest.MiniGames.Common
{
    public abstract class BaseMiniGame : MonoBehaviour
    {
        protected MiniGameConfig Config { get; private set; }
        protected Action<MiniGameResult> OnCompleted { get; private set; }

        public virtual void Setup(
            MiniGameConfig config,
            Action<MiniGameResult> onCompleted)
        {
            Config = config;
            OnCompleted = onCompleted;
        }

        public abstract void StartGame();

        protected void CompleteGame(MiniGameResult result)
        {
            OnCompleted?.Invoke(result);
        }
    }
}