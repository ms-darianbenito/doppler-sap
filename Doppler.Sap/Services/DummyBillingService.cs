using Doppler.Sap.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Doppler.Sap.Services
{
    public class DummyBillingService : IBillingService
    {
        public Task SendCurrencyToSap(List<CurrencyRateDto> currencyRate)
        {
            return Task.CompletedTask;
        }

        public Task CreateBillingRequest(List<BillingRequest> billingRequests)
        {
            return Task.CompletedTask;
        }
    }
}
