using Doppler.Sap.Models;
using Doppler.Sap.Utils;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Doppler.Sap.Factory
{
    public class CreateOrUpdateBusinessPartnerHandler
    {
        private readonly SapConfig _sapConfig;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ISapServiceSettingsFactory _sapServiceSettingsFactory;

        public CreateOrUpdateBusinessPartnerHandler(
            IOptions<SapConfig> sapConfig,
            IHttpClientFactory httpClientFactory,
            ISapServiceSettingsFactory sapServiceSettingsFactory)
        {
            _sapConfig = sapConfig.Value;
            _httpClientFactory = httpClientFactory;
            _sapServiceSettingsFactory = sapServiceSettingsFactory;
        }

        public async Task<SapTaskResult> Handle(SapTask dequeuedTask)
        {
            var sapTaskHandler = _sapServiceSettingsFactory.CreateHandler(dequeuedTask.DopplerUser.BillingCountryCode);
            dequeuedTask = await sapTaskHandler.CreateBusinessPartnerFromDopplerUser(dequeuedTask);

            return string.IsNullOrEmpty(dequeuedTask.ExistentBusinessPartner.FederalTaxID) ?
                await CreateBusinessPartner(dequeuedTask) :
                await UpdateBusinessPartner(dequeuedTask);
        }

        private async Task<SapTaskResult> CreateBusinessPartner(SapTask dequeuedTask)
        {
            var countryCode = dequeuedTask.BusinessPartner.BPAddresses.FirstOrDefault()?.Country ?? string.Empty;
            var serviceSetting = SapServiceSettings.GetSettings(_sapConfig, countryCode);
            var uriString = $"{serviceSetting.BaseServerUrl}{serviceSetting.BusinessPartnerConfig.Endpoint}/";
            var sapResponse = await SendMessage(dequeuedTask.BusinessPartner, uriString, HttpMethod.Post);

            var taskResult = new SapTaskResult
            {
                IsSuccessful = sapResponse.IsSuccessStatusCode,
                SapResponseContent = await sapResponse.Content.ReadAsStringAsync(),
                TaskName = "Creating Business Partner"
            };

            return taskResult;
        }

        private async Task<SapTaskResult> UpdateBusinessPartner(SapTask dequeuedTask)
        {
            var countryCode = dequeuedTask.BusinessPartner.BPAddresses.FirstOrDefault() != null ? dequeuedTask.BusinessPartner.BPAddresses.FirstOrDefault()?.Country : string.Empty;
            var serviceSetting = SapServiceSettings.GetSettings(_sapConfig, countryCode);

            //SAP uses a non conventional patch where you have to send only the fields that you want to be changed with the new values
            dequeuedTask.BusinessPartner.BPAddresses = GetBPAddressesPatchObject(dequeuedTask.BusinessPartner.BPAddresses);
            dequeuedTask.BusinessPartner.ContactEmployees = GetContactEmployeesPatchObject(dequeuedTask.BusinessPartner.ContactEmployees, dequeuedTask.ExistentBusinessPartner.ContactEmployees);
            //we don't want to update: CUITs/DNI and Currency
            dequeuedTask.BusinessPartner.FederalTaxID = null;
            dequeuedTask.BusinessPartner.Currency = null;

            var uriString = $"{serviceSetting.BaseServerUrl}{serviceSetting.BusinessPartnerConfig.Endpoint}('{dequeuedTask.ExistentBusinessPartner.CardCode}')";
            var sapResponse = await SendMessage(dequeuedTask.BusinessPartner, uriString, HttpMethod.Patch);

            var taskResult = new SapTaskResult
            {
                IsSuccessful = sapResponse.IsSuccessStatusCode,
                SapResponseContent = await sapResponse.Content.ReadAsStringAsync(),
                TaskName = "Updating Business Partner"
            };

            return taskResult;
        }

        private async Task<HttpResponseMessage> SendMessage(SapBusinessPartner businessPartner, string uriString, HttpMethod method)
        {
            var message = new HttpRequestMessage()
            {
                RequestUri = new Uri(uriString),
                Content = new StringContent(JsonConvert.SerializeObject(businessPartner,
                    new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    }),
                    Encoding.UTF8,
                    "application/json"),
                Method = method
            };

            var countryCode = businessPartner.BPAddresses.FirstOrDefault() != null ? businessPartner.BPAddresses.FirstOrDefault().Country : string.Empty;
            var sapTaskHandler = _sapServiceSettingsFactory.CreateHandler(countryCode);
            var cookies = await sapTaskHandler.StartSession();
            message.Headers.Add("Cookie", cookies.B1Session);
            message.Headers.Add("Cookie", cookies.RouteId);

            var client = _httpClientFactory.CreateClient();
            return await client.SendAsync(message);
        }

        private List<Address> GetBPAddressesPatchObject(List<Address> bPAddresses)
        {
            bPAddresses.ForEach(x => x.AddressName = null);
            return bPAddresses;
        }

        private List<SapContactEmployee> GetContactEmployeesPatchObject(List<SapContactEmployee> newContactEmployeeList, List<SapContactEmployee> existentContactEmployeeList)
        {
            var updatesOnContactEmployee = newContactEmployeeList
                //new CEs
                .Where(x => !existentContactEmployeeList
                        .Select(y => y.E_Mail)
                        .Contains(x.E_Mail))
                //deactivate existent CEs
                .Union(existentContactEmployeeList
                    .Where(x => !newContactEmployeeList
                        .Select(y => y.E_Mail)
                        .Contains(x.E_Mail))
                    .Select(x => new SapContactEmployee
                    {
                        Active = "tNO",
                        InternalCode = x.InternalCode
                    }))
                //reactivate existent CEs
                .Union(existentContactEmployeeList
                    .Where(x => newContactEmployeeList
                        .Select(y => y.E_Mail)
                        .Contains(x.E_Mail))
                    .Select(x => new SapContactEmployee
                    {
                        Active = "tYES",
                        InternalCode = x.InternalCode
                    }))
                .ToList();

            return updatesOnContactEmployee;
        }
    }
}
