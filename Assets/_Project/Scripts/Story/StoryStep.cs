using System;
using System.Collections.Generic;
using UnityEngine;

namespace NeuroQuest.Story
{
    [Serializable]
    public class StoryStep
    {
        [Header("Identity")]
        [SerializeField] private string stepId;

        [Header("Step Type")]
        [SerializeField] private StoryStepType stepType;

        [Header("Dialogue / Choice Question")]
        [TextArea]
        [SerializeField] private string dialogueText;

        [Header("Choice Options")]
        [SerializeField] private List<ChoiceOption> choiceOptions = new();

        [Header("Mini Game")]
        [SerializeField] private string miniGameId;
        [SerializeField] private int levelNumber = 10;

        [Header("Wait")]
        [SerializeField] private float waitDuration = 1f;

        [Header("End / Navigation")]
        [SerializeField] private string nextScenarioId;

        public string StepId => stepId;
        public StoryStepType StepType => stepType;
        public string DialogueText => dialogueText;
        public IReadOnlyList<ChoiceOption> ChoiceOptions => choiceOptions;
        public string MiniGameId => miniGameId;
        public int LevelNumber => levelNumber;
        public float WaitDuration => waitDuration;
        public string NextScenarioId => nextScenarioId;
    }
}