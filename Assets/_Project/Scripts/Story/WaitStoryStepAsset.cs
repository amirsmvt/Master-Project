using UnityEngine;

namespace NeuroQuest.Story
{
    [CreateAssetMenu(menuName = "NeuroQuest/Story Steps/Wait Step")]
    public class WaitStoryStepAsset : StoryStepAsset
    {
        [Header("Wait")]
        [SerializeField] private float waitDuration = 1f;

        public override StoryStepType StepType => StoryStepType.Wait;
        public float WaitDuration => waitDuration;
    }
}