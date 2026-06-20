using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using NeuroQuest.Data;
using NeuroQuest.MiniGames.Common;
using RTLTMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace NeuroQuest.MiniGames.WordMemory
{
    public class WordMemoryMiniGame : BaseMiniGame
    {
        [Header("UI References")]
        [SerializeField] private GameObject root;
        [SerializeField] private RTLTextMeshPro titleText;
        [SerializeField] private RTLTextMeshPro instructionText;
        [SerializeField] private GameObject learningWordsContainer;
        [FormerlySerializedAs("wordText")]
        [SerializeField] private RTLTextMeshPro recognitionWordText;
        [SerializeField] private RTLTextMeshPro feedbackText;
        [SerializeField] private RTLTextMeshPro progressText;
        [SerializeField] private Button yesButton;
        [SerializeField] private Button noButton;
        [SerializeField] private TMP_FontAsset defaultPersianFontAsset;

        [Header("Fallback Word Bank")]
        [SerializeField]
        private List<string> wordBank = new List<string>
        {
            "کتاب", "مداد", "درخت", "باران", "خانه",
            "مدرسه", "سیب", "ماه", "دریا", "پرنده",
            "گل", "خورشید", "کفش", "پنجره", "دوست",
            "ابر", "ماشین", "کلاس", "دفتر", "چراغ",
            "نان", "آب", "کوه", "رنگ", "صدا",
            "باغ", "قلم", "لبخند", "توپ", "ساعت"
        };

        [Header("Fallback Settings")]
        [SerializeField] private int fallbackLearningWordCount = 7;
        [SerializeField] private int fallbackTargetWordCount = 5;
        [SerializeField] private int fallbackDistractorWordCount = 2;
        [SerializeField] private int fallbackNewWordCount = 3;
        [SerializeField] private float fallbackMemorizationTime = 6f;
        [SerializeField] private float fallbackQuestionInterval = 0.6f;
        [SerializeField] private float fallbackResponseWindow = 5f;
        [SerializeField] private float fallbackFeedbackTime = 0.45f;

        private readonly List<string> targetWords = new();
        private readonly List<string> distractorWords = new();
        private readonly List<string> newWords = new();
        private readonly List<RecognitionTrial> recognitionTrials = new();

        private readonly List<string> expectedAnswerSequence = new();
        private readonly List<string> playerAnswerSequence = new();
        private readonly List<string> correctnessSequence = new();
        private readonly List<string> learningWordOrder = new();
        private readonly List<string> recognitionWordSequence = new();
        private readonly List<string> reactionTimeSequence = new();
        private readonly List<float> reactionTimes = new();

        private int targetWordCount;
        private int distractorWordCount;
        private int learningWordCount;
        private int newWordCount;
        private int recognitionTrialCount;
        private int levelNumber;

        private float memorizationTime;
        private float questionInterval;
        private float responseWindow;
        private float feedbackTime;

        private int currentRecognitionIndex;
        private bool waitingForAnswer;
        private bool hasAnswered;
        private bool playerAnswer;
        private float trialStartTime;

        private int correctCount;
        private int wrongCount;
        private int timeoutCount;
        private int correctRecognitions;
        private int correctRejections;
        private int falseRecognitions;
        private int missedTargets;

        private const string AnswerYes = "yes";
        private const string AnswerNo = "no";
        private const string AnswerTimeout = "timeout";
        private const string SourceTarget = "target";
        private const string SourceLearningDistractor = "learningDistractor";
        private const string SourceNewWord = "newWord";

        private class RecognitionTrial
        {
            public string word;
            public string wordSource;
            public bool wasTarget;
        }

        public override void StartGame()
        {
            ReadDifficulty();
            EnsureUI();
            RegisterButtons();

            ResetRuntimeData();

            DataLogger.LogSimpleEvent(
                "minigame_start",
                Config.MiniGameId,
                Difficulty.DifficultyId,
                Field.Of("miniGameName", Config.DisplayName),
                Field.Of("difficultyName", Difficulty.DisplayName),
                Field.Of("levelNumber", Difficulty.LevelNumber),
                Field.Of("learningWordCount", learningWordCount),
                Field.Of("targetWordCount", targetWordCount),
                Field.Of("distractorWordCount", distractorWordCount),
                Field.Of("newWordCount", newWordCount),
                Field.Of("memorizationTime", FormatFloat(memorizationTime)),
                Field.Of("questionInterval", FormatFloat(questionInterval)),
                Field.Of("responseWindow", FormatFloat(responseWindow))
            );

            StartCoroutine(GameRoutine());
        }

        private void ReadDifficulty()
        {
            levelNumber = Difficulty.LevelNumber;

            targetWordCount = Difficulty.GetInt("targetWordCount", fallbackTargetWordCount);
            distractorWordCount = Difficulty.GetInt("distractorWordCount", fallbackDistractorWordCount);
            learningWordCount = Difficulty.GetInt(
                "learningWordCount",
                Difficulty.GetInt("wordCount", fallbackLearningWordCount)
            );
            newWordCount = Difficulty.GetInt("newWordCount", fallbackNewWordCount);

            if (learningWordCount <= 0)
            {
                learningWordCount = targetWordCount + distractorWordCount;
            }

            if (targetWordCount + distractorWordCount != learningWordCount)
            {
                distractorWordCount = Mathf.Max(0, learningWordCount - targetWordCount);
            }

            memorizationTime = Difficulty.GetFloat(
                "memorizationTime",
                Difficulty.GetFloat("wordDisplayTime", fallbackMemorizationTime)
            );

            questionInterval = Difficulty.GetFloat(
                "questionInterval",
                Difficulty.GetFloat("interWordDelay", fallbackQuestionInterval)
            );

            responseWindow = Difficulty.GetFloat("responseWindow", fallbackResponseWindow);
            feedbackTime = Difficulty.GetFloat("feedbackTime", fallbackFeedbackTime);

            recognitionTrialCount = Difficulty.GetInt("recognitionTrialCount", 0);
        }

        private void ResetRuntimeData()
        {
            targetWords.Clear();
            distractorWords.Clear();
            newWords.Clear();
            recognitionTrials.Clear();

            expectedAnswerSequence.Clear();
            playerAnswerSequence.Clear();
            correctnessSequence.Clear();
            learningWordOrder.Clear();
            recognitionWordSequence.Clear();
            reactionTimeSequence.Clear();
            reactionTimes.Clear();

            currentRecognitionIndex = 0;
            waitingForAnswer = false;
            hasAnswered = false;
            playerAnswer = false;

            correctCount = 0;
            wrongCount = 0;
            timeoutCount = 0;
            correctRecognitions = 0;
            correctRejections = 0;
            falseRecognitions = 0;
            missedTargets = 0;

            ClearLearningWords();
        }

        private IEnumerator GameRoutine()
        {
            SetButtonsVisible(false);

            GenerateWords();
            GenerateRecognitionTrials();

            yield return RunLearningPhase();
            yield return RunRecognitionPhase();

            FinishWordMemoryGame();
        }

        private void GenerateWords()
        {
            List<string> cleanBank = GetCleanWordBank();

            targetWordCount = Mathf.Clamp(targetWordCount, 1, cleanBank.Count);

            targetWords.AddRange(TakeRandomWords(cleanBank, targetWordCount, null));

            HashSet<string> excluded = new HashSet<string>(targetWords);

            int maxDistractors = Mathf.Max(0, cleanBank.Count - excluded.Count);
            distractorWordCount = Mathf.Clamp(distractorWordCount, 0, maxDistractors);
            learningWordCount = targetWordCount + distractorWordCount;
            distractorWords.AddRange(TakeRandomWords(cleanBank, distractorWordCount, excluded));

            foreach (string word in distractorWords)
            {
                excluded.Add(word);
            }

            int maxNewWords = Mathf.Max(0, cleanBank.Count - excluded.Count);
            newWordCount = Mathf.Clamp(newWordCount, 0, maxNewWords);
            newWords.AddRange(TakeRandomWords(cleanBank, newWordCount, excluded));
        }

        private void GenerateRecognitionTrials()
        {
            List<RecognitionTrial> allTrials = new List<RecognitionTrial>();

            foreach (string word in targetWords)
            {
                allTrials.Add(new RecognitionTrial
                {
                    word = word,
                    wordSource = SourceTarget,
                    wasTarget = true
                });
            }

            foreach (string word in distractorWords)
            {
                allTrials.Add(new RecognitionTrial
                {
                    word = word,
                    wordSource = SourceLearningDistractor,
                    wasTarget = false
                });
            }

            foreach (string word in newWords)
            {
                allTrials.Add(new RecognitionTrial
                {
                    word = word,
                    wordSource = SourceNewWord,
                    wasTarget = false
                });
            }

            Shuffle(allTrials);

            if (recognitionTrialCount > 0 && recognitionTrialCount < allTrials.Count)
            {
                recognitionTrials.AddRange(allTrials.GetRange(0, recognitionTrialCount));
            }
            else
            {
                recognitionTrials.AddRange(allTrials);
            }

            foreach (RecognitionTrial trial in recognitionTrials)
            {
                expectedAnswerSequence.Add(trial.wasTarget ? AnswerYes : AnswerNo);
                recognitionWordSequence.Add(trial.word);
            }
        }

        private IEnumerator RunLearningPhase()
        {
            SetText(titleText, "حافظه لغات");
            SetText(instructionText, "کلمات زرد را حفظ کن و کلمات قرمز را نادیده بگیر.");
            SetText(feedbackText, "");
            SetText(progressText, "مرحله یادگیری");
            SetText(recognitionWordText, "");
            SetLearningContainerVisible(true);
            SetButtonsVisible(false);
            ClearLearningWords();

            DataLogger.LogSimpleEvent(
                "learning_phase_start",
                Config.MiniGameId,
                Difficulty.DifficultyId,
                Field.Of("levelNumber", levelNumber),
                Field.Of("targetWords", JoinList(targetWords)),
                Field.Of("distractorWords", JoinList(distractorWords)),
                Field.Of("learningWordCount", targetWords.Count + distractorWords.Count),
                Field.Of("targetWordCount", targetWords.Count),
                Field.Of("distractorWordCount", distractorWords.Count)
            );

            List<(string word, bool isTarget)> learningItems = new List<(string word, bool isTarget)>();

            foreach (string word in targetWords)
            {
                learningItems.Add((word, true));
            }

            foreach (string word in distractorWords)
            {
                learningItems.Add((word, false));
            }

            Shuffle(learningItems);

            for (int i = 0; i < learningItems.Count; i++)
            {
                var item = learningItems[i];
                learningWordOrder.Add(item.word);
                CreateLearningWordText(item.word, item.isTarget);

                DataLogger.LogSimpleEvent(
                    "learning_item_shown",
                    Config.MiniGameId,
                    Difficulty.DifficultyId,
                    Field.Of("levelNumber", levelNumber),
                    Field.Of("learningIndex", i),
                    Field.Of("word", item.word),
                    Field.Of("isTarget", item.isTarget),
                    Field.Of("wordSource", item.isTarget ? SourceTarget : SourceLearningDistractor),
                    Field.Of("displayTime", FormatFloat(memorizationTime))
                );
            }

            SetText(progressText, $"یادگیری {learningItems.Count} کلمه");

            DataLogger.LogSimpleEvent(
                "learning_grid_shown",
                Config.MiniGameId,
                Difficulty.DifficultyId,
                Field.Of("levelNumber", levelNumber),
                Field.Of("learningWordOrder", JoinList(learningWordOrder)),
                Field.Of("memorizationTime", FormatFloat(memorizationTime))
            );

            yield return new WaitForSeconds(memorizationTime);

            SetLearningContainerVisible(false);
            SetText(instructionText, "حالا بگو هر کلمه جزو کلمات زرد بود یا نه.");
            SetText(progressText, "مرحله بازشناسی");

            yield return new WaitForSeconds(questionInterval);
        }

        private IEnumerator RunRecognitionPhase()
        {
            SetButtonsVisible(true);

            for (currentRecognitionIndex = 0; currentRecognitionIndex < recognitionTrials.Count; currentRecognitionIndex++)
            {
                RecognitionTrial trial = recognitionTrials[currentRecognitionIndex];

                yield return RunRecognitionTrial(trial, currentRecognitionIndex);
            }

            SetButtonsVisible(false);
        }

        private IEnumerator RunRecognitionTrial(RecognitionTrial trial, int trialIndex)
        {
            hasAnswered = false;
            waitingForAnswer = true;
            playerAnswer = false;
            trialStartTime = Time.time;

            SetRecognitionWordColor(Color.white);
            SetText(recognitionWordText, trial.word);
            SetText(feedbackText, "");
            SetText(progressText, $"سؤال {trialIndex + 1} / {recognitionTrials.Count}");
            SetButtonsInteractable(true);

            while (waitingForAnswer && Time.time - trialStartTime < responseWindow)
            {
                yield return null;
            }

            waitingForAnswer = false;
            SetButtonsInteractable(false);

            float reactionTime = hasAnswered
                ? Time.time - trialStartTime
                : responseWindow;

            bool isCorrect = false;
            bool timeout = !hasAnswered;
            string answerString = AnswerTimeout;
            string errorType = "none";
            string expectedAnswer = trial.wasTarget ? AnswerYes : AnswerNo;

            if (hasAnswered)
            {
                answerString = playerAnswer ? AnswerYes : AnswerNo;

                if (trial.wasTarget && playerAnswer)
                {
                    isCorrect = true;
                    correctRecognitions++;
                }
                else if (!trial.wasTarget && !playerAnswer)
                {
                    isCorrect = true;
                    correctRejections++;
                }
                else if (!trial.wasTarget && playerAnswer)
                {
                    errorType = "falseRecognition";
                    falseRecognitions++;
                }
                else if (trial.wasTarget && !playerAnswer)
                {
                    errorType = "missedTarget";
                    missedTargets++;
                }
            }
            else
            {
                errorType = "timeout";
                timeoutCount++;
            }

            if (isCorrect)
            {
                correctCount++;
                SetText(feedbackText, "درست");
            }
            else
            {
                wrongCount++;
                SetText(feedbackText, "غلط");
            }

            playerAnswerSequence.Add(answerString);
            correctnessSequence.Add(isCorrect ? "1" : "0");
            reactionTimeSequence.Add(FormatFloat(reactionTime));

            if (hasAnswered)
            {
                reactionTimes.Add(reactionTime);
            }

            DataLogger.LogSimpleEvent(
                "trial",
                Config.MiniGameId,
                Difficulty.DifficultyId,
                Field.Of("phase", "recognition"),
                Field.Of("trialIndex", trialIndex),
                Field.Of("levelNumber", levelNumber),
                Field.Of("shownWord", trial.word),
                Field.Of("wordSource", trial.wordSource),
                Field.Of("wasTarget", trial.wasTarget),
                Field.Of("expectedAnswer", expectedAnswer),
                Field.Of("userSelected", answerString),
                Field.Of("userResponded", hasAnswered),
                Field.Of("reactionTime", FormatFloat(reactionTime)),
                Field.Of("isCorrect", isCorrect),
                Field.Of("errorType", errorType),
                Field.Of("timeout", timeout)
            );

            yield return new WaitForSeconds(feedbackTime);
            SetText(feedbackText, "");
            SetText(recognitionWordText, "");
            yield return new WaitForSeconds(questionInterval);
        }

        private void FinishWordMemoryGame()
        {
            int totalTrials = recognitionTrials.Count;
            float accuracy = totalTrials > 0 ? (float)correctCount / totalTrials : 0f;
            float averageReactionTime = CalculateAverage(reactionTimes);
            float reactionTimeVariability = CalculateStandardDeviation(reactionTimes);
            int finalScore = Mathf.RoundToInt(accuracy * 100f);

            DataLogger.LogSimpleEvent(
                "minigame_summary",
                Config.MiniGameId,
                Difficulty.DifficultyId,
                Field.Of("levelNumber", levelNumber),
                Field.Of("totalTrials", totalTrials),
                Field.Of("learningWordCount", targetWords.Count + distractorWords.Count),
                Field.Of("targetWordCount", targetWords.Count),
                Field.Of("distractorWordCount", distractorWords.Count),
                Field.Of("newWordCount", newWords.Count),
                Field.Of("correctCount", correctCount),
                Field.Of("wrongCount", wrongCount),
                Field.Of("timeoutCount", timeoutCount),
                Field.Of("correctRecognitions", correctRecognitions),
                Field.Of("correctRejections", correctRejections),
                Field.Of("falseRecognitions", falseRecognitions),
                Field.Of("missedTargets", missedTargets),
                Field.Of("accuracy", FormatFloat(accuracy)),
                Field.Of("averageReactionTime", FormatFloat(averageReactionTime)),
                Field.Of("reactionTimeVariability", FormatFloat(reactionTimeVariability)),
                Field.Of("targetWords", JoinList(targetWords)),
                Field.Of("distractorWords", JoinList(distractorWords)),
                Field.Of("learningWordOrder", JoinList(learningWordOrder)),
                Field.Of("recognitionWordSequence", JoinList(recognitionWordSequence)),
                Field.Of("expectedAnswerSequence", JoinList(expectedAnswerSequence)),
                Field.Of("playerAnswerSequence", JoinList(playerAnswerSequence)),
                Field.Of("correctnessSequence", JoinList(correctnessSequence)),
                Field.Of("reactionTimeSequence", JoinList(reactionTimeSequence)),
                Field.Of("memorizationTime", FormatFloat(memorizationTime)),
                Field.Of("questionInterval", FormatFloat(questionInterval)),
                Field.Of("finalScore", finalScore)
            );

            SetText(titleText, "پایان بازی");
            SetLearningContainerVisible(false);
            SetText(recognitionWordText, "");
            SetText(instructionText, $"امتیاز: {finalScore}");
            SetText(feedbackText, $"درست: {correctCount} | غلط: {wrongCount}");
            SetText(progressText, "");

            MiniGameResult result = new MiniGameResult(
                Config.MiniGameId,
                isCompleted: true,
                score: finalScore
            );

            result.AddExtraData("levelNumber", levelNumber);
            result.AddExtraData("accuracy", FormatFloat(accuracy));
            result.AddExtraData("targetWords", JoinList(targetWords));
            result.AddExtraData("distractorWords", JoinList(distractorWords));
            result.AddExtraData("learningWordOrder", JoinList(learningWordOrder));
            result.AddExtraData("recognitionWordSequence", JoinList(recognitionWordSequence));
            result.AddExtraData("expectedAnswerSequence", JoinList(expectedAnswerSequence));
            result.AddExtraData("playerAnswerSequence", JoinList(playerAnswerSequence));
            result.AddExtraData("correctnessSequence", JoinList(correctnessSequence));
            result.AddExtraData("reactionTimeSequence", JoinList(reactionTimeSequence));
            result.AddExtraData("correctCount", correctCount);
            result.AddExtraData("wrongCount", wrongCount);
            result.AddExtraData("timeoutCount", timeoutCount);
            result.AddExtraData("falseRecognitions", falseRecognitions);
            result.AddExtraData("missedTargets", missedTargets);
            result.AddExtraData("averageReactionTime", FormatFloat(averageReactionTime));
            result.AddExtraData("reactionTimeVariability", FormatFloat(reactionTimeVariability));
            result.AddExtraData("memorizationTime", FormatFloat(memorizationTime));
            result.AddExtraData("questionInterval", FormatFloat(questionInterval));
            result.AddExtraData("finalScore", finalScore);

            StartCoroutine(CompleteAfterDelay(result, 1.2f));
        }

        private IEnumerator CompleteAfterDelay(MiniGameResult result, float delay)
        {
            yield return new WaitForSeconds(delay);
            CompleteGame(result);
        }

        private void HandleYesClicked()
        {
            if (!waitingForAnswer || hasAnswered)
            {
                return;
            }

            playerAnswer = true;
            hasAnswered = true;
            waitingForAnswer = false;
        }

        private void HandleNoClicked()
        {
            if (!waitingForAnswer || hasAnswered)
            {
                return;
            }

            playerAnswer = false;
            hasAnswered = true;
            waitingForAnswer = false;
        }

        private void RegisterButtons()
        {
            if (yesButton != null)
            {
                SetButtonLabel(yesButton, "بله");
                yesButton.onClick.RemoveListener(HandleYesClicked);
                yesButton.onClick.AddListener(HandleYesClicked);
            }

            if (noButton != null)
            {
                SetButtonLabel(noButton, "خیر");
                noButton.onClick.RemoveListener(HandleNoClicked);
                noButton.onClick.AddListener(HandleNoClicked);
            }
        }

        private void OnDestroy()
        {
            if (yesButton != null)
            {
                yesButton.onClick.RemoveListener(HandleYesClicked);
            }

            if (noButton != null)
            {
                noButton.onClick.RemoveListener(HandleNoClicked);
            }
        }

        private void SetButtonsVisible(bool visible)
        {
            if (yesButton != null)
            {
                yesButton.gameObject.SetActive(visible);
            }

            if (noButton != null)
            {
                noButton.gameObject.SetActive(visible);
            }
        }

        private void SetButtonsInteractable(bool interactable)
        {
            if (yesButton != null)
            {
                yesButton.interactable = interactable;
            }

            if (noButton != null)
            {
                noButton.interactable = interactable;
            }
        }

        private void SetLearningContainerVisible(bool visible)
        {
            if (learningWordsContainer != null)
            {
                learningWordsContainer.SetActive(visible);
            }
        }

        private void EnsureUI()
        {
            if (root != null &&
                titleText != null &&
                instructionText != null &&
                learningWordsContainer != null &&
                recognitionWordText != null &&
                feedbackText != null &&
                progressText != null &&
                yesButton != null &&
                noButton != null)
            {
                root.SetActive(true);
                return;
            }

            BuildFallbackUI();
        }

        private void BuildFallbackUI()
        {
            GameObject canvasObject = new GameObject("WordMemoryCanvas");
            canvasObject.transform.SetParent(transform, false);

            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObject.AddComponent<GraphicRaycaster>();

            root = new GameObject("Root");
            root.transform.SetParent(canvasObject.transform, false);

            RectTransform rootRect = root.AddComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            Image background = root.AddComponent<Image>();
            background.color = new Color(0.08f, 0.08f, 0.11f, 0.96f);

            titleText = CreateText("TitleText", root.transform, "حافظه لغات", 56, new Vector2(0.5f, 0.88f), new Vector2(1200, 100));
            instructionText = CreateText("InstructionText", root.transform, "", 34, new Vector2(0.5f, 0.74f), new Vector2(1500, 120));
            learningWordsContainer = CreateLearningWordsContainer(root.transform);
            recognitionWordText = CreateText("RecognitionWordText", root.transform, "", 82, new Vector2(0.5f, 0.52f), new Vector2(1000, 160));
            feedbackText = CreateText("FeedbackText", root.transform, "", 40, new Vector2(0.5f, 0.36f), new Vector2(1000, 100));
            progressText = CreateText("ProgressText", root.transform, "", 30, new Vector2(0.5f, 0.24f), new Vector2(900, 80));

            yesButton = CreateButton("YesButton", root.transform, "بله", new Vector2(0.38f, 0.12f));
            noButton = CreateButton("NoButton", root.transform, "خیر", new Vector2(0.62f, 0.12f));
        }

        private RTLTextMeshPro CreateText(
            string objectName,
            Transform parent,
            string text,
            int fontSize,
            Vector2 anchor,
            Vector2 size)
        {
            GameObject textObject = new GameObject(objectName);
            textObject.transform.SetParent(parent, false);

            RectTransform rectTransform = textObject.AddComponent<RectTransform>();
            rectTransform.anchorMin = anchor;
            rectTransform.anchorMax = anchor;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = size;
            rectTransform.anchoredPosition = Vector2.zero;

            RTLTextMeshPro tmp = textObject.AddComponent<RTLTextMeshPro>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.enableWordWrapping = true;
            ConfigureRtlText(tmp, text);

            return tmp;
        }

        private GameObject CreateLearningWordsContainer(Transform parent)
        {
            GameObject container = new GameObject("LearningWordsContainer");
            container.transform.SetParent(parent, false);

            RectTransform rectTransform = container.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = new Vector2(1200, 360);
            rectTransform.anchoredPosition = Vector2.zero;

            GridLayoutGroup grid = container.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(260, 90);
            grid.spacing = new Vector2(28, 24);
            grid.childAlignment = TextAnchor.MiddleCenter;
            grid.constraint = GridLayoutGroup.Constraint.Flexible;

            return container;
        }

        private Button CreateButton(
            string objectName,
            Transform parent,
            string label,
            Vector2 anchor)
        {
            GameObject buttonObject = new GameObject(objectName);
            buttonObject.transform.SetParent(parent, false);

            RectTransform rectTransform = buttonObject.AddComponent<RectTransform>();
            rectTransform.anchorMin = anchor;
            rectTransform.anchorMax = anchor;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = new Vector2(260, 90);
            rectTransform.anchoredPosition = Vector2.zero;

            Image image = buttonObject.AddComponent<Image>();
            image.color = new Color(0.22f, 0.36f, 0.75f, 1f);

            Button button = buttonObject.AddComponent<Button>();

            RTLTextMeshPro buttonText = CreateText(
                $"{objectName}_Text",
                buttonObject.transform,
                label,
                36,
                new Vector2(0.5f, 0.5f),
                new Vector2(240, 80)
            );

            buttonText.color = Color.white;

            return button;
        }

        private void SetButtonLabel(Button button, string label)
        {
            if (button == null)
            {
                return;
            }

            RTLTextMeshPro labelText = button.GetComponentInChildren<RTLTextMeshPro>(true);
            if (labelText != null)
            {
                SetText(labelText, label);
            }
        }

        private void CreateLearningWordText(string word, bool isTarget)
        {
            if (learningWordsContainer == null)
            {
                return;
            }

            RTLTextMeshPro learningText = CreateText(
                $"LearningWord_{learningWordsContainer.transform.childCount + 1}",
                learningWordsContainer.transform,
                word,
                52,
                new Vector2(0.5f, 0.5f),
                new Vector2(260, 90)
            );

            RectTransform rectTransform = learningText.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            learningText.color = isTarget ? new Color(1f, 0.85f, 0.05f) : new Color(1f, 0.25f, 0.25f);
            ConfigureRtlText(learningText, word);
        }

        private void ClearLearningWords()
        {
            if (learningWordsContainer == null)
            {
                return;
            }

            for (int i = learningWordsContainer.transform.childCount - 1; i >= 0; i--)
            {
                Transform child = learningWordsContainer.transform.GetChild(i);
                child.SetParent(null, false);
                Destroy(child.gameObject);
            }
        }

        private void SetText(RTLTextMeshPro textComponent, string value)
        {
            if (textComponent != null)
            {
                ConfigureRtlText(textComponent, value);
            }
        }

        private void ConfigureRtlText(RTLTextMeshPro textComponent, string value)
        {
            if (textComponent == null)
            {
                return;
            }

            if (defaultPersianFontAsset != null)
            {
                textComponent.font = defaultPersianFontAsset;
            }

            textComponent.Farsi = true;
            textComponent.FixTags = true;
            textComponent.ForceFix = !string.IsNullOrEmpty(value);
            textComponent.text = value;
            textComponent.UpdateText();
        }

        private void SetRecognitionWordColor(Color color)
        {
            if (recognitionWordText != null)
            {
                recognitionWordText.color = color;
            }
        }

        private List<string> GetCleanWordBank()
        {
            List<string> cleanWords = new List<string>();

            foreach (string word in wordBank)
            {
                if (string.IsNullOrWhiteSpace(word))
                {
                    continue;
                }

                if (!cleanWords.Contains(word))
                {
                    cleanWords.Add(word.Trim());
                }
            }

            if (cleanWords.Count == 0)
            {
                cleanWords.AddRange(new[]
                {
                    "کتاب", "مداد", "درخت", "خانه", "سیب",
                    "دریا", "پرنده", "ماه", "گل", "کوه"
                });
            }

            return cleanWords;
        }

        private List<string> TakeRandomWords(
            List<string> source,
            int count,
            HashSet<string> excluded)
        {
            List<string> candidates = new List<string>();

            foreach (string word in source)
            {
                if (excluded != null && excluded.Contains(word))
                {
                    continue;
                }

                candidates.Add(word);
            }

            Shuffle(candidates);

            int takeCount = Mathf.Clamp(count, 0, candidates.Count);
            return candidates.GetRange(0, takeCount);
        }

        private void Shuffle<T>(List<T> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                int randomIndex = Random.Range(i, list.Count);
                (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
            }
        }

        private float CalculateAverage(List<float> values)
        {
            if (values == null || values.Count == 0)
            {
                return 0f;
            }

            float sum = 0f;

            foreach (float value in values)
            {
                sum += value;
            }

            return sum / values.Count;
        }

        private float CalculateStandardDeviation(List<float> values)
        {
            if (values == null || values.Count <= 1)
            {
                return 0f;
            }

            float average = CalculateAverage(values);
            float sum = 0f;

            foreach (float value in values)
            {
                float difference = value - average;
                sum += difference * difference;
            }

            return Mathf.Sqrt(sum / values.Count);
        }

        private string JoinList(List<string> values)
        {
            if (values == null || values.Count == 0)
            {
                return "";
            }

            return string.Join("|", values);
        }

        private string FormatFloat(float value)
        {
            return value.ToString("F3", CultureInfo.InvariantCulture);
        }
    }
}
