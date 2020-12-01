using Doppler.Sap.Models;
using Doppler.Sap.Services;
using System;
using System.IO;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SapBillingItemsServiceCollectionExtension
    {
        public static IServiceCollection AddSapBillingItems(this IServiceCollection services)
        {
            var jsonPath = Path.Combine(Environment.CurrentDirectory, @"Resources/SapBillingItems.json");
            var itemsList = Newtonsoft.Json.JsonConvert.DeserializeObject<System.Collections.Generic.List<SapBillingItemModel>>(File.ReadAllText(jsonPath));
            services.AddSingleton(itemsList);

            services.AddSingleton<ISapBillingItemsService, SapBillingItemsService>();

            return services;

        }
    }
}
