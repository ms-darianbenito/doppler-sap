using Doppler.Sap.Models;
using Doppler.Sap.Services;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServicesCollectionExtensions
    {
        public static void AddSapServices(this IServiceCollection services)
        {
            services.AddTransient<IBillingService, BillingService>();
            services.AddTransient<IBusinessPartnerService, BusinessPartnerService>();
        }
    }
}
