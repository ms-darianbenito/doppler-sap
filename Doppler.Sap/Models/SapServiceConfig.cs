namespace Doppler.Sap.Models
{
    public class SapServiceConfig
    {
        public string BaseServerUrl { get; set; }
        public string CompanyDB { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public BusinessPartnerConfig BusinessPartnerConfig { get; set; }
        public BillingConfig BillingConfig { get; set; }
    }
}
