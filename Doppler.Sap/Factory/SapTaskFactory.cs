using System;
using Doppler.Sap.Enums;

namespace Doppler.Sap.Factory
{
    public class SapTaskFactory : ISapTaskFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public SapTaskFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public SapTaskHandler CreateHandler(SapTaskEnum sapTask)
        {
            switch (sapTask)
            {
                case SapTaskEnum.CurrencyRate:
                    return (SetCurrencyRateHandler)_serviceProvider.GetService(typeof(SetCurrencyRateHandler));
                case SapTaskEnum.BillingRequest:
                    return (BillingRequestHandler)_serviceProvider.GetService(typeof(BillingRequestHandler));
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
