namespace Doppler.Sap.Models
{
    public class SapConfig
    {
        public string CompanyDB { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string BaseServerUrl { get; set; }
        public string SlackAlertUrl { get; set; }
        public int MaxAmountAllowedAccounts { get; set; }
        public int SessionTimeoutPadding { get; set; }
    }
}
