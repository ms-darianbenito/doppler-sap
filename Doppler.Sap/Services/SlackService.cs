using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Doppler.Sap.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Doppler.Sap.Services
{
    public class SlackService : ISlackService
    {
        private readonly SapConfig _sapConfig;
        private readonly IHttpClientFactory _httpClientFactory;

        public SlackService(IOptions<SapConfig> sapConfig, IHttpClientFactory httpClientFactory)
        {
            _sapConfig = sapConfig.Value;
            _httpClientFactory = httpClientFactory;
        }

        public async Task SendNotification(string message)
        {
            var httpClient = _httpClientFactory.CreateClient();
            await httpClient.SendAsync(new HttpRequestMessage
            {
                RequestUri = new Uri(_sapConfig.SlackAlertUrl),
                Content = new StringContent(JsonConvert.SerializeObject(new { text = message }),
                    Encoding.UTF8,
                    "application/json"),
                Method = HttpMethod.Post
            });
        }
    }
}
