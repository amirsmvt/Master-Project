using System;
using NeuroQuest.Data;
using UnityEngine;

namespace NeuroQuest.MiniGames.Common
{
    public abstract class BaseMiniGame : MonoBehaviour
    {
        protected MiniGameConfig Config { get; private set; }
        protected DifficultyConfig Difficulty { get; private set; }
        protected DataLogger DataLogger { get; private set; }
        protected Action<MiniGameResult> OnCompleted { get; private set; }

        public virtual void Setup(
            MiniGameConfig config,
            DifficultyConfig difficulty,
            DataLogger dataLogger,
            Action<MiniGameResult> onCompleted)
        {
            Config = config;
            Difficulty = difficulty;
            DataLogger = dataLogger;
            OnCompleted = onCompleted;
        }

        public abstract void StartGame();

        protected void CompleteGame(MiniGameResult result)
        {
            OnCompleted?.Invoke(result);
        }
    }
}