using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Doppler.Sap.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Doppler.Sap.Factory
{
    public class BillingRequestHandler
    {
        private readonly ISapTaskHandler _sapTaskHandler;
        private readonly ILogger<BillingRequestHandler> _logger;
        private readonly SapConfig _sapConfig;
        private readonly IHttpClientFactory _httpClientFactory;

        public BillingRequestHandler(
            IOptions<SapConfig> sapConfig,
            ILogger<BillingRequestHandler> logger,
            ISapTaskHandler sapTaskHandler,
            IHttpClientFactory httpClientFactory)
        {
            _sapConfig = sapConfig.Value;
            _logger = logger;
            _sapTaskHandler = sapTaskHandler;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<SapTaskResult> Handle(SapTask dequeuedTask)
        {
            var businessPartner = await _sapTaskHandler.TryGetBusinessPartner(dequeuedTask);

            if (businessPartner == null)
            {
                _logger.LogError($"Failed at generating billing request for user: {dequeuedTask.BillingRequest.UserId}.");
                return null;
            }

            if (string.IsNullOrEmpty(businessPartner.FederalTaxID))
            {
                _logger.LogError(
                    $"Can not create billing request for user id : {dequeuedTask.BillingRequest.UserId}, FiscalId: {dequeuedTask.BillingRequest.FiscalID} and user type: {dequeuedTask.BillingRequest.PlanType}");
                return null;
            }

            dequeuedTask.BillingRequest.CardCode = businessPartner.CardCode;

            var message = new HttpRequestMessage
            {
                RequestUri = new Uri($"{_sapConfig.BaseServerUrl}Orders"),
                Content = new StringContent(JsonConvert.SerializeObject(dequeuedTask.BillingRequest),
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
                TaskName = "Creating Billing Request"
            };

            return taskResult;
        }
    }
}
