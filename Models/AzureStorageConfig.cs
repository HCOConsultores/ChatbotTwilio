namespace chatBotTwilio.Models
{
    public class AzureStorageConfig
    {
        public string ConnectionString { get; set; }
        public string ContainerName { get; set; }

        // Computed property to extract AccountKey from ConnectionString
        public string AccountKey
        {
            get
            {
                if (string.IsNullOrEmpty(ConnectionString)) return null;
                var parts = ConnectionString.Split(';');
                foreach (var part in parts)
                {
                    if (part.StartsWith("AccountKey=", StringComparison.OrdinalIgnoreCase))
                    {
                        return part.Substring("AccountKey=".Length);
                    }
                }
                return null;
            }
        }
    }
}