using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NeuroQuest.Dialogue
{
    public class DialogueUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject root;
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private Button continueButton;

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