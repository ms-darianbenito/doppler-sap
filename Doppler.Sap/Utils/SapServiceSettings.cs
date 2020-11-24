using Doppler.Sap.Models;
using System;

namespace Doppler.Sap.Utils
{
    public class SapServiceSettings
    {
        public static SapServiceConfig GetSettings(SapConfig sapConfig, string sapSystem)
        {
            if (!sapConfig.SapServiceConfigsBySystem.TryGetValue(sapSystem, out var serviceSettings))
            {
                throw new ArgumentException(nameof(sapSystem), $"The sapSystem '{sapSystem}' is not supported.");
            }

            return serviceSettings;
        }
    }
}
