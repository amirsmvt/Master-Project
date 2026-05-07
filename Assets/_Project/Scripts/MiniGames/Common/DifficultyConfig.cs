using System.Collections.Generic;
using NeuroQuest.Data;
using UnityEngine;

namespace NeuroQuest.MiniGames.Common
{
    [CreateAssetMenu(menuName = "NeuroQuest/Difficulty Config")]
    public class DifficultyConfig : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string difficultyId;

        [Header("Display")]
        [SerializeField] private int levelNumber;
        [SerializeField] private string displayName;
        [SerializeField] private string categoryLabel;

        [Header("Flexible Parameters")]
        [SerializeField] private List<DataField> parameters = new();

        public string DifficultyId => difficultyId;
        public int LevelNumber => levelNumber;
        public string DisplayName => displayName;
        public string CategoryLabel => categoryLabel;
        public IReadOnlyList<DataField> Parameters => parameters;

        public string GetString(string key, string defaultValue = "")
        {
            DataField field = parameters.Find(item => item.key == key);
            return field != null ? field.value : defaultValue;
        }

        public int GetInt(string key, int defaultValue = 0)
        {
            DataField field = parameters.Find(item => item.key == key);

            if (field != null && int.TryParse(field.value, out int value))
            {
                return value;
            }

            return defaultValue;
        }

        public float GetFloat(string key, float defaultValue = 0f)
        {
            DataField field = parameters.Find(item => item.key == key);

            if (field != null && float.TryParse(field.value, out float value))
            {
                return value;
            }

            return defaultValue;
        }

        public bool GetBool(string key, bool defaultValue = false)
        {
            DataField field = parameters.Find(item => item.key == key);

            if (field != null && bool.TryParse(field.value, out bool value))
            {
                return value;
            }

            return defaultValue;
        }
    }
}