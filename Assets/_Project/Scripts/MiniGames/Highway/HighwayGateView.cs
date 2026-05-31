using System.Collections;
using UnityEngine;

namespace NeuroQuest.MiniGames.Highway
{
    public class HighwayGateView : MonoBehaviour
    {
        [Header("Gate Parts")]
        [SerializeField] private Transform clockwisePart;
        [SerializeField] private Transform counterClockwisePart;

        [Header("Animation")]
        [SerializeField] private float openAngle = 70f;
        [SerializeField] private float openDuration = 0.25f;
        [SerializeField] private float closeDelay = 0.25f;
        [SerializeField] private bool closeAfterOpen = true;

        private Quaternion clockwiseClosedRotation;
        private Quaternion counterClockwiseClosedRotation;
        private Coroutine animationCoroutine;

        private void Awake()
        {
            if (clockwisePart != null)
            {
                clockwiseClosedRotation = clockwisePart.localRotation;
            }

            if (counterClockwisePart != null)
            {
                counterClockwiseClosedRotation = counterClockwisePart.localRotation;
            }
        }

        public void PlayOpenAnimation()
        {
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
            }

            animationCoroutine = StartCoroutine(OpenRoutine());
        }

        private IEnumerator OpenRoutine()
        {
            if (clockwisePart == null || counterClockwisePart == null)
            {
                yield break;
            }

            Quaternion clockwiseStart = clockwisePart.localRotation;
            Quaternion counterStart = counterClockwisePart.localRotation;

            Quaternion clockwiseTarget = clockwiseClosedRotation * Quaternion.Euler(0f, 0f, -openAngle);
            Quaternion counterTarget = counterClockwiseClosedRotation * Quaternion.Euler(0f, 0f, -openAngle);

            float timer = 0f;

            while (timer < openDuration)
            {
                timer += Time.deltaTime;
                float t = Mathf.Clamp01(timer / openDuration);

                clockwisePart.localRotation = Quaternion.Lerp(clockwiseStart, clockwiseTarget, t);
                counterClockwisePart.localRotation = Quaternion.Lerp(counterStart, counterTarget, t);

                yield return null;
            }

            clockwisePart.localRotation = clockwiseTarget;
            counterClockwisePart.localRotation = counterTarget;

            if (!closeAfterOpen)
            {
                yield break;
            }

            yield return new WaitForSeconds(closeDelay);

            timer = 0f;

            Quaternion clockwiseCloseStart = clockwisePart.localRotation;
            Quaternion counterCloseStart = counterClockwisePart.localRotation;

            while (timer < openDuration)
            {
                timer += Time.deltaTime;
                float t = Mathf.Clamp01(timer / openDuration);

                clockwisePart.localRotation = Quaternion.Lerp(clockwiseCloseStart, clockwiseClosedRotation, t);
                counterClockwisePart.localRotation = Quaternion.Lerp(counterCloseStart, counterClockwiseClosedRotation, t);

                yield return null;
            }

            clockwisePart.localRotation = clockwiseClosedRotation;
            counterClockwisePart.localRotation = counterClockwiseClosedRotation;
        }
    }
}