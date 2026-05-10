using System.Collections;
using NeuroQuest.Services;
using NeuroQuest.UI;
using UnityEngine;

namespace NeuroQuest.Core
{
    public class AppManager : MonoBehaviour
    {
        [Header("Core References")]
        [SerializeField] private GameManager gameManager;

        [Header("UI Screens")]
        [SerializeField] private UIScreen splashScreen;
        [SerializeField] private LoadingScreenUI loadingScreen;
        [SerializeField] private UIScreen introScreen;

        [Header("Timing")]
        [SerializeField] private float splashDuration = 1.5f;
        [SerializeField] private float loadingDuration = 1.5f;
        [SerializeField] private float introDuration = 1.5f;

        [Header("Temporary Session Info")]
        [SerializeField] private string testParticipantId = "P001";
        [SerializeField] private string testGroupLabel = "test";

        public AppState CurrentState { get; private set; } = AppState.None;

        private void Awake()
        {
            ServiceLocator.Register(this);
        }

        private void Start()
        {
            if (!ValidateReferences())
            {
                return;
            }

            StartCoroutine(BootSequence());
        }

        private IEnumerator BootSequence()
        {
            HideAllScreens();

            SetState(AppState.Splash);

            if (splashScreen != null)
            {
                splashScreen.Show();
            }

            yield return new WaitForSeconds(splashDuration);

            if (splashScreen != null)
            {
                splashScreen.Hide();
            }

            SetState(AppState.Loading);

            if (loadingScreen != null)
            {
                loadingScreen.Show();
            }

            float elapsed = 0f;

            while (elapsed < loadingDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / loadingDuration;

                if (loadingScreen != null)
                {
                    loadingScreen.SetProgress(progress);
                }

                yield return null;
            }

            if (loadingScreen != null)
            {
                loadingScreen.SetProgress(1f);
            }

            yield return new WaitForSeconds(0.2f);

            if (loadingScreen != null)
            {
                loadingScreen.Hide();
            }

            SetState(AppState.Intro);

            if (introScreen != null)
            {
                introScreen.Show();
            }

            yield return new WaitForSeconds(introDuration);

            if (introScreen != null)
            {
                introScreen.Hide();
            }

            StartGameFlow();
        }

        private void StartGameFlow()
        {
            SetState(AppState.Story);

            gameManager.StartGameSession(testParticipantId, testGroupLabel);
            gameManager.StartStory();
        }

        private void SetState(AppState newState)
        {
            CurrentState = newState;
            Debug.Log($"AppManager: State changed to {CurrentState}");
        }

        private void HideAllScreens()
        {
            if (splashScreen != null)
            {
                splashScreen.Hide();
            }

            if (loadingScreen != null)
            {
                loadingScreen.Hide();
            }

            if (introScreen != null)
            {
                introScreen.Hide();
            }
        }

        private bool ValidateReferences()
        {
            if (gameManager == null)
            {
                Debug.LogError("AppManager: GameManager is not assigned.");
                return false;
            }

            return true;
        }
    }
}