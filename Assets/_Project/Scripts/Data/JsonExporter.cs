using System.IO;
using UnityEngine;

namespace NeuroQuest.Data
{
    public class JsonExporter : MonoBehaviour, IDataExporter
    {
        public void Export(SessionData sessionData, string folderPath)
        {
            if (sessionData == null)
            {
                Debug.LogError("JsonExporter: SessionData is null.");
                return;
            }

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string safeSessionId = ExportPathUtility.SanitizeFileName(sessionData.sessionId);
            string fileName = $"session_{safeSessionId}.json";
            string filePath = Path.Combine(folderPath, fileName);

            string json = JsonUtility.ToJson(sessionData, true);

            File.WriteAllText(filePath, json);

            Debug.Log($"JsonExporter: JSON exported to: {filePath}");
        }
    }
}