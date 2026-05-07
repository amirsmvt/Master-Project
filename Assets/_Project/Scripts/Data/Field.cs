namespace NeuroQuest.Data
{
    public static class Field
    {
        public static DataField Of(string key, object value)
        {
            return new DataField
            {
                key = key,
                value = value != null ? value.ToString() : string.Empty
            };
        }
    }
}