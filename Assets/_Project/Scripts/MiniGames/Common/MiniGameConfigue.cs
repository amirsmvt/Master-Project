using UnityEngine;

namespace NeuroQuest.MiniGames.Common
{
    [CreateAssetMenu(menuName = "NeuroQuest/MiniGame Config")]
    public class MiniGameConfig : ScriptableObject
    {
        [Header("Basic Info")]
        [SerializeField] private string miniGameId;
        [SerializeField] private string displayName;

        [Header("Prefab")]
        [SerializeField] private BaseMiniGame miniGamePrefab;

        public string MiniGameId => miniGameId;
        public string DisplayName => displayName;
        public BaseMiniGame MiniGamePrefab => miniGamePrefab;
    }
}