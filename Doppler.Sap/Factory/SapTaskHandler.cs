using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Doppler.Sap.Mappers;
using Doppler.Sap.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Extensions.Options;

namespace Doppler.Sap.Factory
{
    public abstract class SapTaskHandler
    {
        protected readonly SapConfig SapConfig;
        protected SapLoginCookies SapCookies;
        private DateTime _sessionStartedAt;
        public HttpClient Client;
        protected readonly ILogger<SapTaskHandler> Logger;

        protected SapTaskHandler(IOptions<SapConfig> sapConfig, ILogger<SapTaskHandler> logger)
        {
            SapConfig = sapConfig.Value;
            Logger = logger;

            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
                UseCookies = false
            };

            Client = new HttpClient(handler);
        }

        public abstract Task<SapTaskResult> Handle(SapTask dequeuedTask);

        protected async Task StartSession()
        {
            if (SapCookies == null || DateTime.Now > _sessionStartedAt.AddMinutes(30))
            {
                try
                {
                    var sapResponse = await Client.SendAsync(new HttpRequestMessage
                    {
                        RequestUri = new Uri($"{SapConfig.BaseServerUrl}Login/"),
                        Content = new StringContent(JsonConvert.SerializeObject(
                                new SapConfig
                                {
                                    CompanyDB = SapConfig.CompanyDB,
                                    Password = SapConfig.Password,
                                    UserName = SapConfig.UserName
                                }),
                            Encoding.UTF8,
                            "application/json"),
                        Method = HttpMethod.Post
                    });
                    sapResponse.EnsureSuccessStatusCode();

                    SapCookies = new SapLoginCookies
                    {
                        B1Session = sapResponse.Headers.GetValues("Set-Cookie").Where(x => x.Contains("B1SESSION"))
                            .Select(y => y.ToString().Substring(0, 46)).FirstOrDefault(),
                        RouteId = sapResponse.Headers.GetValues("Set-Cookie").Where(x => x.Contains("ROUTEID"))
                            .Select(y => y.ToString().Substring(0, 14)).FirstOrDefault()
                    };
                    _sessionStartedAt = DateTime.Now;
                }
                catch (Exception e)
                {
                    Logger.LogError(e, "Error starting session in Sap.");
                    throw;
                }
            }
        }

        protected async Task<SapBusinessPartner> TryGetBusinessPartner(SapTask task)
        {
            var incompleteCardCode = BusinessPartnerMapper.MapDopplerUserIdToSapBusinessPartnerId(task.BillingRequest.UserId, task.BillingRequest.PlanType);

            var message = new HttpRequestMessage
            {
                RequestUri = new Uri($"{SapConfig.BaseServerUrl}BusinessPartners?$filter=startswith(CardCode,'{incompleteCardCode}')"),
                Method = HttpMethod.Get
            };

            message.Headers.Add("Cookie", SapCookies.B1Session);
            message.Headers.Add("Cookie", SapCookies.RouteId);

            var sapResponse = await Client.SendAsync(message);
            // Should throw error because if the business partner doesn't exists it returns an empty json.
            sapResponse.EnsureSuccessStatusCode();

            var businessPartnersList = JsonConvert.DeserializeObject<SapBusinessPartnerList>(await sapResponse.Content.ReadAsStringAsync());
            var businessPartner = businessPartnersList.value.FirstOrDefault(x => x.FederalTaxID == task.BillingRequest.FiscalID);

            return businessPartner;
        }
    }
}
