using System.Collections.Generic;
using UnityEngine;

namespace NeuroQuest.Story
{
    [CreateAssetMenu(menuName = "NeuroQuest/Story Steps/Choice Step")]
    public class ChoiceStoryStepAsset : StoryStepAsset
    {
        [Header("Question")]
        [TextArea]
        [SerializeField] private string questionText;

        [Header("Options")]
        [SerializeField] private List<ChoiceOption> choiceOptions = new();

        public override StoryStepType StepType => StoryStepType.Choice;
        public string QuestionText => questionText;
        public IReadOnlyList<ChoiceOption> ChoiceOptions => choiceOptions;
    }
}