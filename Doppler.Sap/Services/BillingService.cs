using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Doppler.Sap.Enums;
using Doppler.Sap.Mappers;
using Doppler.Sap.Models;
using Doppler.Sap.Utils;
using Microsoft.Extensions.Logging;

namespace Doppler.Sap.Services
{
    public class BillingService : IBillingService
    {
        private readonly IQueuingService _queuingService;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ILogger<BillingService> _logger;
        private readonly ISlackService _slackService;

        public BillingService(
            IQueuingService queuingService,
            IDateTimeProvider dateTimeProvider,
            ILogger<BillingService> logger,
            ISlackService slackService)
        {
            _queuingService = queuingService;
            _dateTimeProvider = dateTimeProvider;
            _logger = logger;
            _slackService = slackService;
        }

        public Task SendCurrencyToSap(List<CurrencyRateDto> currencyRate)
        {
            // if it is friday we must set the currency rates for the weekend and next monday
            //TODO: We can create a DateTime service for obtains days of the weekend
            var allCurrenciesRates = _dateTimeProvider.UtcNow.DayOfWeek != DayOfWeek.Friday ? currencyRate
                : currencyRate
                    .SelectMany(x =>
                        new List<CurrencyRateDto>
                        {
                            new CurrencyRateDto
                            {
                                Date = x.Date,
                                CurrencyCode = x.CurrencyCode,
                                CurrencyName = x.CurrencyName,
                                SaleValue = x.SaleValue
                            },
                            new CurrencyRateDto
                            {
                                Date = x.Date.AddDays(1),
                                CurrencyCode = x.CurrencyCode,
                                CurrencyName = x.CurrencyName,
                                SaleValue = x.SaleValue
                            },
                            new CurrencyRateDto
                            {
                                Date = x.Date.AddDays(2),
                                CurrencyCode = x.CurrencyCode,
                                CurrencyName = x.CurrencyName,
                                SaleValue = x.SaleValue
                            }
                        })
                    .ToList();

            foreach (var setCurrencyRateTask in allCurrenciesRates.Select(rate => new SapTask
            {
                CurrencyRate = CurrencyRateMapper.MapCurrencyRate(rate),
                TaskType = SapTaskEnum.CurrencyRate
            }))
            {
                _queuingService.AddToTaskQueue(setCurrencyRateTask);
            }

            return Task.CompletedTask;
        }

        public async Task CreateBillingRequest(List<BillingRequest> billingRequests)
        {
            foreach (var billing in billingRequests)
            {
                try
                {
                    ValidateBillingRequest(billing);
                    var billingRequest = SaleOrderMapper.MapDopplerBillingRequestToSapSaleOrder(billing);

                    _queuingService.AddToTaskQueue(
                        new SapTask
                        {
                            BillingRequest = billingRequest,
                            TaskType = SapTaskEnum.BillingRequest
                        }
                    );
                }
                catch (Exception e)
                {
                    _logger.LogError($"Failed at generating billing request for user: {billing.Id}. Error: {e.Message}");
                    await _slackService.SendNotification(
                        $"Failed at generating billing request for user: {billing.Id}. Error: {e.Message}");
                }
            }
        }

        private void ValidateBillingRequest(BillingRequest dopplerBillingRequest)
        {
            if (dopplerBillingRequest.Id.Equals(default))
            {
                _logger.LogError("Billing Request won't be sent to SAP because it doesn't have the user's Id.");
                throw new ArgumentException("Value can not be null", "Id");
            }
            if (string.IsNullOrEmpty(dopplerBillingRequest.FiscalID))
            {
                _logger.LogError(
                    $"Billing Request won't be sent to SAP because it doesn't have a FiscalId value. userId:{dopplerBillingRequest.Id}, planType: {dopplerBillingRequest.PlanType}");
                throw new ArgumentException("Value can not be null or empty", "FiscalId");
            }
        }
    }
}
