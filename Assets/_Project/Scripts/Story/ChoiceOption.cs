using System;
using UnityEngine;

namespace NeuroQuest.Story
{
    [Serializable]
    public class ChoiceOption
    {
        [Header("Identity")]
        [SerializeField] private string optionId;

        [Header("Display")]
        [SerializeField] private string displayText;

        [Header("Navigation")]
        [SerializeField] private StoryScenario nextScenario;

        public string OptionId => optionId;
        public string DisplayText => displayText;
        public StoryScenario NextScenario => nextScenario;
        public bool HasNextScenario => nextScenario != null;
    }
}