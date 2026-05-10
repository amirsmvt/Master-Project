using System.Collections.Generic;
using UnityEngine;

namespace NeuroQuest.Story
{
    [CreateAssetMenu(menuName = "NeuroQuest/Story Scenario")]
    public class StoryScenario : ScriptableObject
    {
        [Header("Scenario Info")]
        [SerializeField] private string scenarioId;
        [SerializeField] private string displayName;

        [Header("Steps")]
        [SerializeField] private List<StoryStepAsset> steps = new();

        public string ScenarioId => scenarioId;
        public string DisplayName => displayName;
        public IReadOnlyList<StoryStepAsset> Steps => steps;
    }
}