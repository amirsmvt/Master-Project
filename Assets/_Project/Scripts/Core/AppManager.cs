using System.Collections;
using NeuroQuest.Services;
using NeuroQuest.UI;
using NeuroQuest.World;
using UnityEngine;

namespace NeuroQuest.Core
{
    public class AppManager : MonoBehaviour
    {
        [Header("Core References")]
        [SerializeField] private GameManager gameManager;
        [SerializeField] private SessionManager sessionManager;

        [Header("UI Screens")]
        [SerializeField] private UIScreen splashScreen;
        [SerializeField] private LoadingScreenUI loadingScreen;
        [SerializeField] private UIScreen introScreen;

        [Header("World")]
        [SerializeField] private GameObject worldRoot;
        [SerializeField] private PlayerController2D playerController;
        [SerializeField] private bool startStoryOnBoot = true;

        [Header("Timing")]
        [SerializeField] private float splashDuration = 1.5f;
        [SerializeField] private float loadingDuration = 1.5f;
        [SerializeField] private float introDuration = 1.5f;

        public AppState CurrentState { get; private set; } = AppState.None;

        private bool sessionStarted;

        private void Awake()
        {
            ServiceLocator.Register(this);
        }

        private void Start()
        {
            ResolveServices();

            if (!ValidateReferences())
            {
                return;
            }

            PrepareWorldForStartup();
            StartCoroutine(BootSequence());
        }

        private void ResolveServices()
        {
            if (gameManager == null)
            {
                gameManager = ServiceLocator.Get<GameManager>();
            }

            if (sessionManager == null)
            {
                sessionManager = ServiceLocator.Get<SessionManager>();
            }
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

            if (startStoryOnBoot)
            {
                StartGameFlow();
            }
            else
            {
                EnterWorld();
            }
        }

        private void StartGameFlow()
        {
            SetState(AppState.Story);

            StartSessionIfNeeded();
            gameManager.StartStory();
        }

        private void EnterWorld()
        {
            HideAllScreens();
            StartSessionIfNeeded();

            if (worldRoot != null)
            {
                worldRoot.SetActive(true);
            }

            if (playerController != null)
            {
                playerController.SetMovementEnabled(true);
            }

            SetState(AppState.World);
        }

        private void StartSessionIfNeeded()
        {
            if (sessionStarted)
            {
                return;
            }

            sessionManager.StartTestSession();
            sessionStarted = true;
        }

        private void PrepareWorldForStartup()
        {
            if (playerController != null)
            {
                playerController.SetMovementEnabled(false);
            }

            if (!startStoryOnBoot && worldRoot != null)
            {
                worldRoot.SetActive(false);
            }
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
                Debug.LogError("AppManager: GameManager is not assigned and not registered.");
                return false;
            }

            if (sessionManager == null)
            {
                Debug.LogError("AppManager: SessionManager is not assigned and not registered.");
                return false;
            }

            return true;
        }
    }
}
