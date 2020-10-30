using Doppler.Sap.Models;
using Doppler.Sap.Services;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServicesCollectionExtensions
    {
        public static void AddSapServices(this IServiceCollection services)
        {
            services.AddSingleton<DummyBillingService>();
            services.AddTransient<BillingService>();
            services.AddSingleton<DummyBusinessPartnerService>();
            services.AddTransient<BusinessPartnerService>();

            services.AddTransient<IBillingService>(serviceProvider =>
            {
                var sapProviderOptions = serviceProvider.GetRequiredService<IOptions<SapConfig>>();

                return sapProviderOptions.Value.UseDummyData
                    ? (IBillingService)serviceProvider.GetRequiredService<DummyBillingService>()
                    : serviceProvider.GetRequiredService<BillingService>();
            });

            services.AddTransient<IBusinessPartnerService>(serviceProvider =>
            {
                var sapProviderOptions = serviceProvider.GetRequiredService<IOptions<SapConfig>>();

                return sapProviderOptions.Value.UseDummyData
                    ? (IBusinessPartnerService)serviceProvider.GetRequiredService<DummyBusinessPartnerService>()
                    : serviceProvider.GetRequiredService<BusinessPartnerService>();
            });
        }
    }
}
