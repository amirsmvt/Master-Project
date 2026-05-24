using System.IO;
using UnityEngine;

namespace NeuroQuest.Data
{
    public static class ExportPathUtility
    {
        public static string GetExportFolderPath()
        {
            string folderPath = Path.Combine(
                Application.persistentDataPath,
                "NeuroQuestExports"
            );

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            return folderPath;
        }

        public static string SanitizeFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return "unknown";
            }

            foreach (char invalidChar in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(invalidChar, '_');
            }

            return fileName;
        }
    }
}