using System.Collections.Generic;

namespace Doppler.Sap.Models
{
    public class SapConfig
    {
        public string SlackAlertUrl { get; set; }
        public int MaxAmountAllowedAccounts { get; set; }
        public int SessionTimeoutPadding { get; set; }
        public bool UseDummyData { get; set; }
        public Dictionary<string, SapServiceConfig> SapServiceConfigsBySystem { get; set; }
    }
}
