using Doppler.Sap.Mappers;
using Doppler.Sap.Models;
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
        private readonly ISapTaskHandler _sapTaskHandler;
        private readonly IHttpClientFactory _httpClientFactory;

        public CreateOrUpdateBusinessPartnerHandler(
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
            dequeuedTask = await _sapTaskHandler.CreateBusinessPartnerFromDopplerUser(dequeuedTask);

            return string.IsNullOrEmpty(dequeuedTask.ExistentBusinessPartner.FederalTaxID) ?
                await CreateBusinessPartner(dequeuedTask) :
                await UpdateBusinessPartner(dequeuedTask);
        }

        private async Task<SapTaskResult> CreateBusinessPartner(SapTask dequeuedTask)
        {
            var message = new HttpRequestMessage()
            {
                RequestUri = new Uri($"{_sapConfig.BaseServerUrl}BusinessPartners/"),
                Content = new StringContent(JsonConvert.SerializeObject(dequeuedTask.BusinessPartner),
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
                TaskName = "Creating Business Partner"
            };

            return taskResult;
        }

        private async Task<SapTaskResult> UpdateBusinessPartner(SapTask dequeuedTask)
        {
            //SAP uses a non conventional patch where you have to send only the fields that you want to be changed with the new values
            dequeuedTask.BusinessPartner.BPAddresses = GetBPAddressesPatchObject(dequeuedTask.BusinessPartner.BPAddresses);
            dequeuedTask.BusinessPartner.ContactEmployees = GetContactEmployeesPatchObject(dequeuedTask.BusinessPartner.ContactEmployees, dequeuedTask.ExistentBusinessPartner.ContactEmployees);
            //we don't want to update: CUITs/DNI and Currency
            dequeuedTask.BusinessPartner.FederalTaxID = null;
            dequeuedTask.BusinessPartner.Currency = null;

            var sapResponse = await SendMessage(dequeuedTask);

            var taskResult = new SapTaskResult
            {
                IsSuccessful = sapResponse.IsSuccessStatusCode,
                SapResponseContent = await sapResponse.Content.ReadAsStringAsync(),
                TaskName = "Updating Business Partner"
            };

            return taskResult;
        }

        private async Task<HttpResponseMessage> SendMessage(SapTask dequeuedTask)
        {
            var message = new HttpRequestMessage()
            {
                RequestUri = new Uri($"{_sapConfig.BaseServerUrl}BusinessPartners('{dequeuedTask.ExistentBusinessPartner.CardCode}')"),
                Content = new StringContent(JsonConvert.SerializeObject(dequeuedTask.BusinessPartner,
                    new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    }),
                    Encoding.UTF8,
                    "application/json"),
                Method = HttpMethod.Patch
            };

            var cookies = await _sapTaskHandler.StartSession();
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
