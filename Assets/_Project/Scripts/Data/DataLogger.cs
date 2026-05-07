using NeuroQuest.Services;
using UnityEngine;

namespace NeuroQuest.Data
{
    public class DataLogger : MonoBehaviour
    {
        private SessionData sessionData;
        private float sessionStartTime;

        private void Awake()
        {
            ServiceLocator.Register(this);

            sessionStartTime = Time.time;

            sessionData = new SessionData
            {
                sessionId = System.Guid.NewGuid().ToString(),
                sessionStartTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            Debug.Log("DataLogger: Session started.");
        }

        public void SetParticipantInfo(string participantId, string groupLabel)
        {
            sessionData.participantId = participantId;
            sessionData.groupLabel = groupLabel;

            Debug.Log($"DataLogger: Participant set | ID: {participantId}, Group: {groupLabel}");
        }

        public void LogSimpleEvent(
            string eventType,
            string miniGameId = "",
            string difficultyId = "",
            params DataField[] fields)
        {
            GameEventData gameEvent = new GameEventData
            {
                eventType = eventType,
                miniGameId = miniGameId,
                difficultyId = difficultyId,
                timeFromSessionStart = Time.time - sessionStartTime
            };

            foreach (DataField field in fields)
            {
                gameEvent.fields.Add(field);
            }

            sessionData.events.Add(gameEvent);

            Debug.Log($"DataLogger: Event logged | {eventType} | Total Events: {sessionData.events.Count}");
        }

        public SessionData GetSessionData()
        {
            return sessionData;
        }

        public void PrintSessionSummary()
        {
            Debug.Log("========== SESSION SUMMARY ==========");
            Debug.Log($"Session ID: {sessionData.sessionId}");
            Debug.Log($"Participant ID: {sessionData.participantId}");
            Debug.Log($"Group Label: {sessionData.groupLabel}");
            Debug.Log($"Start Time: {sessionData.sessionStartTime}");
            Debug.Log($"Total Events: {sessionData.events.Count}");
            Debug.Log("=====================================");
        }
    }
}