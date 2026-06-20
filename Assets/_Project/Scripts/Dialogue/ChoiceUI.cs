using System;
using System.Collections.Generic;
using NeuroQuest.Story;
using RTLTMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NeuroQuest.Dialogue
{
    public class ChoiceUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject root;
        [SerializeField] private RTLTextMeshPro questionText;

        [Header("Choice Buttons")]
        [SerializeField] private List<Button> optionButtons = new();
        [SerializeField] private List<RTLTextMeshPro> optionTexts = new();

        private Action<ChoiceOption> onOptionSelected;
        private readonly List<ChoiceOption> currentOptions = new();

        private void Awake()
        {
            RegisterButtons();
            Hide();
        }

        public void Show(
            string question,
            IReadOnlyList<ChoiceOption> options,
            Action<ChoiceOption> onSelected)
        {
            onOptionSelected = onSelected;

            currentOptions.Clear();

            if (options != null)
            {
                currentOptions.AddRange(options);
            }

            if (root != null)
            {
                root.SetActive(true);
            }
            else
            {
                gameObject.SetActive(true);
            }

            if (questionText != null)
            {
                questionText.text = question;
            }

            RefreshButtons();
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

        private void RegisterButtons()
        {
            for (int i = 0; i < optionButtons.Count; i++)
            {
                int capturedIndex = i;

                if (optionButtons[i] != null)
                {
                    optionButtons[i].onClick.RemoveAllListeners();
                    optionButtons[i].onClick.AddListener(() => HandleOptionClicked(capturedIndex));
                }
            }
        }

        private void RefreshButtons()
        {
            for (int i = 0; i < optionButtons.Count; i++)
            {
                bool hasOption = i < currentOptions.Count;

                if (optionButtons[i] != null)
                {
                    optionButtons[i].gameObject.SetActive(hasOption);
                }

                if (hasOption && i < optionTexts.Count && optionTexts[i] != null)
                {
                    optionTexts[i].text = currentOptions[i].DisplayText;
                }
            }
        }

        private void HandleOptionClicked(int index)
        {
            if (index < 0 || index >= currentOptions.Count)
            {
                Debug.LogError($"ChoiceUI: Invalid option index: {index}");
                return;
            }

            ChoiceOption selectedOption = currentOptions[index];

            Hide();

            Action<ChoiceOption> callback = onOptionSelected;
            onOptionSelected = null;

            callback?.Invoke(selectedOption);
        }
    }
}
