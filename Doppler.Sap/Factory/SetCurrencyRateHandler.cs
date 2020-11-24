using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Doppler.Sap.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using Doppler.Sap.Enums;
using Doppler.Sap.Utils;

namespace Doppler.Sap.Factory
{
    public class SetCurrencyRateHandler
    {
        private readonly ISapServiceSettingsFactory _sapServiceSettingsFactory;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly SapConfig _sapConfig;
        private const string _sapSystem = "AR";

        public SetCurrencyRateHandler(
            IOptions<SapConfig> sapConfig,
            ISapServiceSettingsFactory sapServiceSettingsFactory,
            IHttpClientFactory httpClientFactory)
        {
            _sapConfig = sapConfig.Value;
            _sapServiceSettingsFactory = sapServiceSettingsFactory;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<SapTaskResult> Handle(SapTask dequeuedTask)
        {
            var sapTaskHandler = _sapServiceSettingsFactory.CreateHandler(_sapSystem);
            var sapServiceSettings = SapServiceSettings.GetSettings(_sapConfig, _sapSystem);
            var message = new HttpRequestMessage
            {
                RequestUri = new Uri($"{sapServiceSettings.BaseServerUrl}SBOBobService_SetCurrencyRate"),
                Content = new StringContent(JsonConvert.SerializeObject(dequeuedTask.CurrencyRate),
                    Encoding.UTF8,
                    "application/json"),
                Method = HttpMethod.Post
            };

            var cookies = await sapTaskHandler.StartSession();
            message.Headers.Add("Cookie", cookies.B1Session);
            message.Headers.Add("Cookie", cookies.RouteId);

            var client = _httpClientFactory.CreateClient();
            var sapResponse = await client.SendAsync(message);
            var taskResult = new SapTaskResult
            {
                IsSuccessful = sapResponse.IsSuccessStatusCode,
                SapResponseContent = await sapResponse.Content.ReadAsStringAsync(),
                TaskName = $"Setting the {dequeuedTask.CurrencyRate.Currency} Currency Rate"
            };

            return taskResult;
        }
    }
}
