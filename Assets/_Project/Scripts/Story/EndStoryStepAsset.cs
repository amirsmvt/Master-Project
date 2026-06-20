using UnityEngine;

namespace NeuroQuest.Story
{
    public enum StoryEndingMode
    {
        EndSession = 0,
        ReturnToWorld = 1
    }

    [CreateAssetMenu(menuName = "NeuroQuest/Story Steps/End Step")]
    public class EndStoryStepAsset : StoryStepAsset
    {
        [Header("Ending")]
        [SerializeField] private StoryEndingMode endingMode = StoryEndingMode.EndSession;

        [Header("Navigation")]
        [SerializeField] private StoryScenario nextScenario;

        public override StoryStepType StepType => StoryStepType.End;
        public StoryEndingMode EndingMode => endingMode;
        public StoryScenario NextScenario => nextScenario;
        public bool HasNextScenario => nextScenario != null;
    }
}
