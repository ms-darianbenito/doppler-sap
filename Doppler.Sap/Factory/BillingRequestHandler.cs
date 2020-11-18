using Doppler.Sap.Enums;
using Doppler.Sap.Models;
using Doppler.Sap.Utils;
using Doppler.Sap.Validations.Billing;
using Microsoft.Extensions.Logging;
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
    public class BillingRequestHandler
    {
        private readonly ISapServiceSettingsFactory _sapServiceSettingsFactory;
        private readonly ILogger<BillingRequestHandler> _logger;
        private readonly SapConfig _sapConfig;
        private readonly IHttpClientFactory _httpClientFactory;
        //private const string _sapSystem = "AR";
        private readonly IEnumerable<IBillingValidation> _billingValidations;

        public BillingRequestHandler(
            IOptions<SapConfig> sapConfig,
            ILogger<BillingRequestHandler> logger,
            ISapServiceSettingsFactory sapServiceSettingsFactory,
            IHttpClientFactory httpClientFactory,
            IEnumerable<IBillingValidation> billingValidations)
        {
            _sapConfig = sapConfig.Value;
            _logger = logger;
            _sapServiceSettingsFactory = sapServiceSettingsFactory;
            _httpClientFactory = httpClientFactory;
            _billingValidations = billingValidations;
        }

        public async Task<SapTaskResult> Handle(SapTask dequeuedTask)
        {
            try
            {
                var sapSystem = SapSystemHelper.GetSapSystemByBillingSystem(dequeuedTask.BillingRequest.BillingSystemId);
                var sapTaskHandler = _sapServiceSettingsFactory.CreateHandler(sapSystem);
                var businessPartner = await sapTaskHandler.TryGetBusinessPartner(dequeuedTask.BillingRequest.UserId, dequeuedTask.BillingRequest.FiscalID, dequeuedTask.BillingRequest.PlanType);

                if (!GetValidator(sapSystem).CanCreate(businessPartner, dequeuedTask.BillingRequest))
                {
                    return new SapTaskResult
                    {
                        IsSuccessful = false,
                        SapResponseContent = $"Failed at generating billing request for the user: {dequeuedTask.BillingRequest.UserId}.",
                        TaskName = "Creating Billing Request"
                    };
                }

                dequeuedTask.BillingRequest.CardCode = businessPartner.CardCode;

                var serviceSetting = SapServiceSettings.GetSettings(_sapConfig, sapSystem);
                var message = new HttpRequestMessage
                {
                    RequestUri = new Uri($"{serviceSetting.BaseServerUrl}{serviceSetting.BillingConfig.Endpoint}"),
                    Content = new StringContent(JsonConvert.SerializeObject(dequeuedTask.BillingRequest),
                        Encoding.UTF8,
                        "application/json"),
                    Method = HttpMethod.Post
                };

                var cookies = await sapTaskHandler.StartSession();
                message.Headers.Add("Cookie", cookies.B1Session);
                message.Headers.Add("Cookie", cookies.RouteId);

                var client = _httpClientFactory.CreateClient();
                var sapResponse = await client.SendAsync(message);

                return new SapTaskResult
                {
                    IsSuccessful = sapResponse.IsSuccessStatusCode,
                    SapResponseContent = await sapResponse.Content.ReadAsStringAsync(),
                    TaskName = "Creating Billing Request"
                };
            }
            catch (Exception ex)
            {
                return new SapTaskResult
                {
                    IsSuccessful = false,
                    SapResponseContent = ex.Message,
                    TaskName = "Creating Billing Request"
                };
            }
        }

        private IBillingValidation GetValidator(string sapSystem)
        {
            // Check if exists billing validator for the sapSystem
            var validator = _billingValidations.FirstOrDefault(m => m.CanValidateSapSystem(sapSystem));
            if (validator == null)
            {
                _logger.LogError($"Billing Request won't be sent to SAP because the sapSystem '{sapSystem}' is not supported.");
                throw new ArgumentException(nameof(sapSystem), $"The countryCode '{sapSystem}' is not supported.");
            }

            return validator;
        }
    }
}
