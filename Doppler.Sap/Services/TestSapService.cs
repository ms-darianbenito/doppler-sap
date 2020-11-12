using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Doppler.Sap.Enums;
using Doppler.Sap.Models;
using Doppler.Sap.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Doppler.Sap.Services
{
    public class TestSapService : ITestSapService
    {
        private readonly ILogger<TestSapService> _logger;
        private readonly SapConfig _sapConfig;
        private const string countryCodeForAR = "AR";
        private const string countryCodeForUS = "US";

        public TestSapService(ILogger<TestSapService> logger, IOptions<SapConfig> sapConfig)
        {
            _logger = logger;
            _sapConfig = sapConfig.Value;
        }

        public async Task<string> TestSapConnection()
        {
            try
            {
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
                    UseCookies = false
                };
                var sapServiceSettings = SapServiceSettings.GetSettings(_sapConfig, countryCodeForAR);
                var client = new HttpClient(handler);
                var sapResponse = await client.SendAsync(new HttpRequestMessage
                {
                    RequestUri = new Uri($"{sapServiceSettings.BaseServerUrl}Login"),
                    Content = new StringContent(JsonConvert.SerializeObject(
                            new SapServiceConfig
                            {
                                CompanyDB = sapServiceSettings.CompanyDB,
                                Password = sapServiceSettings.Password,
                                UserName = sapServiceSettings.UserName
                            }),
                        Encoding.UTF8,
                        "application/json"),
                    Method = HttpMethod.Post
                });

                if (sapResponse.IsSuccessStatusCode)
                {
                    _logger.LogInformation("[TEST] Connect with Sap correctly");
                }
                else
                {
                    _logger.LogInformation("[TEST] Connect with Sap failed");
                }

                return $"Success {sapResponse.StatusCode}";
            }
            catch (Exception e)
            {
                _logger.LogError(e, "[TEST] Error starting session in Sap.");
                return $"Exception: {e.Message}";
            }
        }

        public async Task<string> TestSapUsConnection()
        {
            try
            {
                var handler = new HttpClientHandler
                {
                    //ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; },
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
                    UseCookies = false
                };
                var sapServiceSettings = SapServiceSettings.GetSettings(_sapConfig, countryCodeForUS);
                var client = new HttpClient(handler);
                var sapResponse = await client.SendAsync(new HttpRequestMessage
                {
                    RequestUri = new Uri($"{sapServiceSettings.BaseServerUrl}Login"),
                    Content = new StringContent(JsonConvert.SerializeObject(
                            new SapServiceConfig
                            {
                                CompanyDB = sapServiceSettings.CompanyDB,
                                Password = sapServiceSettings.Password,
                                UserName = sapServiceSettings.UserName
                            }),
                        Encoding.UTF8,
                        "application/json"),
                    Method = HttpMethod.Post
                });

                if (sapResponse.IsSuccessStatusCode)
                {
                    _logger.LogInformation("[TEST] Connect with Sap correctly");
                }
                else
                {
                    _logger.LogInformation("[TEST] Connect with Sap failed");
                }

                return $"Success {sapResponse.StatusCode}";
            }
            catch (Exception e)
            {
                _logger.LogError(e, "[TEST] Error starting session in Sap.");
                return $"Exception: {e.Message}";
            }
        }
    }
}
