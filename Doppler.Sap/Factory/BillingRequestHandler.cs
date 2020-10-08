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
    public class BillingRequestHandler : SapTaskHandler
    {
        public BillingRequestHandler(IOptions<SapConfig> sapConfig, ILogger<SapTaskHandler> logger)
            : base(sapConfig, logger) { }

        public override async Task<SapTaskResult> Handle(SapTask dequeuedTask)
        {
            await StartSession();

            var businessPartner = await TryGetBusinessPartner(dequeuedTask);

            if (businessPartner == null)
            {
                Logger.LogError($"Failed at generating billing request for user: {dequeuedTask.BillingRequest.UserId}.");
                return null;
            }

            if (string.IsNullOrEmpty(businessPartner.FederalTaxID))
            {
                Logger.LogError(
                    $"Can not create billing request for user id : {dequeuedTask.BillingRequest.UserId}, FiscalId: {dequeuedTask.BillingRequest.FiscalID} and user type: {dequeuedTask.BillingRequest.PlanType}");
                return null;
            }

            dequeuedTask.BillingRequest.CardCode = businessPartner.CardCode;

            var message = new HttpRequestMessage
            {
                RequestUri = new Uri($"{SapConfig.BaseServerUrl}Orders"),
                Content = new StringContent(JsonConvert.SerializeObject(dequeuedTask.BillingRequest),
                    Encoding.UTF8,
                    "application/json"),
                Method = HttpMethod.Post
            };

            message.Headers.Add("Cookie", SapCookies.B1Session);
            message.Headers.Add("Cookie", SapCookies.RouteId);

            var sapResponse = await Client.SendAsync(message);

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
