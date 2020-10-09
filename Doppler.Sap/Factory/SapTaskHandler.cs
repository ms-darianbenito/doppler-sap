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
    public class SapTaskHandler : ISapTaskHandler
    {
        private readonly SapConfig _sapConfig;
        private SapLoginCookies _sapCookies;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<SapTaskHandler> _logger;

        public SapTaskHandler(IOptions<SapConfig> sapConfig, ILogger<SapTaskHandler> logger, IHttpClientFactory httpClientFactory)
        {
            _sapConfig = sapConfig.Value;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<SapLoginCookies> StartSession()
        {
            if (_sapCookies == null)
            {
                try
                {
                    var client = _httpClientFactory.CreateClient();
                    var sapResponse = await client.SendAsync(new HttpRequestMessage
                    {
                        RequestUri = new Uri($"{_sapConfig.BaseServerUrl}Login/"),
                        Content = new StringContent(JsonConvert.SerializeObject(
                                new SapConfig
                                {
                                    CompanyDB = _sapConfig.CompanyDB,
                                    Password = _sapConfig.Password,
                                    UserName = _sapConfig.UserName
                                }),
                            Encoding.UTF8,
                            "application/json"),
                        Method = HttpMethod.Post
                    });
                    sapResponse.EnsureSuccessStatusCode();

                    _sapCookies = new SapLoginCookies
                    {
                        B1Session = sapResponse.Headers.GetValues("Set-Cookie").Where(x => x.Contains("B1SESSION"))
                            .Select(y => y.ToString().Substring(0, 46)).FirstOrDefault(),
                        RouteId = sapResponse.Headers.GetValues("Set-Cookie").Where(x => x.Contains("ROUTEID"))
                            .Select(y => y.ToString().Substring(0, 14)).FirstOrDefault()
                    };

                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error starting session in Sap.");
                    throw;
                }
            }

            return _sapCookies;
        }

        public async Task<SapBusinessPartner> TryGetBusinessPartner(SapTask task)
        {
            var incompleteCardCode = BusinessPartnerMapper.MapDopplerUserIdToSapBusinessPartnerId(task.BillingRequest.UserId, task.BillingRequest.PlanType);

            var message = new HttpRequestMessage
            {
                RequestUri = new Uri($"{_sapConfig.BaseServerUrl}BusinessPartners?$filter=startswith(CardCode,'{incompleteCardCode}')"),
                Method = HttpMethod.Get
            };

            var cookies = await StartSession();
            message.Headers.Add("Cookie", cookies.B1Session);
            message.Headers.Add("Cookie", cookies.RouteId);

            var client = _httpClientFactory.CreateClient();
            var sapResponse = await client.SendAsync(message);
            // Should throw error because if the business partner doesn't exists it returns an empty json.
            sapResponse.EnsureSuccessStatusCode();

            var businessPartnersList = JsonConvert.DeserializeObject<SapBusinessPartnerList>(await sapResponse.Content.ReadAsStringAsync());
            var businessPartner = businessPartnersList.value.FirstOrDefault(x => x.FederalTaxID == task.BillingRequest.FiscalID);

            return businessPartner;
        }
    }
}
