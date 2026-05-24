using NeuroQuest.Services;
using UnityEngine;

namespace NeuroQuest.Data
{
    public class ExportManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private DataLogger dataLogger;
        [SerializeField] private JsonExporter jsonExporter;
        [SerializeField] private CsvExporter csvExporter;

        [Header("Export Settings")]
        [SerializeField] private bool exportJson = true;
        [SerializeField] private bool exportCsv = true;

        private bool hasExported;

        private void Awake()
        {
            ServiceLocator.Register(this);
        }

        private void ResolveServices()
        {
            if (dataLogger == null)
            {
                dataLogger = ServiceLocator.Get<DataLogger>();
            }
        }

        public void ExportCurrentSession()
        {
            ResolveServices();

            if (hasExported)
            {
                Debug.LogWarning("ExportManager: Session has already been exported.");
                return;
            }

            if (dataLogger == null)
            {
                Debug.LogError("ExportManager: DataLogger is not assigned and not registered.");
                return;
            }

            SessionData sessionData = dataLogger.GetSessionData();

            if (sessionData == null)
            {
                Debug.LogError("ExportManager: SessionData is null.");
                return;
            }

            string folderPath = ExportPathUtility.GetExportFolderPath();

            if (exportJson)
            {
                if (jsonExporter == null)
                {
                    Debug.LogError("ExportManager: JsonExporter is not assigned.");
                }
                else
                {
                    jsonExporter.Export(sessionData, folderPath);
                }
            }

            if (exportCsv)
            {
                if (csvExporter == null)
                {
                    Debug.LogError("ExportManager: CsvExporter is not assigned.");
                }
                else
                {
                    csvExporter.Export(sessionData, folderPath);
                }
            }

            hasExported = true;

            Debug.Log($"ExportManager: Export completed. Folder: {folderPath}");
        }

        public void ResetExportState()
        {
            hasExported = false;
        }
    }
}