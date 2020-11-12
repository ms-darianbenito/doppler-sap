using Doppler.Sap.Factory;
using Doppler.Sap.Mappers.BusinessPartner;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SapMappersServiceCollectionExtension
    {
        public static IServiceCollection AddSapMappers(this IServiceCollection services)
        {
            services.AddTransient<IBusinessPartnerMapper, BusinessPartnerForArMapper>();
            services.AddTransient<IBusinessPartnerMapper, BusinessPartnerForUsMapper>();

            return services;

        }
    }
}
