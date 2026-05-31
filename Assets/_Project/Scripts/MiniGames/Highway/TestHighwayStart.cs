using UnityEngine;
using NeuroQuest.Core;

namespace NeuroQuest.Core
{
    public class TestHighwayStart : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private string participantId = "test_user";
        [SerializeField] private string groupLabel = "test_group";

        private void Start()
        {
            if (gameManager == null)
            {
                Debug.LogError("TestHighwayStart: GameManager is not assigned.");
                return;
            }

            gameManager.StartGameSession(participantId, groupLabel);
            gameManager.StartStory();
        }
    }
}