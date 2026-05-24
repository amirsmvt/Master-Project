namespace NeuroQuest.Data
{
    public interface IDataExporter
    {
        void Export(SessionData sessionData, string folderPath);
    }
}