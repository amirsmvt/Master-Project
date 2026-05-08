using UnityEngine;

namespace NeuroQuest.UI
{
    public class UIScreen : MonoBehaviour
    {
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