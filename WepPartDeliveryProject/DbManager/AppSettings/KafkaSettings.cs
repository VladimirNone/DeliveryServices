namespace DbManager.AppSettings
{
    public class KafkaSettings
    {
        public string BootstrapServers { get; set; }

        public string GroupId { get; set; }

        public string ContainerEventsTopic { get; set; }

        public string ContainerOrdersTopic { get; set; }
    }
}
