using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NeuroQuest.MiniGames.Highway
{
    public class HighwayLaneView : MonoBehaviour, IPointerClickHandler
    {
        [Header("Points")]
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private Transform gatePoint;

        [Header("Visual")]
        [SerializeField] private HighwayVisualItem gateVisual;

        [Header("Gate Animation")]
        [SerializeField] private HighwayGateView gateView;

        private Action<int> onClicked;

        public int LaneCode { get; private set; }
        public int LaneColorCode { get; private set; }
        public int LaneTagCode { get; private set; }

        public Transform SpawnPoint => spawnPoint;
        public Transform GatePoint => gatePoint;
        public float Y => transform.position.y;

        public void Configure(
            int laneCode,
            int laneColorCode,
            int laneTagCode,
            bool showLaneTag,
            Action<int> onLaneClicked)
        {
            LaneCode = laneCode;
            LaneColorCode = laneColorCode;
            LaneTagCode = laneTagCode;
            onClicked = onLaneClicked;

            gameObject.SetActive(true);

            if (gateVisual != null)
            {
                gateVisual.Configure(laneColorCode, laneTagCode, showLaneTag);
            }
        }

        public void Deactivate()
        {
            gameObject.SetActive(false);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            onClicked?.Invoke(LaneCode);
        }

        public void PlayGateOpenAnimation()
        {
            if (gateView != null)
            {
                gateView.PlayOpenAnimation();
            }
        }
    }
}