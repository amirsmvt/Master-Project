using NeuroQuest.MiniGames.Common;
using UnityEngine;

namespace NeuroQuest.Story
{
    [CreateAssetMenu(menuName = "NeuroQuest/Story Steps/Mini Game Step")]
    public class MiniGameStoryStepAsset : StoryStepAsset
    {
        [Header("Mini Game")]
        [SerializeField] private MiniGameDefinition miniGameDefinition;

        [Header("Level")]
        [SerializeField] private int levelNumber = 10;

        public override StoryStepType StepType => StoryStepType.PlayMiniGame;
        public MiniGameDefinition MiniGameDefinition => miniGameDefinition;
        public int LevelNumber => levelNumber;
    }
}