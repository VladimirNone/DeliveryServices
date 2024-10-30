namespace WepPartDeliveryProject
{
    public class ApplicationSettings
    {
        public int CountOfItemsOnWebPage { get; set; }
        public string JwtSecretKey { get; set; }
        public bool GenerateData { get; set; }
        public string BootstrapServers { get; set; }
    }
}
