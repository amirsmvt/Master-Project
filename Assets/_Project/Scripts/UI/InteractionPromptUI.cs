using RTLTMPro;
using TMPro;
using UnityEngine;

namespace NeuroQuest.UI
{
    public class InteractionPromptUI : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private RTLTextMeshPro promptText;
        [SerializeField] private string defaultPrompt = "برای صحبت کردن، E را فشار بده";
        [SerializeField] private TMP_FontAsset fontAsset;

        private void Awake()
        {
            EnsurePromptText();
            Hide();
        }

        public void Show(string prompt = null)
        {
            EnsurePromptText();

            if (promptText != null)
            {
                promptText.text = string.IsNullOrWhiteSpace(prompt) ? defaultPrompt : prompt;
            }

            if (root != null)
            {
                root.SetActive(true);
            }
            else
            {
                gameObject.SetActive(true);
            }
        }

        public void Hide()
        {
            if (root != null)
            {
                root.SetActive(false);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private void EnsurePromptText()
        {
            if (promptText != null)
            {
                return;
            }

            GameObject textObject = new GameObject("PromptText", typeof(RectTransform), typeof(CanvasRenderer), typeof(RTLTextMeshPro));
            textObject.transform.SetParent(transform, false);

            RectTransform rectTransform = textObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0f);
            rectTransform.anchorMax = new Vector2(0.5f, 0f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = new Vector2(0f, 64f);
            rectTransform.sizeDelta = new Vector2(560f, 72f);

            promptText = textObject.GetComponent<RTLTextMeshPro>();
            promptText.alignment = TextAlignmentOptions.Center;
            promptText.fontSize = 32f;
            promptText.color = Color.white;

            if (fontAsset == null)
            {
                fontAsset = FindExistingPersianFont();
            }

            if (fontAsset != null)
            {
                promptText.font = fontAsset;
            }
        }

        private TMP_FontAsset FindExistingPersianFont()
        {
            Canvas parentCanvas = GetComponentInParent<Canvas>();
            if (parentCanvas == null)
            {
                return null;
            }

            RTLTextMeshPro[] textComponents = parentCanvas.GetComponentsInChildren<RTLTextMeshPro>(true);
            foreach (RTLTextMeshPro textComponent in textComponents)
            {
                if (textComponent != null && textComponent.font != null && textComponent != promptText)
                {
                    return textComponent.font;
                }
            }

            return null;
        }
    }
}
