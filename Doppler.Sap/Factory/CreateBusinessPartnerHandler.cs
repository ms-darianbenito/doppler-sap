using Doppler.Sap.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Doppler.Sap.Factory
{
    public class CreateBusinessPartnerHandler
    {
        private readonly SapConfig _sapConfig;
        private readonly ISapTaskHandler _sapTaskHandler;
        private readonly IHttpClientFactory _httpClientFactory;

        public CreateBusinessPartnerHandler(
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
            var message = new HttpRequestMessage()
            {
                RequestUri = new Uri($"{_sapConfig.BaseServerUrl}BusinessPartners/"),
                Content = new StringContent(JsonConvert.SerializeObject(dequeuedTask.BusinessPartner)
                        , Encoding.UTF8
                        , "application/json"),
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
                TaskName = "Creating Business Partner"
            };

            return taskResult;
        }
    }
}
