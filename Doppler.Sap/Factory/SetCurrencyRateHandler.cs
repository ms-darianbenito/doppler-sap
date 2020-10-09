using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Doppler.Sap.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;

namespace Doppler.Sap.Factory
{
    public class SetCurrencyRateHandler
    {
        private readonly ISapTaskHandler _sapTaskHandler;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly SapConfig _sapConfig;

        public SetCurrencyRateHandler(
            IOptions<SapConfig> sapConfig,
            ISapTaskHandler sapTaskHandler,
            IHttpClientFactory httpClientFactory)
        {
            _sapConfig = sapConfig.Value;
            _sapTaskHandler = sapTaskHandler;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<SapTaskResult> Handle(SapTask dequeuedTask)
        {
            var message = new HttpRequestMessage
            {
                RequestUri = new Uri($"{_sapConfig.BaseServerUrl}SBOBobService_SetCurrencyRate"),
                Content = new StringContent(JsonConvert.SerializeObject(dequeuedTask.CurrencyRate),
                    Encoding.UTF8,
                    "application/json"),
                Method = HttpMethod.Post
            };

            var cookies = await _sapTaskHandler.StartSession();
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
