using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

namespace NeuroQuest.Data
{
    public class CsvExporter : MonoBehaviour, IDataExporter
    {
        public void Export(SessionData sessionData, string folderPath)
        {
            if (sessionData == null)
            {
                Debug.LogError("CsvExporter: SessionData is null.");
                return;
            }

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string safeSessionId = ExportPathUtility.SanitizeFileName(sessionData.sessionId);
            string fileName = $"session_{safeSessionId}_events.csv";
            string filePath = Path.Combine(folderPath, fileName);

            StringBuilder builder = new StringBuilder();

            builder.AppendLine(
                "sessionId,participantId,groupLabel,sessionStartTime,eventIndex,eventType,miniGameId,difficultyId,timeFromSessionStart,fields"
            );

            for (int i = 0; i < sessionData.events.Count; i++)
            {
                GameEventData gameEvent = sessionData.events[i];

                builder.Append(Escape(sessionData.sessionId));
                builder.Append(",");
                builder.Append(Escape(sessionData.participantId));
                builder.Append(",");
                builder.Append(Escape(sessionData.groupLabel));
                builder.Append(",");
                builder.Append(Escape(sessionData.sessionStartTime));
                builder.Append(",");
                builder.Append(i);
                builder.Append(",");
                builder.Append(Escape(gameEvent.eventType));
                builder.Append(",");
                builder.Append(Escape(gameEvent.miniGameId));
                builder.Append(",");
                builder.Append(Escape(gameEvent.difficultyId));
                builder.Append(",");
                builder.Append(gameEvent.timeFromSessionStart.ToString("F3", CultureInfo.InvariantCulture));
                builder.Append(",");
                builder.Append(Escape(BuildFieldsString(gameEvent)));
                builder.AppendLine();
            }

            File.WriteAllText(filePath, builder.ToString(), Encoding.UTF8);

            Debug.Log($"CsvExporter: CSV exported to: {filePath}");
        }

        private string BuildFieldsString(GameEventData gameEvent)
        {
            if (gameEvent == null || gameEvent.fields == null || gameEvent.fields.Count == 0)
            {
                return "";
            }

            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < gameEvent.fields.Count; i++)
            {
                DataField field = gameEvent.fields[i];

                if (field == null)
                {
                    continue;
                }

                builder.Append(field.key);
                builder.Append("=");
                builder.Append(field.value);

                if (i < gameEvent.fields.Count - 1)
                {
                    builder.Append(";");
                }
            }

            return builder.ToString();
        }

        private string Escape(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "\"\"";
            }

            value = value.Replace("\"", "\"\"");
            return $"\"{value}\"";
        }
    }
}