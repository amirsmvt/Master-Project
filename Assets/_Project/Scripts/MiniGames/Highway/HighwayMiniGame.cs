using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NeuroQuest.Data;
using NeuroQuest.MiniGames.Common;
using UnityEngine;

namespace NeuroQuest.MiniGames.Highway
{
    public class HighwayMiniGame : BaseMiniGame
    {
        private enum HighwayRule
        {
            ObjectTagToLaneCode = 1,
            ObjectTagToLaneCodeColoredObject = 2,
            ObjectColorToLaneCode = 3,
            ObjectColorToLaneTag = 4,
            ObjectTagToLaneColor = 5,
            ObjectColorToLaneColor = 6,
            NotMatchingObjectColorOrTag = 7
        }

        private class MovingItemState
        {
            public int trialIndex;
            public bool isBomb;

            public HighwayVisualItem visual;
            public Transform transform;

            public int startLaneCode;
            public int currentLaneCode;
            public int selectedLaneCode;
            public int correctLaneCode;

            public int objectColorCode;
            public int objectTagCode;

            public bool decisionMade;
            public bool finalized;
            public bool clickedBomb;

            public float spawnTime;
            public float responseStartTime;
            public float selectedTime;
            public float disappearTime;

            public float speed;
            public float targetY;
            public float distanceToGateAtSelection;

            public string responseTimeValue = "NONE";
            public string reactionTime2Value = "NONE";
            public string userSelectionValue = "0";
            public string unresolvedCountValue = "NONE";
            public string correctLaneValue = "NONE";
            public string distanceToGateValue = "NONE";
        }

        [Header("Prefabs")]
        [SerializeField] private HighwayLaneView lanePrefab;
        [SerializeField] private HighwayVisualItem objectPrefab;
        [SerializeField] private HighwayVisualItem bombPrefab;
        [SerializeField] private HighwayDistractorView distractorPrefab;

        [Header("Parents")]
        [SerializeField] private Transform laneParent;
        [SerializeField] private Transform movingItemParent;
        [SerializeField] private Transform distractorParent;

        [Header("Lane Layout")]
        [SerializeField] private float laneX = 0f;
        [SerializeField] private float topLaneY = 3f;
        [SerializeField] private float laneSpacing = 1.2f;
        [SerializeField] private float spawnX = 8f;
        [SerializeField] private float gateX = -7f;

        [Header("Movement")]
        [SerializeField] private float laneChangeSpeed = 7f;
        [SerializeField] private float selectedSpeedMultiplierFallback = 1.25f;

        [Header("Distractor Area")]
        [SerializeField] private Vector2 distractorAreaMin = new Vector2(-5f, -3f);
        [SerializeField] private Vector2 distractorAreaMax = new Vector2(5f, 3f);
        [SerializeField] private float distractorLifetime = 3f;
        [SerializeField] private float distractorMoveSpeed = 1.5f;

        [Header("Bomb")]
        [SerializeField] private float bombLifetime = 2f;

        private readonly List<HighwayLaneView> lanes = new();
        private readonly List<MovingItemState> activeItems = new();
        private readonly List<MovingItemState> normalTrials = new();

        private int activeLaneCount;
        private int objectCount;
        private int ruleNumber;
        private int bombCount;
        private int spawnedBombCount;
        private int bombClickedCount;
        private int distractorSpawnedCount;

        private float startSpeed;
        private float endSpeed;
        private float spawnInterval;
        private float selectedSpeedMultiplier;

        private bool distractorsEnabled;
        private bool bombsEnabled;
        private bool spawningFinished;
        private bool gameCompleted;

        private float? lastUserSelectionTime;
        private bool previousNormalTrialHadSelection;

        private Coroutine spawnRoutine;
        private Coroutine distractorRoutine;

        public override void StartGame()
        {
            ReadDifficultyParameters();

            BuildLanes();

            DataLogger.LogSimpleEvent(
                "minigame_start",
                Config.MiniGameId,
                Difficulty.DifficultyId,
                Field.Of("levelNumber", Difficulty.LevelNumber),
                Field.Of("activeLaneCount", activeLaneCount),
                Field.Of("objectCount", objectCount),
                Field.Of("ruleNumber", ruleNumber),
                Field.Of("startSpeed", startSpeed),
                Field.Of("endSpeed", endSpeed),
                Field.Of("spawnInterval", spawnInterval),
                Field.Of("distractorsEnabled", distractorsEnabled),
                Field.Of("bombsEnabled", bombsEnabled),
                Field.Of("bombCount", bombCount)
            );

            spawnRoutine = StartCoroutine(SpawnSequence());

            if (distractorsEnabled)
            {
                distractorRoutine = StartCoroutine(SpawnDistractors());
            }
        }

        private void ReadDifficultyParameters()
        {
            activeLaneCount = Mathf.Clamp(Difficulty.GetInt("activeLaneCount", 4), 1, 4);
            objectCount = Mathf.Max(1, Difficulty.GetInt("objectCount", 20));
            ruleNumber = Mathf.Clamp(Difficulty.GetInt("ruleNumber", 1), 1, 7);

            startSpeed = Mathf.Max(0.1f, Difficulty.GetFloat("startSpeed", 2f));
            endSpeed = Mathf.Max(startSpeed, Difficulty.GetFloat("endSpeed", 5f));

            spawnInterval = Mathf.Max(0.15f, Difficulty.GetFloat("spawnInterval", 0.9f));
            selectedSpeedMultiplier = Difficulty.GetFloat("selectedSpeedMultiplier", selectedSpeedMultiplierFallback);

            distractorsEnabled = Difficulty.GetBool("distractorsEnabled", false);
            bombsEnabled = Difficulty.GetBool("bombsEnabled", false);
            bombCount = bombsEnabled ? Mathf.Max(0, Difficulty.GetInt("bombCount", 0)) : 0;
        }

        private void BuildLanes()
        {
            ClearExistingChildren(laneParent);

            lanes.Clear();

            for (int i = 0; i < 4; i++)
            {
                int laneCode = i + 1;
                HighwayLaneView lane = Instantiate(lanePrefab, laneParent);

                float y = topLaneY - i * laneSpacing;
                lane.transform.position = new Vector3(laneX, y, 0f);

                if (laneCode <= activeLaneCount)
                {
                    int laneColorCode = GetLaneColorCode(laneCode);
                    int laneTagCode = GetLaneTagCode(laneCode);
                    bool showLaneTag = ShouldShowLaneTag();

                    lane.Configure(
                        laneCode,
                        laneColorCode,
                        laneTagCode,
                        showLaneTag,
                        OnLaneClicked
                    );
                }
                else
                {
                    lane.Deactivate();
                }

                lanes.Add(lane);
            }
        }

        private void ClearExistingChildren(Transform parent)
        {
            if (parent == null)
            {
                return;
            }

            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Destroy(parent.GetChild(i).gameObject);
            }
        }

        private IEnumerator SpawnSequence()
        {
            HashSet<int> bombSlots = GenerateBombSlots();

            int normalSpawned = 0;
            int totalSlots = objectCount + bombSlots.Count;

            for (int slot = 0; slot < totalSlots; slot++)
            {
                if (bombSlots.Contains(slot))
                {
                    SpawnBomb();
                }
                else
                {
                    normalSpawned++;
                    SpawnNormalObject(normalSpawned);
                }

                yield return new WaitForSeconds(spawnInterval);
            }

            spawningFinished = true;
        }

        private HashSet<int> GenerateBombSlots()
        {
            HashSet<int> result = new();

            if (!bombsEnabled || bombCount <= 0)
            {
                return result;
            }

            int totalSlots = objectCount + bombCount;

            while (result.Count < bombCount && result.Count < totalSlots)
            {
                int slot = UnityEngine.Random.Range(0, totalSlots);
                result.Add(slot);
            }

            return result;
        }

        private void SpawnNormalObject(int trialIndex)
        {
            int startLaneCode = UnityEngine.Random.Range(1, activeLaneCount + 1);
            HighwayLaneView lane = GetLane(startLaneCode);

            int objectColorCode;
            int objectTagCode;

            GenerateObjectCodes(out objectColorCode, out objectTagCode);

            int correctLaneCode = ResolveCorrectLaneCode(objectColorCode, objectTagCode);

            HighwayVisualItem visual = Instantiate(objectPrefab, movingItemParent);
            visual.transform.position = new Vector3(spawnX, lane.Y, 0f);
            visual.Configure(objectColorCode, objectTagCode, ShouldShowObjectTag());

            float t = objectCount <= 1 ? 0f : (trialIndex - 1f) / (objectCount - 1f);
            float speed = Mathf.Lerp(startSpeed, endSpeed, t);

            MovingItemState state = new MovingItemState
            {
                trialIndex = trialIndex,
                isBomb = false,
                visual = visual,
                transform = visual.transform,
                startLaneCode = startLaneCode,
                currentLaneCode = startLaneCode,
                selectedLaneCode = 0,
                correctLaneCode = correctLaneCode,
                objectColorCode = objectColorCode,
                objectTagCode = objectTagCode,
                decisionMade = false,
                finalized = false,
                spawnTime = Time.time,
                responseStartTime = GetResponseStartTimeForNewObject(),
                speed = speed,
                targetY = lane.Y,
                correctLaneValue = correctLaneCode.ToString(CultureInfo.InvariantCulture)
            };

            activeItems.Add(state);
            normalTrials.Add(state);

            DataLogger.LogSimpleEvent(
                "highway_object_spawned",
                Config.MiniGameId,
                Difficulty.DifficultyId,
                Field.Of("trialIndex", trialIndex),
                Field.Of("startLaneCode", startLaneCode),
                Field.Of("objectColorCode", objectColorCode),
                Field.Of("objectTagCode", objectTagCode),
                Field.Of("correctLaneCode", correctLaneCode),
                Field.Of("speed", speed),
                Field.Of("ruleNumber", ruleNumber)
            );
        }

        private float GetResponseStartTimeForNewObject()
        {
            MovingItemState latestBomb = activeItems
                .Where(item => item.isBomb && item.disappearTime > 0f)
                .OrderByDescending(item => item.disappearTime)
                .FirstOrDefault();

            if (latestBomb != null && latestBomb.disappearTime > Time.time - 0.5f)
            {
                return latestBomb.disappearTime;
            }

            return Time.time;
        }

        private void SpawnBomb()
        {
            if (bombPrefab == null)
            {
                return;
            }

            spawnedBombCount++;

            int startLaneCode = UnityEngine.Random.Range(1, activeLaneCount + 1);
            HighwayLaneView lane = GetLane(startLaneCode);

            HighwayVisualItem visual = Instantiate(bombPrefab, movingItemParent);
            visual.transform.position = new Vector3(spawnX, lane.Y, 0f);
            visual.Configure(0, 0, false);

            MovingItemState state = new MovingItemState
            {
                trialIndex = -1,
                isBomb = true,
                visual = visual,
                transform = visual.transform,
                startLaneCode = startLaneCode,
                currentLaneCode = startLaneCode,
                decisionMade = false,
                finalized = false,
                spawnTime = Time.time,
                responseStartTime = Time.time,
                speed = startSpeed,
                targetY = lane.Y
            };

            activeItems.Add(state);

            DataLogger.LogSimpleEvent(
                "highway_bomb_spawned",
                Config.MiniGameId,
                Difficulty.DifficultyId,
                Field.Of("bombIndex", spawnedBombCount),
                Field.Of("laneCode", startLaneCode)
            );

            StartCoroutine(RemoveBombAfterDelay(state));
        }

        private IEnumerator RemoveBombAfterDelay(MovingItemState bomb)
        {
            yield return new WaitForSeconds(bombLifetime);

            if (bomb == null || bomb.finalized)
            {
                yield break;
            }

            bomb.disappearTime = Time.time;
            bomb.finalized = true;

            activeItems.Remove(bomb);

            if (bomb.visual != null)
            {
                Destroy(bomb.visual.gameObject);
            }

            DataLogger.LogSimpleEvent(
                "highway_bomb_disappeared",
                Config.MiniGameId,
                Difficulty.DifficultyId,
                Field.Of("bombClicked", bomb.clickedBomb)
            );
        }

        private IEnumerator SpawnDistractors()
        {
            int distractorTargetCount = Mathf.Max(0, Difficulty.LevelNumber * 2);

            for (int i = 0; i < distractorTargetCount; i++)
            {
                float randomDelay = UnityEngine.Random.Range(0.4f, 2.0f);
                yield return new WaitForSeconds(randomDelay);

                SpawnDistractor();
            }
        }

        private void SpawnDistractor()
        {
            if (distractorPrefab == null)
            {
                return;
            }

            distractorSpawnedCount++;

            Rect bounds = new Rect(
                distractorAreaMin.x,
                distractorAreaMin.y,
                distractorAreaMax.x - distractorAreaMin.x,
                distractorAreaMax.y - distractorAreaMin.y
            );

            Vector3 position = new Vector3(
                UnityEngine.Random.Range(bounds.xMin, bounds.xMax),
                UnityEngine.Random.Range(bounds.yMin, bounds.yMax),
                0f
            );

            HighwayDistractorView distractor = Instantiate(distractorPrefab, distractorParent);
            distractor.transform.position = position;
            distractor.Setup(bounds, distractorMoveSpeed);

            Destroy(distractor.gameObject, distractorLifetime);

            DataLogger.LogSimpleEvent(
                "highway_distractor_spawned",
                Config.MiniGameId,
                Difficulty.DifficultyId,
                Field.Of("distractorIndex", distractorSpawnedCount)
            );
        }

        private void Update()
        {
            MoveActiveItems();
            TryCompleteGame();
        }

        private void MoveActiveItems()
        {
            for (int i = activeItems.Count - 1; i >= 0; i--)
            {
                MovingItemState item = activeItems[i];

                if (item == null || item.finalized || item.transform == null)
                {
                    continue;
                }

                Vector3 position = item.transform.position;

                position.x = Mathf.MoveTowards(
                    position.x,
                    gateX,
                    item.speed * Time.deltaTime
                );

                position.y = Mathf.MoveTowards(
                    position.y,
                    item.targetY,
                    laneChangeSpeed * Time.deltaTime
                );

                item.transform.position = position;

                if (position.x <= gateX)
                {
                    if (item.isBomb)
                    {
                        continue;
                    }

                    FinalizeNormalObjectAtGate(item);
                }
            }
        }

        private void OnLaneClicked(int selectedLaneCode)
        {
            MovingItemState decisionItem = GetCurrentDecisionItem();

            if (decisionItem == null)
            {
                return;
            }

            if (decisionItem.isBomb)
            {
                bombClickedCount++;
                decisionItem.clickedBomb = true;

                DataLogger.LogSimpleEvent(
                    "highway_bomb_clicked",
                    Config.MiniGameId,
                    Difficulty.DifficultyId,
                    Field.Of("bombClickedCount", bombClickedCount),
                    Field.Of("selectedLaneCode", selectedLaneCode)
                );

                return;
            }

            if (decisionItem.decisionMade)
            {
                return;
            }

            RegisterLaneSelection(decisionItem, selectedLaneCode);
        }

        private MovingItemState GetCurrentDecisionItem()
        {
            return activeItems
                .Where(item => item != null && !item.finalized && !item.decisionMade)
                .OrderBy(item => item.spawnTime)
                .FirstOrDefault();
        }

        private void RegisterLaneSelection(MovingItemState item, int selectedLaneCode)
        {
            float now = Time.time;

            item.decisionMade = true;
            item.selectedTime = now;
            item.selectedLaneCode = selectedLaneCode;
            item.userSelectionValue = selectedLaneCode.ToString(CultureInfo.InvariantCulture);

            float responseTime = now - item.responseStartTime;
            item.responseTimeValue = FormatFloat(responseTime);

            float reactionTime2;

            if (!lastUserSelectionTime.HasValue || !previousNormalTrialHadSelection)
            {
                reactionTime2 = responseTime;
            }
            else
            {
                reactionTime2 = now - lastUserSelectionTime.Value;
            }

            item.reactionTime2Value = FormatFloat(reactionTime2);

            int unresolvedOthersCount = activeItems.Count(other =>
                other != null &&
                !other.isBomb &&
                !other.finalized &&
                !other.decisionMade &&
                other.spawnTime > item.spawnTime
            );

            item.unresolvedCountValue = unresolvedOthersCount.ToString(CultureInfo.InvariantCulture);

            float distanceToGate = Mathf.Abs(item.transform.position.x - gateX);
            item.distanceToGateAtSelection = distanceToGate;
            item.distanceToGateValue = FormatFloat(distanceToGate);

            HighwayLaneView targetLane = GetLane(selectedLaneCode);

            if (targetLane != null)
            {
                item.currentLaneCode = selectedLaneCode;
                item.targetY = targetLane.Y;
            }

            item.speed *= selectedSpeedMultiplier;

            lastUserSelectionTime = now;
            previousNormalTrialHadSelection = true;

            bool isCorrect = selectedLaneCode == item.correctLaneCode;

            DataLogger.LogSimpleEvent(
                "highway_lane_selected",
                Config.MiniGameId,
                Difficulty.DifficultyId,
                Field.Of("trialIndex", item.trialIndex),
                Field.Of("selectedLaneCode", selectedLaneCode),
                Field.Of("correctLaneCode", item.correctLaneCode),
                Field.Of("isCorrect", isCorrect),
                Field.Of("responseTime", item.responseTimeValue),
                Field.Of("reactionTime2", item.reactionTime2Value),
                Field.Of("unresolvedOtherObjects", item.unresolvedCountValue),
                Field.Of("distanceToGate", item.distanceToGateValue),
                Field.Of("objectColorCode", item.objectColorCode),
                Field.Of("objectTagCode", item.objectTagCode),
                Field.Of("ruleNumber", ruleNumber)
            );
        }

        private void FinalizeNormalObjectAtGate(MovingItemState item)
        {
            if (item.finalized)
            {
                return;
            }

            item.finalized = true;

            if (!item.decisionMade)
            {
                item.userSelectionValue = "0";
                item.responseTimeValue = "NONE";
                item.reactionTime2Value = "NONE";
                item.unresolvedCountValue = "NONE";
                item.distanceToGateValue = "0";
                previousNormalTrialHadSelection = false;
            }

            bool isCorrect = item.decisionMade && item.selectedLaneCode == item.correctLaneCode;
            string errorType = GetErrorType(item, isCorrect);
            HighwayLaneView finalLane = GetLane(item.currentLaneCode);

            if (finalLane != null)
            {
                finalLane.PlayGateOpenAnimation();
            }

            DataLogger.LogSimpleEvent(
                "trial",
                Config.MiniGameId,
                Difficulty.DifficultyId,
                Field.Of("trialIndex", item.trialIndex),
                Field.Of("objectColorCode", item.objectColorCode),
                Field.Of("objectTagCode", item.objectTagCode),
                Field.Of("startLaneCode", item.startLaneCode),
                Field.Of("selectedLaneCode", item.selectedLaneCode),
                Field.Of("correctLaneCode", item.correctLaneCode),
                Field.Of("userSelection", item.userSelectionValue),
                Field.Of("isCorrect", isCorrect),
                Field.Of("errorType", errorType),
                Field.Of("responseTime", item.responseTimeValue),
                Field.Of("reactionTime2", item.reactionTime2Value),
                Field.Of("unresolvedOtherObjects", item.unresolvedCountValue),
                Field.Of("distanceToGateAtSelection", item.distanceToGateValue),
                Field.Of("ruleNumber", ruleNumber)
            );

            activeItems.Remove(item);

            if (item.visual != null)
            {
                Destroy(item.visual.gameObject);
            }
        }

        private string GetErrorType(MovingItemState item, bool isCorrect)
        {
            if (isCorrect)
            {
                return "none";
            }

            if (!item.decisionMade)
            {
                return "timeout";
            }

            return "wrongSelection";
        }

        private void TryCompleteGame()
        {
            if (gameCompleted)
            {
                return;
            }

            if (!spawningFinished)
            {
                return;
            }

            bool anyNormalActive = activeItems.Any(item => item != null && !item.isBomb && !item.finalized);

            if (anyNormalActive)
            {
                return;
            }

            gameCompleted = true;

            if (spawnRoutine != null)
            {
                StopCoroutine(spawnRoutine);
            }

            if (distractorRoutine != null)
            {
                StopCoroutine(distractorRoutine);
            }

            CompleteHighwayGame();
        }

        private void CompleteHighwayGame()
        {
            int totalTrials = normalTrials.Count;
            int correctCount = normalTrials.Count(item =>
                item.decisionMade && item.selectedLaneCode == item.correctLaneCode
            );

            int noSelectionCount = normalTrials.Count(item => !item.decisionMade);
            int wrongSelectionCount = normalTrials.Count(item =>
                item.decisionMade && item.selectedLaneCode != item.correctLaneCode
            );

            float accuracy = totalTrials > 0 ? (float)correctCount / totalTrials : 0f;

            float precision = correctCount + wrongSelectionCount > 0
                ? (float)correctCount / (correctCount + wrongSelectionCount)
                : 0f;

            float recall = totalTrials > 0
                ? (float)correctCount / totalTrials
                : 0f;

            float sensitivity = recall;

            float f1 = precision + recall > 0f
                ? 2f * precision * recall / (precision + recall)
                : 0f;

            float averageResponseTime = AverageOfSequence(normalTrials.Select(item => item.responseTimeValue));
            float averageReactionTime2 = AverageOfSequence(normalTrials.Select(item => item.reactionTime2Value));

            int finalScore = Mathf.RoundToInt(accuracy * 1000f);

            string responseTimes = JoinSequence(normalTrials.Select(item => item.responseTimeValue));
            string reactionTimes2 = JoinSequence(normalTrials.Select(item => item.reactionTime2Value));
            string userSelections = JoinSequence(normalTrials.Select(item => item.userSelectionValue));
            string unresolvedCounts = JoinSequence(normalTrials.Select(item => item.unresolvedCountValue));
            string correctLaneCodes = JoinSequence(normalTrials.Select(item => item.correctLaneValue));
            string distancesToGate = JoinSequence(normalTrials.Select(item => item.distanceToGateValue));

            DataLogger.LogSimpleEvent(
                "minigame_summary",
                Config.MiniGameId,
                Difficulty.DifficultyId,
                Field.Of("levelNumber", Difficulty.LevelNumber),
                Field.Of("totalTrials", totalTrials),
                Field.Of("correctCount", correctCount),
                Field.Of("wrongSelectionCount", wrongSelectionCount),
                Field.Of("noSelectionCount", noSelectionCount),
                Field.Of("accuracy", FormatFloat(accuracy)),
                Field.Of("precision", FormatFloat(precision)),
                Field.Of("recall", FormatFloat(recall)),
                Field.Of("sensitivity", FormatFloat(sensitivity)),
                Field.Of("f1Score", FormatFloat(f1)),
                Field.Of("averageResponseTime", FormatFloat(averageResponseTime)),
                Field.Of("averageReactionTime2", FormatFloat(averageReactionTime2)),
                Field.Of("responseTimes", responseTimes),
                Field.Of("reactionTimes2", reactionTimes2),
                Field.Of("userSelections", userSelections),
                Field.Of("unresolvedObjectCounts", unresolvedCounts),
                Field.Of("correctLaneCodes", correctLaneCodes),
                Field.Of("distanceToGateAtSelection", distancesToGate),
                Field.Of("bombCount", bombCount),
                Field.Of("bombSpawnedCount", spawnedBombCount),
                Field.Of("bombClickedCount", bombClickedCount),
                Field.Of("distractorSpawnedCount", distractorSpawnedCount),
                Field.Of("finalScore", finalScore),
                Field.Of("completed", true)
            );

            DataLogger.LogSimpleEvent(
                "minigame_end",
                Config.MiniGameId,
                Difficulty.DifficultyId,
                Field.Of("score", finalScore),
                Field.Of("completed", true)
            );

            MiniGameResult result = new MiniGameResult(
                Config.MiniGameId,
                isCompleted: true,
                score: finalScore
            );

            result.AddExtraData("accuracy", FormatFloat(accuracy));
            result.AddExtraData("precision", FormatFloat(precision));
            result.AddExtraData("recall", FormatFloat(recall));
            result.AddExtraData("sensitivity", FormatFloat(sensitivity));
            result.AddExtraData("f1Score", FormatFloat(f1));
            result.AddExtraData("averageResponseTime", FormatFloat(averageResponseTime));
            result.AddExtraData("averageReactionTime2", FormatFloat(averageReactionTime2));
            result.AddExtraData("bombClickedCount", bombClickedCount);
            result.AddExtraData("distractorSpawnedCount", distractorSpawnedCount);
            result.AddExtraData("levelNumber", Difficulty.LevelNumber);

            CompleteGame(result);
        }

        private void GenerateObjectCodes(out int objectColorCode, out int objectTagCode)
        {
            HighwayRule rule = GetCurrentRule();

            switch (rule)
            {
                case HighwayRule.ObjectTagToLaneCode:
                    objectColorCode = 0;
                    objectTagCode = RandomActiveColorCode();
                    break;

                case HighwayRule.ObjectTagToLaneCodeColoredObject:
                case HighwayRule.ObjectColorToLaneCode:
                case HighwayRule.ObjectColorToLaneTag:
                case HighwayRule.ObjectTagToLaneColor:
                case HighwayRule.ObjectColorToLaneColor:
                case HighwayRule.NotMatchingObjectColorOrTag:
                    objectColorCode = RandomActiveColorCode();
                    objectTagCode = RandomActiveColorCode();
                    break;

                default:
                    objectColorCode = 0;
                    objectTagCode = RandomActiveColorCode();
                    break;
            }

            if (rule == HighwayRule.NotMatchingObjectColorOrTag)
            {
                GenerateValidCodesForRule7(out objectColorCode, out objectTagCode);
            }
        }

        private void GenerateValidCodesForRule7(out int objectColorCode, out int objectTagCode)
        {
            List<int> validObjectColors = new();

            for (int color = 1; color <= activeLaneCount; color++)
            {
                int candidateColorCode = color;

                for (int tag = 1; tag <= activeLaneCount; tag++)
                {
                    int candidateTagCode = tag;

                    int matchCount = lanes.Count(lane =>
                        lane.gameObject.activeSelf &&
                        lane.LaneColorCode != candidateColorCode &&
                        lane.LaneColorCode != candidateTagCode &&
                        lane.LaneTagCode != candidateColorCode &&
                        lane.LaneTagCode != candidateTagCode
                    );

                    if (matchCount == 1)
                    {
                        validObjectColors.Add(candidateColorCode);
                        break;
                    }
                }
            }

            if (validObjectColors.Count == 0)
            {
                objectColorCode = RandomActiveColorCode();
                objectTagCode = RandomActiveColorCode();
                return;
            }

            objectColorCode = validObjectColors[UnityEngine.Random.Range(0, validObjectColors.Count)];

            int selectedObjectColorCode = objectColorCode;

            List<int> validTags = new();

            for (int tag = 1; tag <= activeLaneCount; tag++)
            {
                int candidateTagCode = tag;

                int matchCount = lanes.Count(lane =>
                    lane.gameObject.activeSelf &&
                    lane.LaneColorCode != selectedObjectColorCode &&
                    lane.LaneColorCode != candidateTagCode &&
                    lane.LaneTagCode != selectedObjectColorCode &&
                    lane.LaneTagCode != candidateTagCode
                );

                if (matchCount == 1)
                {
                    validTags.Add(candidateTagCode);
                }
            }

            objectTagCode = validTags.Count > 0
                ? validTags[UnityEngine.Random.Range(0, validTags.Count)]
                : RandomActiveColorCode();
        }
        private int ResolveCorrectLaneCode(int objectColorCode, int objectTagCode)
        {
            HighwayRule rule = GetCurrentRule();

            switch (rule)
            {
                case HighwayRule.ObjectTagToLaneCode:
                    return objectTagCode;

                case HighwayRule.ObjectTagToLaneCodeColoredObject:
                    return objectTagCode;

                case HighwayRule.ObjectColorToLaneCode:
                    return objectColorCode;

                case HighwayRule.ObjectColorToLaneTag:
                    return FindSingleLaneByCondition(lane => lane.LaneTagCode == objectColorCode);

                case HighwayRule.ObjectTagToLaneColor:
                    return FindSingleLaneByCondition(lane => lane.LaneColorCode == objectTagCode);

                case HighwayRule.ObjectColorToLaneColor:
                    return FindSingleLaneByCondition(lane => lane.LaneColorCode == objectColorCode);

                case HighwayRule.NotMatchingObjectColorOrTag:
                    return FindSingleLaneByCondition(lane =>
                        lane.LaneColorCode != objectColorCode &&
                        lane.LaneColorCode != objectTagCode &&
                        lane.LaneTagCode != objectColorCode &&
                        lane.LaneTagCode != objectTagCode
                    );

                default:
                    return objectTagCode;
            }
        }

        private int FindSingleLaneByCondition(Func<HighwayLaneView, bool> condition)
        {
            List<HighwayLaneView> matched = lanes
                .Where(lane => lane != null && lane.gameObject.activeSelf && condition(lane))
                .ToList();

            if (matched.Count == 1)
            {
                return matched[0].LaneCode;
            }

            if (matched.Count > 1)
            {
                return matched[0].LaneCode;
            }

            return UnityEngine.Random.Range(1, activeLaneCount + 1);
        }

        private int GetLaneColorCode(int laneCode)
        {
            HighwayRule rule = GetCurrentRule();

            switch (rule)
            {
                case HighwayRule.ObjectColorToLaneTag:
                case HighwayRule.ObjectTagToLaneColor:
                case HighwayRule.ObjectColorToLaneColor:
                case HighwayRule.NotMatchingObjectColorOrTag:
                    return laneCode;

                default:
                    return 0;
            }
        }

        private int GetLaneTagCode(int laneCode)
        {
            HighwayRule rule = GetCurrentRule();

            switch (rule)
            {
                case HighwayRule.ObjectColorToLaneTag:
                case HighwayRule.ObjectTagToLaneColor:
                case HighwayRule.ObjectColorToLaneColor:
                case HighwayRule.NotMatchingObjectColorOrTag:
                    return laneCode;

                default:
                    return 0;
            }
        }

        private bool ShouldShowObjectTag()
        {
            HighwayRule rule = GetCurrentRule();

            switch (rule)
            {
                case HighwayRule.ObjectColorToLaneCode:
                    return true;

                default:
                    return true;
            }
        }

        private bool ShouldShowLaneTag()
        {
            HighwayRule rule = GetCurrentRule();

            switch (rule)
            {
                case HighwayRule.ObjectColorToLaneTag:
                case HighwayRule.ObjectTagToLaneColor:
                case HighwayRule.ObjectColorToLaneColor:
                case HighwayRule.NotMatchingObjectColorOrTag:
                    return true;

                default:
                    return false;
            }
        }

        private HighwayRule GetCurrentRule()
        {
            return (HighwayRule)ruleNumber;
        }

        private int RandomActiveColorCode()
        {
            return UnityEngine.Random.Range(1, activeLaneCount + 1);
        }

        private HighwayLaneView GetLane(int laneCode)
        {
            return lanes.FirstOrDefault(lane => lane != null && lane.LaneCode == laneCode);
        }

        private string JoinSequence(IEnumerable<string> values)
        {
            return string.Join("-", values.Select(value => string.IsNullOrWhiteSpace(value) ? "NONE" : value));
        }

        private string FormatFloat(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
            {
                return "NONE";
            }

            return value.ToString("0.###", CultureInfo.InvariantCulture);
        }

        private float AverageOfSequence(IEnumerable<string> values)
        {
            List<float> parsedValues = new();

            foreach (string value in values)
            {
                if (string.IsNullOrWhiteSpace(value) || value == "NONE")
                {
                    continue;
                }

                if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsed))
                {
                    parsedValues.Add(parsed);
                }
            }

            if (parsedValues.Count == 0)
            {
                return 0f;
            }

            return parsedValues.Average();
        }
    }
}