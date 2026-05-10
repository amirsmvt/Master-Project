using UnityEngine;

namespace NeuroQuest.Story
{
    public abstract class StoryStepAsset : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string stepId;

        public string StepId => stepId;
        public abstract StoryStepType StepType { get; }
    }
}