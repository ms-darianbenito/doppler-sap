using Doppler.Sap.Models;
using System;

namespace Doppler.Sap.Utils
{
    public class SapServiceSettings
    {
        public static SapServiceConfig GetSettings(SapConfig sapConfig, string countryCode)
        {
            if (!sapConfig.SapServiceConfigsByCountryCode.TryGetValue(countryCode, out var serviceSettings))
            {
                throw new ArgumentException(nameof(countryCode), $"The countryCode '{countryCode}' is not supported.");
            }

            return serviceSettings;
        }
    }
}
