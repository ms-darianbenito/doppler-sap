using System;
using System.Threading.Tasks;
using Doppler.Sap.Enums;
using Doppler.Sap.Models;

namespace Doppler.Sap.Factory
{
    public class SapTaskFactory : ISapTaskFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public SapTaskFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<SapTaskResult> CreateHandler(SapTask sapTask)
        {
            switch (sapTask.TaskType)
            {
                case SapTaskEnum.CurrencyRate:
                    return await ((SetCurrencyRateHandler)_serviceProvider.GetService(typeof(SetCurrencyRateHandler))).Handle(sapTask);
                case SapTaskEnum.BillingRequest:
                    return await ((BillingRequestHandler)_serviceProvider.GetService(typeof(BillingRequestHandler))).Handle(sapTask);
                case SapTaskEnum.CreateOrUpdateBusinessPartner:
                    return await ((CreateOrUpdateBusinessPartnerHandler)_serviceProvider.GetService(typeof(CreateOrUpdateBusinessPartnerHandler))).Handle(sapTask);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

    }
}
