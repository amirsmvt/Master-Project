using System.Collections;
using System;
using NeuroQuest.Core;
using NeuroQuest.Data;
using NeuroQuest.Dialogue;
using NeuroQuest.MiniGames.Common;
using NeuroQuest.Services;
using UnityEngine;

namespace NeuroQuest.Story
{
    public class StoryRunner : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private DialogueUI dialogueUI;
        [SerializeField] private ChoiceUI choiceUI;
        [SerializeField] private GameManager gameManager;
        [SerializeField] private DataLogger dataLogger;
        [SerializeField] private ExportManager exportManager;

        private StoryScenario currentScenario;
        private int currentStepIndex;
        private bool isRunning;
        private Action currentScenarioCompleted;

        private void Awake()
        {
            ServiceLocator.Register(this);
        }

        private void ResolveServices()
        {
            if (gameManager == null)
            {
                gameManager = ServiceLocator.Get<GameManager>();
            }

            if (dataLogger == null)
            {
                dataLogger = ServiceLocator.Get<DataLogger>();
            }
            if (exportManager == null)
            {
                exportManager = ServiceLocator.Get<ExportManager>();
            }
        }

        public void StartStory(StoryScenario scenario)
        {
            StartScenario(scenario, null);
        }

        public void StartScenario(StoryScenario scenario, Action onCompleted)
        {
            ResolveServices();

            if (!ValidateBaseReferences())
            {
                return;
            }

            if (scenario == null)
            {
                Debug.LogError("StoryRunner: Cannot start null scenario.");
                return;
            }

            currentScenario = scenario;
            currentStepIndex = 0;
            isRunning = true;
            currentScenarioCompleted = onCompleted;

            dataLogger.LogSimpleEvent(
                "story_started",
                "",
                "",
                Field.Of("scenarioId", currentScenario.ScenarioId),
                Field.Of("scenarioName", currentScenario.DisplayName)
            );

            Debug.Log($"StoryRunner: Started scenario '{currentScenario.DisplayName}'.");

            RunCurrentStep();
        }

        private void RunCurrentStep()
        {
            if (!isRunning)
            {
                return;
            }

            if (currentScenario == null)
            {
                Debug.LogError("StoryRunner: Current scenario is null.");
                EndStory();
                return;
            }

            if (currentStepIndex >= currentScenario.Steps.Count)
            {
                EndStory();
                return;
            }

            StoryStepAsset step = currentScenario.Steps[currentStepIndex];

            if (step == null)
            {
                Debug.LogError($"StoryRunner: Step at index {currentStepIndex} is null.");
                GoToNextStep();
                return;
            }

            dataLogger.LogSimpleEvent(
                "story_step_start",
                "",
                "",
                Field.Of("scenarioId", currentScenario.ScenarioId),
                Field.Of("stepId", step.StepId),
                Field.Of("stepType", step.StepType)
            );

            switch (step.StepType)
            {
                case StoryStepType.Dialogue:
                    RunDialogueStep(step as DialogueStoryStepAsset);
                    break;

                case StoryStepType.PlayMiniGame:
                    RunMiniGameStep(step as MiniGameStoryStepAsset);
                    break;

                case StoryStepType.Wait:
                    StartCoroutine(RunWaitStep(step as WaitStoryStepAsset));
                    break;

                case StoryStepType.Choice:
                    RunChoiceStep(step as ChoiceStoryStepAsset);
                    break;

                case StoryStepType.End:
                    RunEndStep(step as EndStoryStepAsset);
                    break;

                default:
                    Debug.LogError($"StoryRunner: Unsupported step type: {step.StepType}");
                    GoToNextStep();
                    break;
            }
        }

        private void RunDialogueStep(DialogueStoryStepAsset step)
        {
            if (step == null)
            {
                Debug.LogError("StoryRunner: Dialogue step is invalid.");
                GoToNextStep();
                return;
            }

            dialogueUI.Show(
                step.SpeakerName,
                step.DialogueText,
                step.Portrait,
                step.ContinueButtonText,
                GoToNextStep);
        }

        private void RunMiniGameStep(MiniGameStoryStepAsset step)
        {
            if (step == null)
            {
                Debug.LogError("StoryRunner: MiniGame step is invalid.");
                GoToNextStep();
                return;
            }

            MiniGameDefinition definition = step.MiniGameDefinition;

            if (definition == null)
            {
                Debug.LogError($"StoryRunner: MiniGameDefinition is missing in step: {step.StepId}");
                GoToNextStep();
                return;
            }

            gameManager.RunMiniGame(
                definition,
                step.LevelNumber,
                result =>
                {
                    dataLogger.LogSimpleEvent(
                        "story_minigame_completed",
                        result.MiniGameId,
                        "",
                        Field.Of("scenarioId", currentScenario.ScenarioId),
                        Field.Of("stepId", step.StepId),
                        Field.Of("score", result.Score),
                        Field.Of("completed", result.IsCompleted)
                    );

                    GoToNextStep();
                }
            );
        }

        private IEnumerator RunWaitStep(WaitStoryStepAsset step)
        {
            if (step == null)
            {
                Debug.LogError("StoryRunner: Wait step is invalid.");
                GoToNextStep();
                yield break;
            }

            yield return new WaitForSeconds(step.WaitDuration);
            GoToNextStep();
        }

        private void RunChoiceStep(ChoiceStoryStepAsset step)
        {
            if (step == null)
            {
                Debug.LogError("StoryRunner: Choice step is invalid.");
                GoToNextStep();
                return;
            }

            if (choiceUI == null)
            {
                Debug.LogError("StoryRunner: ChoiceUI is not assigned.");
                GoToNextStep();
                return;
            }

            choiceUI.Show(
                step.QuestionText,
                step.ChoiceOptions,
                selectedOption =>
                {
                    string nextScenarioId = selectedOption.NextScenario != null
                        ? selectedOption.NextScenario.ScenarioId
                        : string.Empty;

                    dataLogger.LogSimpleEvent(
                        "story_choice_selected",
                        "",
                        "",
                        Field.Of("scenarioId", currentScenario.ScenarioId),
                        Field.Of("stepId", step.StepId),
                        Field.Of("optionId", selectedOption.OptionId),
                        Field.Of("optionText", selectedOption.DisplayText),
                        Field.Of("nextScenarioId", nextScenarioId)
                    );

                    if (selectedOption.HasNextScenario)
                    {
                        EndCurrentScenarioBeforeTransition(selectedOption.NextScenario, "choice");
                        StartScenario(selectedOption.NextScenario, currentScenarioCompleted);
                    }
                    else
                    {
                        GoToNextStep();
                    }
                }
            );
        }

        private void RunEndStep(EndStoryStepAsset step)
        {
            if (step == null)
            {
                Debug.LogError("StoryRunner: End step is invalid.");
                EndStory();
                return;
            }

            if (step.HasNextScenario)
            {
                EndCurrentScenarioBeforeTransition(step.NextScenario, "end_step");
                StartScenario(step.NextScenario, currentScenarioCompleted);
                return;
            }

            if (step.EndingMode == StoryEndingMode.ReturnToWorld)
            {
                EndScenarioAndReturnToWorld();
                return;
            }

            EndStory();
        }

        private void GoToNextStep()
        {
            if (currentScenario != null && currentStepIndex < currentScenario.Steps.Count)
            {
                StoryStepAsset step = currentScenario.Steps[currentStepIndex];

                if (step != null)
                {
                    dataLogger.LogSimpleEvent(
                        "story_step_end",
                        "",
                        "",
                        Field.Of("scenarioId", currentScenario.ScenarioId),
                        Field.Of("stepId", step.StepId),
                        Field.Of("stepType", step.StepType)
                    );
                }
            }

            currentStepIndex++;
            RunCurrentStep();
        }

        private void EndCurrentScenarioBeforeTransition(StoryScenario nextScenario, string reason)
        {
            if (currentScenario == null)
            {
                return;
            }

            dataLogger.LogSimpleEvent(
                "story_transition",
                "",
                "",
                Field.Of("fromScenarioId", currentScenario.ScenarioId),
                Field.Of("nextScenarioId", nextScenario != null ? nextScenario.ScenarioId : string.Empty),
                Field.Of("reason", reason)
            );

            dataLogger.LogSimpleEvent(
                "story_scenario_exited",
                "",
                "",
                Field.Of("scenarioId", currentScenario.ScenarioId),
                Field.Of("scenarioName", currentScenario.DisplayName)
            );
        }

        private void EndStory()
        {
            if (!isRunning)
            {
                return;
            }

            isRunning = false;
            Action callback = currentScenarioCompleted;
            currentScenarioCompleted = null;

            if (currentScenario != null)
            {
                dataLogger.LogSimpleEvent(
                    "story_ended",
                    "",
                    "",
                    Field.Of("scenarioId", currentScenario.ScenarioId),
                    Field.Of("scenarioName", currentScenario.DisplayName)
                );
            }

            Debug.Log("StoryRunner: Story ended.");
            callback?.Invoke();
            dataLogger.PrintSessionSummary();

            if (exportManager == null)
            {
                exportManager = ServiceLocator.Get<ExportManager>();
            }

            if (exportManager != null)
            {
                exportManager.ExportCurrentSession();
            }
            else
            {
                Debug.LogWarning("StoryRunner: ExportManager is not assigned and not registered. Session was not exported.");
            }
        }

        private void EndScenarioAndReturnToWorld()
        {
            if (!isRunning)
            {
                return;
            }

            isRunning = false;
            Action callback = currentScenarioCompleted;
            currentScenarioCompleted = null;

            if (currentScenario != null)
            {
                dataLogger.LogSimpleEvent(
                    "story_scenario_returned_to_world",
                    "",
                    "",
                    Field.Of("scenarioId", currentScenario.ScenarioId),
                    Field.Of("scenarioName", currentScenario.DisplayName)
                );
            }

            dialogueUI.Hide();
            choiceUI?.Hide();

            Debug.Log("StoryRunner: Scenario returned to world.");
            callback?.Invoke();
        }

        private bool ValidateBaseReferences()
        {
            if (dialogueUI == null)
            {
                Debug.LogError("StoryRunner: DialogueUI is not assigned.");
                return false;
            }

            if (gameManager == null)
            {
                Debug.LogError("StoryRunner: GameManager is not assigned and not registered.");
                return false;
            }

            if (dataLogger == null)
            {
                Debug.LogError("StoryRunner: DataLogger is not assigned and not registered.");
                return false;
            }

            return true;
        }
    }
}
