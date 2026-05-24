using NeuroQuest.Data;
using NeuroQuest.Services;
using UnityEngine;

namespace NeuroQuest.Core
{
    public class SessionManager : MonoBehaviour
    {
        [Header("Runtime References")]
        [SerializeField] private GameManager gameManager;
        [SerializeField] private DataLogger dataLogger;

        [Header("Temporary Participant Info")]
        [SerializeField] private ParticipantInfo testParticipantInfo = new ParticipantInfo
        {
            participantId = "P001",
            groupLabel = "test",
            age = 0,
            gender = "",
            notes = ""
        };

        public ParticipantInfo CurrentParticipantInfo { get; private set; }

        private void Awake()
        {
            ServiceLocator.Register(this);
        }

        private void ResolveServices()
        {
            if (gameManager == null)
            {
                gameManager = ServiceLocator.Get<GameManager>();
            }

            if (dataLogger == null)
            {
                dataLogger = ServiceLocator.Get<DataLogger>();
            }
        }

        public void StartTestSession()
        {
            StartSession(testParticipantInfo);
        }

        public void StartSession(ParticipantInfo participantInfo)
        {
            ResolveServices();

            if (participantInfo == null)
            {
                Debug.LogError("SessionManager: ParticipantInfo is null.");
                return;
            }

            if (gameManager == null)
            {
                Debug.LogError("SessionManager: GameManager is not assigned and not registered.");
                return;
            }

            CurrentParticipantInfo = participantInfo;

            gameManager.StartGameSession(
                participantInfo.participantId,
                participantInfo.groupLabel
            );

            if (dataLogger != null)
            {
                dataLogger.LogSimpleEvent(
                    "participant_info_set",
                    "",
                    "",
                    Field.Of("participantId", participantInfo.participantId),
                    Field.Of("groupLabel", participantInfo.groupLabel),
                    Field.Of("age", participantInfo.age),
                    Field.Of("gender", participantInfo.gender),
                    Field.Of("notes", participantInfo.notes)
                );
            }

            Debug.Log($"SessionManager: Session started for participant {participantInfo.participantId}");
        }
    }
}