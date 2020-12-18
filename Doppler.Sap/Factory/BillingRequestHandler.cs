using Doppler.Sap.Mappers.Billing;
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
        private readonly IEnumerable<IBillingMapper> _billingMappers;
        private readonly IEnumerable<IBillingValidation> _billingValidations;

        public BillingRequestHandler(
            IOptions<SapConfig> sapConfig,
            ILogger<BillingRequestHandler> logger,
            ISapServiceSettingsFactory sapServiceSettingsFactory,
            IHttpClientFactory httpClientFactory,
            IEnumerable<IBillingValidation> billingValidations,
            IEnumerable<IBillingMapper> billingMappers)
        {
            _sapConfig = sapConfig.Value;
            _logger = logger;
            _sapServiceSettingsFactory = sapServiceSettingsFactory;
            _httpClientFactory = httpClientFactory;
            _billingMappers = billingMappers;
            _billingValidations = billingValidations;
        }

        public async Task<SapTaskResult> Handle(SapTask dequeuedTask)
        {
            try
            {
                var sapSystem = SapSystemHelper.GetSapSystemByBillingSystem(dequeuedTask.BillingRequest.BillingSystemId);
                var sapTaskHandler = _sapServiceSettingsFactory.CreateHandler(sapSystem);
                var businessPartner = await sapTaskHandler.TryGetBusinessPartner(dequeuedTask.BillingRequest.UserId, dequeuedTask.BillingRequest.FiscalID, dequeuedTask.BillingRequest.PlanType);

                var billingValidator = GetValidator(sapSystem);
                if (!billingValidator.CanCreate(businessPartner, dequeuedTask.BillingRequest))
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
                var responseContent = await sapResponse.Content.ReadAsStringAsync();

                if (sapResponse.IsSuccessStatusCode)
                {
                    if (serviceSetting.BillingConfig.NeedCreateIncomingPayments &&
                        dequeuedTask.BillingRequest.TransactionApproved)
                    {
                        responseContent = await sapResponse.Content.ReadAsStringAsync();
                        var response = JsonConvert.DeserializeObject<SapSaleOrderInvoiceResponse>(responseContent);
                        return await SendIncomingPaymentToSap(serviceSetting, sapSystem, response, dequeuedTask.BillingRequest.TransferReference, cookies);
                    }
                }
                else
                {
                    _logger.LogError($"Invoice/Sales Order could'n create to SAP because exists an error: '{responseContent}'.");
                }

                return new SapTaskResult
                {
                    IsSuccessful = sapResponse.IsSuccessStatusCode,
                    SapResponseContent = responseContent,
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

        private async Task<SapTaskResult> SendIncomingPaymentToSap(SapServiceConfig serviceSetting, string sapSystem, SapSaleOrderInvoiceResponse response, string transferReference, SapLoginCookies cookies)
        {
            var billingMapper = GetMapper(sapSystem);
            var incomingPaymentRequest = billingMapper.MapSapIncomingPayment(response.DocEntry, response.CardCode, response.DocTotal, response.DocDate, transferReference);

            var message = new HttpRequestMessage
            {
                RequestUri = new Uri($"{serviceSetting.BaseServerUrl}{serviceSetting.BillingConfig.IncomingPaymentsEndpoint}"),
                Content = new StringContent(JsonConvert.SerializeObject(incomingPaymentRequest), Encoding.UTF8, "application/json"),
                Method = HttpMethod.Post
            };

            message.Headers.Add("Cookie", cookies.B1Session);
            message.Headers.Add("Cookie", cookies.RouteId);

            var client = _httpClientFactory.CreateClient();
            var sapResponse = await client.SendAsync(message);

            if (!sapResponse.IsSuccessStatusCode)
            {
                _logger.LogError($"Incoming Payment could'n create to SAP because exists an error: '{sapResponse.Content.ReadAsStringAsync()}'.");
            }

            return new SapTaskResult
            {
                IsSuccessful = sapResponse.IsSuccessStatusCode,
                SapResponseContent = await sapResponse.Content.ReadAsStringAsync(),
                TaskName = "Creating Billing Request"
            };
        }

        private IBillingMapper GetMapper(string sapSystem)
        {
            // Check if exists business partner mapper for the sapSystem
            var mapper = _billingMappers.FirstOrDefault(m => m.CanMapSapSystem(sapSystem));
            if (mapper == null)
            {
                _logger.LogError($"Billing Request won't be sent to SAP because the sapSystem '{sapSystem}' is not supported.");
                throw new ArgumentException(nameof(sapSystem), $"The sapSystem '{sapSystem}' is not supported.");
            }

            return mapper;
        }

        private IBillingValidation GetValidator(string sapSystem)
        {
            // Check if exists billing validator for the sapSystem
            var validator = _billingValidations.FirstOrDefault(m => m.CanValidateSapSystem(sapSystem));
            if (validator == null)
            {
                _logger.LogError($"Billing Request won't be sent to SAP because the sapSystem '{sapSystem}' is not supported.");
                throw new ArgumentException(nameof(sapSystem), $"The sapSystem '{sapSystem}' is not supported.");
            }

            return validator;
        }
    }
}
