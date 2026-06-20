using NeuroQuest.Services;
using NeuroQuest.Story;
using NeuroQuest.UI;
using UnityEngine;

namespace NeuroQuest.World
{
    [RequireComponent(typeof(Collider2D))]
    public class StoryInteractionTrigger : MonoBehaviour
    {
        [SerializeField] private StoryScenario scenario;
        [SerializeField] private InteractionPromptUI promptUI;
        [SerializeField] private PlayerController2D playerController;
        [SerializeField] private string promptText = "برای صحبت کردن، E را فشار بده";

        private StoryRunner storyRunner;
        private bool playerInRange;
        private bool interactionActive;

        private void Awake()
        {
            Collider2D triggerCollider = GetComponent<Collider2D>();
            triggerCollider.isTrigger = true;
        }

        private void Start()
        {
            if (storyRunner == null)
            {
                storyRunner = ServiceLocator.Get<StoryRunner>();
            }
        }

        private void Update()
        {
            if (!playerInRange || interactionActive)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space))
            {
                BeginInteraction();
            }
        }

        public void ReenablePlayerMovement()
        {
            interactionActive = false;

            if (playerController != null)
            {
                playerController.SetMovementEnabled(true);
            }

            if (playerInRange && promptUI != null)
            {
                promptUI.Show(promptText);
            }
        }

        private void BeginInteraction()
        {
            interactionActive = true;

            if (promptUI != null)
            {
                promptUI.Hide();
            }

            if (playerController != null)
            {
                playerController.SetMovementEnabled(false);
            }

            if (scenario == null)
            {
                Debug.LogWarning($"{nameof(StoryInteractionTrigger)}: No StoryScenario assigned on {name}.");
                ReenablePlayerMovement();
                return;
            }

            if (storyRunner == null)
            {
                storyRunner = ServiceLocator.Get<StoryRunner>();
            }

            if (storyRunner == null)
            {
                Debug.LogError($"{nameof(StoryInteractionTrigger)}: StoryRunner is not available.");
                ReenablePlayerMovement();
                return;
            }

            storyRunner.StartScenario(scenario, ReenablePlayerMovement);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            PlayerController2D enteringPlayer = other.GetComponent<PlayerController2D>();
            if (enteringPlayer == null)
            {
                return;
            }

            playerController = enteringPlayer;
            playerInRange = true;

            if (!interactionActive && promptUI != null)
            {
                promptUI.Show(promptText);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            PlayerController2D exitingPlayer = other.GetComponent<PlayerController2D>();
            if (exitingPlayer == null || exitingPlayer != playerController)
            {
                return;
            }

            playerInRange = false;

            if (promptUI != null)
            {
                promptUI.Hide();
            }
        }
    }
}
