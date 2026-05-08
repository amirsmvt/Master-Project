using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NeuroQuest.UI
{
    public class LoadingScreenUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Slider progressSlider;
        [SerializeField] private TextMeshProUGUI progressText;

        [Header("Screen Settings")]
        [SerializeField] private bool disableGameObjectOnHide = true;

        private CanvasGroup canvasGroup;

        private void Awake()
        {
            EnsureCanvasGroup();
        }

        public void Show()
        {
            EnsureCanvasGroup();

            gameObject.SetActive(true);

            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

            SetProgress(0f);
        }

        public void Hide()
        {
            EnsureCanvasGroup();

            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            if (disableGameObjectOnHide)
            {
                gameObject.SetActive(false);
            }
        }

        public void SetProgress(float progress)
        {
            progress = Mathf.Clamp01(progress);

            if (progressSlider != null)
            {
                progressSlider.value = progress;
            }

            if (progressText != null)
            {
                int percent = Mathf.RoundToInt(progress * 100f);
                progressText.text = $"{percent}%";
            }
        }

        private void EnsureCanvasGroup()
        {
            if (canvasGroup != null)
            {
                return;
            }

            canvasGroup = GetComponent<CanvasGroup>();

            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }
    }
}