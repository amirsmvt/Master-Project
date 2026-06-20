using System;
using RTLTMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NeuroQuest.Dialogue
{
    public class DialogueUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject root;
        [SerializeField] private RTLTextMeshPro speakerNameText;
        [SerializeField] private RTLTextMeshPro dialogueText;
        [SerializeField] private Image portraitImage;
        [SerializeField] private Button continueButton;
        [SerializeField] private RTLTextMeshPro continueButtonText;
        [SerializeField] private string defaultContinueButtonText = "ادامه";

        private Action onContinue;

        private void Awake()
        {
            if (continueButton != null)
            {
                continueButton.onClick.AddListener(HandleContinueClicked);
            }

            Hide();
        }

        public void Show(string text, Action onContinueClicked)
        {
            Show(string.Empty, text, null, defaultContinueButtonText, onContinueClicked);
        }

        public void Show(
            string speakerName,
            string text,
            Sprite portrait,
            string continueText,
            Action onContinueClicked)
        {
            onContinue = onContinueClicked;

            if (root != null)
            {
                root.SetActive(true);
            }
            else
            {
                gameObject.SetActive(true);
            }

            if (dialogueText != null)
            {
                dialogueText.text = text;
            }

            if (speakerNameText != null)
            {
                speakerNameText.text = speakerName;
                speakerNameText.gameObject.SetActive(!string.IsNullOrWhiteSpace(speakerName));
            }

            if (portraitImage != null)
            {
                portraitImage.sprite = portrait;
                portraitImage.gameObject.SetActive(portrait != null);
            }

            if (continueButtonText != null)
            {
                continueButtonText.text = string.IsNullOrWhiteSpace(continueText)
                    ? defaultContinueButtonText
                    : continueText;
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

        private void HandleContinueClicked()
        {
            Hide();

            Action callback = onContinue;
            onContinue = null;

            callback?.Invoke();
        }

        private void OnDestroy()
        {
            if (continueButton != null)
            {
                continueButton.onClick.RemoveListener(HandleContinueClicked);
            }
        }
    }
}
