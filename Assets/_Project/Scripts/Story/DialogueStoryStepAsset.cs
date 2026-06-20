using UnityEngine;

namespace NeuroQuest.Story
{
    [CreateAssetMenu(menuName = "NeuroQuest/Story Steps/Dialogue Step")]
    public class DialogueStoryStepAsset : StoryStepAsset
    {
        [Header("Dialogue")]
        [SerializeField] private string speakerName;
        [TextArea]
        [SerializeField] private string dialogueText;
        [SerializeField] private Sprite portrait;
        [SerializeField] private string continueButtonText = "ادامه";

        public override StoryStepType StepType => StoryStepType.Dialogue;
        public string SpeakerName => speakerName;
        public string DialogueText => dialogueText;
        public Sprite Portrait => portrait;
        public string ContinueButtonText => continueButtonText;
    }
}
