using UnityEngine;

namespace NeuroQuest.Story
{
    [CreateAssetMenu(menuName = "NeuroQuest/Story Steps/End Step")]
    public class EndStoryStepAsset : StoryStepAsset
    {
        [Header("Navigation")]
        [SerializeField] private StoryScenario nextScenario;

        public override StoryStepType StepType => StoryStepType.End;
        public StoryScenario NextScenario => nextScenario;
        public bool HasNextScenario => nextScenario != null;
    }
}