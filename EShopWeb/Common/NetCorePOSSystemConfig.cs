namespace EShopWeb.Common
{
    public class EShopSystemConfig
    {
        private Uri baseAddress;
        public string BaseUrl { get; set; }
        public Uri BaseAddress
        {
            get
            {
                return baseAddress;
            }
            set
            {
                if (!string.IsNullOrEmpty(BaseUrl))
                {
                    baseAddress = new Uri(BaseUrl);
                }
            }
        }
    }
}
