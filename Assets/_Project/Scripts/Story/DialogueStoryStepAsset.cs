using UnityEngine;

namespace NeuroQuest.Story
{
    [CreateAssetMenu(menuName = "NeuroQuest/Story Steps/Dialogue Step")]
    public class DialogueStoryStepAsset : StoryStepAsset
    {
        [Header("Dialogue")]
        [TextArea]
        [SerializeField] private string dialogueText;

        public override StoryStepType StepType => StoryStepType.Dialogue;
        public string DialogueText => dialogueText;
    }
}