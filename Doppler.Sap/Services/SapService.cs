using Doppler.Sap.Factory;
using Doppler.Sap.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Doppler.Sap.Services
{
    public class SapService : ISapService
    {
        private readonly ISapTaskFactory _sapTaskFactory;
        private readonly HttpClient _client;
        private readonly SapConfig _sapConfig;
        private SapLoginCookies _sapCookies;
        private DateTime _sessionStartedAt;

        public SapService(ISapTaskFactory sapTaskFactory,
            IOptions<SapConfig> sapConfig,
            IHttpClientFactory httpClientFactory)
        {
            _sapTaskFactory = sapTaskFactory;
            _sapConfig = sapConfig.Value;
            _client = httpClientFactory.CreateClient();
        }

        public async Task<SapTaskResult> SendToSap(SapTask dequeuedTask)
        {
            return await _sapTaskFactory.CreateHandler(dequeuedTask);
        }

        public async Task<SapBusinessPartner> TryGetBusinessPartnerByCardCode(string cardCode)
        {
            await StartSession();

            var message = new HttpRequestMessage()
            {
                RequestUri = new Uri($"{_sapConfig.BaseServerUrl}BusinessPartners('{cardCode}')"),
                Method = HttpMethod.Get
            };
            message.Headers.Add("Cookie", _sapCookies.B1Session);
            message.Headers.Add("Cookie", _sapCookies.RouteId);

            var sapResponse = await _client.SendAsync(message);

            if (sapResponse.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<SapBusinessPartner>((await sapResponse.Content.ReadAsStringAsync()));
            }
            return null;
        }

        private async Task StartSession()
        {
            if (_sapCookies == null || (DateTime.Now > _sessionStartedAt.AddMinutes(30)))
            {
                var sapResponse = await _client.SendAsync(new HttpRequestMessage
                {
                    RequestUri = new Uri($"{_sapConfig.BaseServerUrl}Login/"),
                    Content = new StringContent(JsonConvert.SerializeObject(new SapConfig
                    {
                        CompanyDB = _sapConfig.CompanyDB,
                        Password = _sapConfig.Password,
                        UserName = _sapConfig.UserName
                    })
                        , Encoding.UTF8
                        , "application/json"),
                    Method = HttpMethod.Post,
                });
                if (sapResponse.IsSuccessStatusCode)
                {
                    _sapCookies = new SapLoginCookies
                    {
                        B1Session = sapResponse.Headers.GetValues("Set-Cookie").Where(x => x.Contains("B1SESSION")).Select(y => y.ToString().Substring(0, 46)).FirstOrDefault(),
                        RouteId = sapResponse.Headers.GetValues("Set-Cookie").Where(x => x.Contains("ROUTEID")).Select(y => y.ToString().Substring(0, 14)).FirstOrDefault()
                    };
                    _sessionStartedAt = DateTime.Now;
                    return;
                }
                else
                {
                    throw new UnauthorizedAccessException();
                }
            }
            else
            {
                return;
            }
        }

        public async Task<SapBusinessPartner> TryGetBusinessPartner(string cardCode, string cuit)
        {
            await StartSession();

            var message = new HttpRequestMessage()
            {
                RequestUri = new Uri($"{_sapConfig.BaseServerUrl}BusinessPartners?$filter=startswith(CardCode,'{cardCode}')"),
                Method = HttpMethod.Get
            };
            message.Headers.Add("Cookie", _sapCookies.B1Session);
            message.Headers.Add("Cookie", _sapCookies.RouteId);

            var sapResponse = await _client.SendAsync(message);
            // Should throw error because if the business partner doesn't exists it returns an empty json.
            sapResponse.EnsureSuccessStatusCode();

            var businessPartnersList = JsonConvert.DeserializeObject<SapBusinessPartnerList>((await sapResponse.Content.ReadAsStringAsync()));
            var businessPartner = businessPartnersList.value
                .Where(x => x.FederalTaxID == cuit)
                .FirstOrDefault();

            if (businessPartner == null)
            {
                var amountBusinessPartnersForSameUser = businessPartnersList.value.Count();
                if (amountBusinessPartnersForSameUser >= _sapConfig.MaxAmountAllowedAccounts)
                {
                    throw new ArgumentOutOfRangeException("User can't have more than 10 accounts in Sap");
                }
                businessPartner = new SapBusinessPartner()
                {
                    CardCode = $"{cardCode}{amountBusinessPartnersForSameUser}"
                };
            }
            return businessPartner;
        }
    }
}
